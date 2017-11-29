using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace espn
{
    public class DbManager
    {
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
                if (id == -1)
                    id = int.Parse(Microsoft.VisualBasic.Interaction.InputBox("Can't Found Player, Please Insert Id", "Add New Player", "Default", -1, -1));

                using (var db = new EspnEntities())
                {
                    if (db.Players.Any(p => p.Name.Equals(name) || p.ID == id))
                    {
                        //MessageBox.Show("Player Already Exist");
                        return;
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

        public void UpdatePlayers(int id)
        {
            using (var db = new EspnEntities())
            {
                
            }
        }
    }
}
