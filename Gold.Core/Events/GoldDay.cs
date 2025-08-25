using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Gold.Core.Events
{
    /// <summary>
    /// GoldDay class represents a GoldDay
    /// </summary>
    public class GoldDay
    {
        /// <summary>
        /// Database id
        /// </summary>
        public Guid GoldDayId { get; set; }

        /// <summary>
        /// User id who created it
        /// </summary>
        public Guid CreatedBy { get; set; }

        /// <summary>
        /// Gram to be transfered by users each turn
        /// </summary>
        public decimal GramAmount { get; set; }

        /// <summary>
        /// Date created
        /// </summary>
        public DateTime GoldDayCreationDateTime { get; set; }

        /// <summary>
        /// Time interval
        /// </summary>
        public string GoldDayTimeInterval { get; set; }

        /// <summary>
        /// True iff gold day initiated
        /// </summary>
        public bool GoldDayStarted { get; set; }

        /// <summary>
        /// True iff gold day cancelled
        /// </summary>
        public bool GoldDayCancelled { get; set; }

        /// <summary>
        /// True iff gold day completed
        /// </summary>
        public bool GoldDayComplete { get; set; }

        /// <summary>
        /// Name of this gold day
        /// </summary>
        public string GoldDayName { get; set; }

        /// <summary>
        /// User amount in the gold day
        /// </summary>
        public int UserAmount { get; set; }

        /// <summary>
        /// Gold day start date time
        /// </summary>
        public DateTime GoldDayStartDateTime { get; set; }

        /// <summary>
        /// Gold day data file name
        /// </summary>
        public string GameDataFile { get; set; }

        /// <summary>
        /// User who created this gold day
        /// </summary>
        public virtual User2 User { get; set; }

        /// <summary>
        /// Time interval types
        /// </summary>
        public static readonly string[] ValidTypes = { "day", "month", "year" };

        /// <summary>
        /// Private Constructor Required for EFCore Database framework
        /// </summary>
        private GoldDay()
        {
            
        }

        /// <summary>
        /// Creates new gold day
        /// </summary>
        /// <param name="userId">user who creates it</param>
        /// <param name="name">name of the gold day</param>
        /// <param name="grams">amount to be transferred</param>
        /// <param name="timeType">time interval type</param>
        /// <param name="userAmount">amount of users</param>
        /// <param name="startDateTime">start date of the goldday</param>
        public GoldDay(
            Guid userId,
            string name, 
            decimal grams,
            string timeType, 
            int userAmount,
            DateTime startDateTime)
        {
            CreatedBy = userId;
            GoldDayName = name;
            GramAmount = grams;
            GoldDayTimeInterval = timeType;
            GoldDayStarted = false;
            GoldDayCancelled = false;
            GoldDayComplete = false;
            UserAmount = userAmount;
            GoldDayStartDateTime = startDateTime;
            GenerateFile();
        }

        /// <summary>
        /// Calculates the next DateTime for the gold day
        /// adds either 7days 15days or 1month to the given date
        /// depending on the time interval
        /// </summary>
        /// <param name="then">previous due date</param>
        /// <param name="interval">amount of dates to be added</param>
        /// <returns></returns>
        public static DateTime NextDateTime(DateTime then, string interval)
        {
            if (interval == "7")
            {
                return then.AddDays(7);
            }
            else if (interval == "15")
            {
                return then.AddDays(15);
            }
            else
            {
                return then.AddMonths(1);
            }

        }

        /// <summary>
        /// Generates file to save Goldday data
        /// </summary>
        private void GenerateFile()
        {
            string content = "#users 1\n";
            content += CreatedBy.ToString() + ":true:true:true:true\n";
            content += "#turns 1\n";
            content += CreatedBy.ToString() + ":0:" + NextDateTime(GoldDayStartDateTime, GoldDayTimeInterval).ToString() + ":0\n";
            GameDataFile = "C:/GoldDays/" + GoldDayName.Replace(' ', '_') + "_" + GoldDayStartDateTime.ToString() + ".gd";
            System.IO.File.WriteAllText(GameDataFile, content);
        }


        /*
        public string SerializeData()
        {
            var ids = ParseMemberIds();

            var result = string.Format("GoldDay:{0}:{1}\n{2}\n", GoldDayName, GoldDayId, UserAmount);


            int user = 0;

            foreach (var id in ids)
            {
                var subResult = string.Format("{0}:", user);
                for (int i = 0; i < UserAmount; i++)
                {
                    subResult += "";
                }
            }



            return result;
        }

        public bool IsCompleted()
        {
            Completed = NextIndex == UserAmount;
            return Completed;
        }

        public void CompleteTurn()
        {
            if (TimeIntervalType == "day")
                NextPayDay = NextPayDay.AddDays(TimeIntervalValue);
            else if (TimeIntervalType == "month")
                NextPayDay = NextPayDay.AddMonths(TimeIntervalValue);
            else
                NextPayDay = NextPayDay.AddYears(TimeIntervalValue);
            NextIndex += 1;
        }

        private string GenerateMemberIds(List<int> memberIds, int userAmount)
        {
            string result = "";
            for (int i = 0; i < userAmount; i++)
            {
                result += memberIds[i].ToString();
                if (i != userAmount - 1)
                {
                    result += ":";
                }
            }
            return result;
        }

        public List<int> ParseMemberIds()
        {
            var res = new List<int>();
            var parts = MemberIds.Split(":");
            for (int i = 0; i < UserAmount; i++)
            {
                res.Add(int.Parse(parts[i]));
            }
            return res;
        }*/
    }
}
