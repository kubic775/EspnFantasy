using System;
using System.Collections.Generic;
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

        public static void CreateTables()
        {
            using (var db = new EspnEntities())
            {
                int gamePk = db.Games.Max(g => g.Pk);

                foreach (Player player in db.Players)
                {
                    Console.WriteLine(player.Name);
                    if (player.Age != null) continue;
                    var playerInfo = new PlayerInfo(player.Name, player.ID);
                    player.Age = playerInfo.Age;
                    player.Team = playerInfo.Team;
                    player.Misc = playerInfo.Misc;

                    foreach (GameStats gameStats in playerInfo.Games)
                    {
                        var game = new Game(gameStats, ++gamePk, player.ID);
                        db.Games.Add(game);
                    }
                    db.SaveChanges();
                }
            }
        }

        public static void AddNewPlayersFromFile(string filePath)
        {
            string[] names = File.ReadAllLines(filePath).Select(p => p.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[0]).ToArray();
            int[] ids = File.ReadAllLines(filePath).Select(p => int.Parse(p.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[1])).ToArray();

            for (int i = 0; i < ids.Length; i++)
            {
                AddNewPlayer(names[i], ids[i]);
            }
        }

        public static void AddNewPlayer(string name, int id = -1)
        {
            try
            {
                if (id == -1)
                    id = PlayerInfo.GetPlayerId(name);
                if (id == -1 )
                    id = int.Parse(Microsoft.VisualBasic.Interaction.InputBox("Can't Found Player, Please Insert Id", "Add New Player", "Default", -1, -1));

                using (var db = new EspnEntities())
                {
                    if (db.Players.Any(p => p.Name.Equals(name) || p.ID == id))
                    {
                        id = int.Parse(Microsoft.VisualBasic.Interaction.InputBox("Can't Found Player, Please Insert Id", "Add New Player", "-1"));
                        if (id == -1) return;
                    }

                    int gamePk = db.Games.Max(g => g.Pk);
                    var playerInfo = new PlayerInfo(name, id);

                    Player player = new Player(playerInfo);
                    Game[] games = playerInfo.Games.Select(g => new Game(g, ++gamePk, player.ID)).ToArray();

                    db.Players.Add(player);
                    db.Games.AddRange(games);
                    db.SaveChanges();

                    Console.WriteLine(player.Name);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public static async Task UpdatePlayers()
        {
            Player[] players;
            var startTime = DateTime.Now;

            using (var db = new EspnEntities())
            {
                players = db.Players.ToArray();
                List<Game> games = new List<Game>(db.Games.Where(g => g.GameDate > new DateTime(2017, 10, 1)));
                MainForm.PlayerRater = new Rater(players, games);
            }

            if (!Utils.Ping(@"http://www.espn.com"))
            {
                Console.WriteLine("No Ping");
                return;
            }

            await Task.Run(() =>
            {
                var playerInfos = players.AsParallel().Select(p => new PlayerInfo(p.Name, p.ID, 2018));
                foreach (PlayerInfo playerInfo in playerInfos)
                {
                    if (playerInfo.Games.Count == 0) continue;
                    UpdatePlayer(playerInfo);
                }
            }).ConfigureAwait(false);
            Console.WriteLine(Environment.NewLine + "Done In " + (DateTime.Now - startTime).TotalSeconds + " Seconds");

            using (var db = new EspnEntities())
            {
                List<Game> games = new List<Game>(db.Games.Where(g => g.GameDate > new DateTime(2017, 10, 1)));
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

                    Game lastGame = db.Games.Where(g => g.PlayerId == player.ID).OrderByDescending(g => g.GameDate).First();
                    lastGame = new Game(playerInfo.Games.First(g => g.GameDate == lastGame.GameDate), lastGame.Pk, playerInfo.Id);


                    foreach (GameStats gameStats in playerInfo.Games.Where(g => g.GameDate > lastGame.GameDate))
                    {
                        var game = new Game(gameStats, GetNextGamePk(), player.ID);
                        db.Games.Add(game);
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
    }
}
