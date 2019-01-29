using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace espn
{
    public class DbManager
    {
        private static int _nextGamePk = -1;
        private static readonly Mutex Mutex = new Mutex();

        public static int GetNextGamePk()
        {
            int pk;
            Mutex.WaitOne();
            try
            {
                if (_nextGamePk == -1)
                {
                    using (var db = new EspnEntities())
                    {
                        _nextGamePk = db.Games.Max(g => g.Pk) + 1;
                        pk = _nextGamePk;
                    }
                }
                else
                {
                    pk = ++_nextGamePk;
                }
            }
            catch (Exception)
            {
                pk = -1;
            }
            finally
            {
                Mutex.ReleaseMutex();
            }
            return pk;
        }

        public static void AddNewPlayer(string name, int id = -1)
        {
            try
            {
                Console.WriteLine("Add New Player - " + name);
                if (id == -1)
                    id = PlayerInfo.GetPlayerId(name);
                if (id == -1)
                    id = int.Parse(Microsoft.VisualBasic.Interaction.InputBox("Can't Found Player, Please Insert Id", "Add New Player", "Default", -1, -1));

                using (var db = new EspnEntities())
                {
                    var names = db.Players.Select(p => p.Name);
                    var ids = db.Players.Select(p => p.ID);

                    if (names.Contains(name) || ids.Contains(id))
                    {

                        Console.WriteLine("Player Already Exist");
                        return;
                    }

                    int gamePk = db.Games.Max(g => g.Pk);
                    var playerInfo = new PlayerInfo(name, id);

                    Player player = new Player(playerInfo);
                    Game[] games = playerInfo.Games.Select(g => new Game(g, ++gamePk, player.ID)).ToArray();

                    db.Players.Add(player);
                    db.Games.AddRange(games);
                    db.SaveChanges();

                    Console.WriteLine("Done");
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public static void UpdatePlayers(LogDelegate log = null)
        {
            var players = MainForm.PlayerRater.Players;
            var startTime = DateTime.Now;
            int currentYear = Utils.GetCurrentYear();

            log?.Invoke("Start Crate Rater...");
            if (!Utils.Ping(@"http://www.espn.com"))
            {
                Console.WriteLine("No Ping");
                log?.Invoke("No Internet, Can't Update");
                return;
            }

            double counter = 0;
            IEnumerable<PlayerInfo> playerInfos = players.AsParallel().Select(p =>
            {
                log?.Invoke($"Download - {Math.Round(100 * ++counter / players.Count())} %");
                Console.WriteLine(Math.Round(100 * counter / players.Count()) + "%");
                return new PlayerInfo(p.Name, p.ID, currentYear + 1);
            });

            foreach (PlayerInfo playerInfo in playerInfos)
            {
                log?.Invoke("Update - " + playerInfo.PlayerName);
                if (playerInfo?.Games?.Count != 0)
                {
                    UpdatePlayer(playerInfo);
                }
            }
            Console.WriteLine(Environment.NewLine + "Done In " + (DateTime.Now - startTime).TotalSeconds + " Seconds");

            using (var db = new EspnEntities())
            {
                List<Game> games = new List<Game>(db.Games.AsEnumerable().Where(g => g.GameDate > new DateTime(currentYear, 10, 1)));
                MainForm.PlayerRater.Games = new List<Game>(games);
            }
        }

        private static void UpdatePlayer(PlayerInfo playerInfo)
        {
            try
            {
                using (var db = new EspnEntities())
                {
                    Player player = db.Players.FirstOrDefault(p => p.ID == playerInfo.Id);
                    if (player == null) return;


                    player.Age = playerInfo.Age;
                    player.Misc = playerInfo.Misc;
                    player.Team = playerInfo.Team;


                    Game lastGame = db.Games.Where(g => g.PlayerId == player.ID).OrderByDescending(g => g.GameDate).FirstOrDefault();

                    if (lastGame != null)
                    {
                        playerInfo.Games.FirstOrDefault(g => g.GameDate == lastGame.GameDate)
                            ?.UpdateGame(lastGame); //Update Last Game In DB
                        db.Games.AddOrUpdate(lastGame);


                        foreach (GameStats gameStats in playerInfo.Games.Where(g => g.GameDate > lastGame.GameDate)) //Update Rest Of The Games
                        {
                            var game = new Game(gameStats, GetNextGamePk(), player.ID);
                            db.Games.Add(game);
                        }
                    }

                    db.SaveChanges();
                }
                Console.WriteLine(playerInfo.PlayerName + " - Success");
            }
            catch (Exception e)
            {
                Console.WriteLine(playerInfo.PlayerName + " - " + e.Message);
            }
        }

        public static int[] GetIdByType(string type)
        {
            int val = 0;
            switch (type)
            {
                case "Roster":
                    val = 1;
                    break;
                case "Watch":
                    val = 2;
                    break;
            }


            using (var db = new EspnEntities())
            {
                var playres = db.Players.Where(p => p.Type.HasValue && p.Type.Value == val);
                if (playres.Any())
                    return playres.Select(p => p.ID).ToArray();
                else
                    return new int[0];
            }
        }

        public static string[] GetTeamList()
        {
            using (var db = new EspnEntities())
            {
                IQueryable<string> teams = db.Players.Select(p => p.Team).Where(t => t != null && !t.Contains("null"))
                    .Distinct().OrderBy(t => t);
                return teams.ToArray();
            }
        }


    }
}
