using System;
using MySql.Data.MySqlClient;

namespace ChickenHunt.Website.DataLayer
{
    public interface IDataStorage
    {
        int InsertChickenCandidate(string name, string email, DateTime createDate, string password);
        void UpdateChickenCandidate(int chickenCandidateID, string name, string email, DateTime createDate);
        int InsertChickenRecord(DateTime createDate, int chickenCandidateID1, int chickenCandidateID2, int? chickenCandidateGiverID1, int? chickenCandidateGiverID2, int reportedByHunterID);
        void UpdateChickenRecord(int chickenRecordID, DateTime createDate, int chickenCandidateID1, int chickenCandidateID2, int? chickenCandidateGiverID1, int? chickenCandidateGiverID2);
        string GenerateToken(string email, string password);
        string GetToken(string email, string password);
        ChickenCandidate GetHunterByEmail(string email);
        ChickenCandidate GetHunterByResetPasswordCode(string resetPasswordCode);
        void Init();
        bool IsNameAvailable(string name);
        bool IsEmailAvailable(string email);
        ChickenCandidate GetHunterByToken(string token);
        ChickenCandidate[] GetHunters();
        void UpdateCude(int hunterID, DateTime date);
        ChickenHuntReport[] GetReport();
        void UpdateResetPasswordCode(string email, string passwordResetCode);
        void UpdatePassword(int hunterID, string password);
        RecentChickenRecord[] GetRecentChickens();

        /// <summary>
        ///  Returns list of chickens where hunter participated
        /// </summary>
        /// <param name="hunterID"></param>
        /// <returns></returns>
        RecentChickenRecord[] GetHunterGames(int hunterID);
    }

    public class ChickenHuntReport
    {
        public int HunterID { get; set; }
        public string HunterName { get; set; }
        public int Chickens { get; set; }
        public DateTime Date { get; set; }
        public int Today { get; set; }
    }

    public class ChickenCandidate
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime CreateDate { get; set; }
    }

    public static class ChickenCandidateExtensions
    {
        public static ChickenCandidate ToDataObject(this MySqlDataReader r)
        {
            return new ChickenCandidate()
            {
                ID = r.GetInt32("ID"),
                Name = r.GetString("Name"),
                Email = r.GetString("Email"),
                CreateDate = r.GetDateTime("CreateDate")
            };
        }
    }

    internal class ChickenRecord
    {
        public int ID { get; set; }
        public DateTime CreateDate { get; set; }
        public int ChickenCandidateID1 { get; set; }
        public int ChickenCandidateID2 { get; set; }
        public int? ChickenCandidateGiverID1 { get; set; }
        public int? ChickenCandidateGiverID2 { get; set; }
    }

    public class RecentChickenRecord
    {
        public DateTime Date { get; set; }
        public int Recipient1ID { get; set; }
        public string Recipient1Name { get; set; }
        public int Recipient2ID { get; set; }
        public string Recipient2Name { get; set; }
        public int Maker1ID { get; set; }
        public string Maker1Name { get; set; }
        public int Maker2ID { get; set; }
        public string Maker2Name { get; set; }
        public int ReporterID { get; set; }
        public string ReporterName { get; set; }
    }

    internal class ChickenRecordHistory
    {
        public int ID { get; set; }
        public int ChickenRecordID { get; set; }
        public DateTime CreateDate { get; set; }
        public int SubmittedByChickenCandidateID { get; set; }
        public string Change { get; set; }
    }
}