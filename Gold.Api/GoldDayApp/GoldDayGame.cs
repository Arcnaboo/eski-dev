using Gold.Core.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


namespace Gold.Api.GoldDayApp
{
    /// <summary>
    /// ALTIN GUNU OYUNU
    /// 
    /// OYUN DATA DOSYASI FORMATI
    /// ********
    /// 
    /// </summary>
    public class GoldDayGame
    {


        private class GoldDayTurn
        {
            public Guid Host { get; set; }
            public int ReceivedAmount { get; set; }
            public DateTime DueDateTime { get; set; }
            public decimal TotalReceived { get; set; }

            override public string ToString()
            {
                return Host.ToString() + ":" + ReceivedAmount.ToString() + ":" + DueDateTime.ToString()
                    + ":" + TotalReceived.ToString();
            }

        }


        private class GoldDayUser
        {
            public Guid UserId { get; set; }
            public bool GoldDayCreator { get; set; }
            public bool CurrentTurnHost { get; set; }
            public bool PaidCurrentTurn { get; set; }
            public bool UserAcceptedInvite { get; set; }

            override public string ToString() 
            {
                return UserId.ToString() + ":" + GoldDayCreator.ToString() + ":" + CurrentTurnHost.ToString()
                    + ":" + PaidCurrentTurn.ToString() + ":" + UserAcceptedInvite.ToString();
            }
            
            public string SmallToString()
            {
                var str = ToString();

                str = str.Substring(str.IndexOf(":") + 1);

                return str;
            }
           
        }

        //private List<User> Users;
        private GoldDay _GoldDay;
        private List<GoldDayUser> GoldDayUsers;
        private List<GoldDayTurn> Turns;
        public GoldDayGame(GoldDay goldDay)
        {
            _GoldDay = goldDay;
            GoldDayUsers = new List<GoldDayUser>();
            Turns = new List<GoldDayTurn>();
            LoadGame();
        }


        public Dictionary<Guid, string> GetUserStatus()
        {
            var result = new Dictionary<Guid, string>();
            foreach (var user in GoldDayUsers)
            {
                result.Add(user.UserId, user.SmallToString());
            }


            return result;
        }

      

        public void UserAccepted(Guid userId)
        {
            GoldDayUser user = null;
            for (var i = 0; i < GoldDayUsers.Count; i++)
            {
                if (GoldDayUsers[i].UserId == userId)
                {
                    user = GoldDayUsers[i];
                    break;
                }
            }

            user.UserAcceptedInvite = true;

            SaveGame();
        }

        public void RemoveUser(Guid userId)
        {
            GoldDayUser toRemove = null;
            GoldDayTurn toRemoveTurn = null;
            for (var i = 0; i < GoldDayUsers.Count; i++)
            {
                if (GoldDayUsers[i].UserId == userId)
                {
                    toRemove = GoldDayUsers[i];
                    toRemoveTurn = Turns[i];
                    break;
                }
            }

            if (toRemove == null)
            {
                throw new Exception("Bu kullanıcı GoldDay üyesi değil");
            }

            GoldDayUsers.Remove(toRemove);
            Turns.Remove(toRemoveTurn);

            SaveGame();
        }


        public void AddNewUser(Guid userId)
        {
            if (GoldDayUsers.Count == _GoldDay.UserAmount)
            {
                throw new Exception("Daha fazla kullanıcı eklenemez.");
            }

            var user = new GoldDayUser
            {
                UserId = userId,
                GoldDayCreator = false,
                CurrentTurnHost = false,
                PaidCurrentTurn = false,
                UserAcceptedInvite = false
            };

            GoldDayUsers.Add(user);
            AddNewTurn(user, Turns[Turns.Count - 1]);
            SaveGame();
        }

        private void AddNewTurn(GoldDayUser user, GoldDayTurn previousTurn)
        {

            var turn = new GoldDayTurn
            {
                Host = user.UserId,
                ReceivedAmount = 0,
                DueDateTime = GoldDay.NextDateTime(previousTurn.DueDateTime, _GoldDay.GoldDayTimeInterval),
                TotalReceived = 0

            };

            Turns.Add(turn);
        }


        private void SaveGame()
        {
            StreamWriter writer = new StreamWriter(_GoldDay.GameDataFile);
            writer.WriteLine("#users " + GoldDayUsers.Count.ToString());
            
            foreach (var user in GoldDayUsers)
            {
                writer.WriteLine(user.ToString());
            }

            writer.WriteLine("#turns " + Turns.Count.ToString());

            foreach (var turn in Turns)
            {
                writer.WriteLine(turn.ToString());
            }
            writer.Flush();
            writer.Close();
            
        }

        private void LoadGame()
        {

            string line;
            StreamReader file = new System.IO.StreamReader(_GoldDay.GameDataFile);

            line = file.ReadLine();
            if (line == null)
            {
                file.Close();
                throw new Exception("Invalid GoldDay file format");
            }

            if (line.StartsWith("#users"))
            {
                var splits = line.Split(' ');
                if (splits.Length != 2)
                {
                    file.Close();
                    throw new Exception("Invalid GoldDay file format");
                }
                int amount = int.Parse(splits[1]);

                for (int i = 0; i < amount; i++)
                {
                    line = file.ReadLine();
                    if (line == null)
                    {
                        file.Close();
                        throw new Exception("Invalid GoldDay file format");
                    }

                    splits = line.Split(':');
                    if (splits.Length != 5)
                    {
                        file.Close();
                        throw new Exception("Invalid GoldDay file format");
                    }

                    var user = new GoldDayUser
                    {
                        UserId = Guid.Parse(splits[0]),
                        GoldDayCreator = bool.Parse(splits[1]),
                        CurrentTurnHost = bool.Parse(splits[2]),
                        PaidCurrentTurn = bool.Parse(splits[3]),
                        UserAcceptedInvite = bool.Parse(splits[4])
                    };
                    GoldDayUsers.Add(user);
                }
            }
            else
            {
                file.Close();
                throw new Exception("Invalid GoldDay file format");
            }

            line = file.ReadLine();
            if (line == null)
            {
                file.Close();
                throw new Exception("Invalid GoldDay file format");
            }
            if (line.StartsWith("#turns"))
            {
                var splits = line.Split(' ');
                if (splits.Length != 2)
                {
                    file.Close();
                    throw new Exception("Invalid GoldDay file format");
                }
                int amount = int.Parse(splits[1]);
                for (int i = 0; i < amount; i++)
                {
                    line = file.ReadLine();
                    if (line == null)
                    {
                        file.Close();
                        throw new Exception("Invalid GoldDay file format");
                    }

                    splits = line.Split(':');
                    if (splits.Length != 4)
                    {
                        file.Close();
                        throw new Exception("Invalid GoldDay file format");
                    }

                    var turn = new GoldDayTurn
                    {
                        Host = Guid.Parse(splits[0]),
                        ReceivedAmount = int.Parse(splits[1]),
                        DueDateTime = DateTime.Parse(splits[2]),
                        TotalReceived = Decimal.Parse(splits[3])
                    };

                    Turns.Add(turn);
                }
            }
            else
            {
                file.Close();
                throw new Exception("Invalid GoldDay file format");
            }

            file.Close();
        }
        

       

        

        


        
    }
}
