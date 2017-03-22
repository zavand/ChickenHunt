using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using MySql.Data.MySqlClient;

namespace ChickenHunt.Website.DataLayer
{
    internal class DataStorage : MySqlDataStorage, IDataStorage
    {
        private readonly string _initialConnectionString;

        public DataStorage(string initialConnectionString, string dbname) : base(initialConnectionString, dbname)
        {
            _initialConnectionString = initialConnectionString;
        }

        public override void CreateTables()
        {
            using (var c = new MySqlConnection(ConnectionString))
            {
                c.Open();

                c.ExecuteNonQuery(@"
CREATE TABLE IF NOT EXISTS `hunter` (
  `ID` int NOT NULL AUTO_INCREMENT,
  `CreateDate` datetime NOT NULL,
  `Name` varchar(250) NOT NULL,
  `Email` varchar(250) NOT NULL,
  `PasswordSHA256` binary(32) NOT NULL,
  `Token` varchar(250) NULL,
  `ResetPasswordCode` varchar(250) NULL,
  PRIMARY KEY (`ID`),
  KEY `IDX_CreateDate` (`CreateDate`),
  KEY `IDX_Name` (`Name`),
  KEY `IDX_Email` (`Email`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
");

                c.ExecuteNonQuery(@"
CREATE TABLE IF NOT EXISTS `chicken` (
  `ID` int NOT NULL AUTO_INCREMENT,
  `CreateDate` datetime NOT NULL,
  `ChickenRecipientID1` int NOT NULL,
  `ChickenRecipientID2` int NOT NULL,
  `ChickenMakerID1` int NULL,
  `ChickenMakerID2` int NULL,
  `ReportedByHunterID` int NOT NULL,
  `DeletedByHunterID` int NULL,
  PRIMARY KEY (`ID`),
  KEY `IDX_ChickenRecipientID1` (`ChickenRecipientID1`),
  KEY `IDX_ChickenRecipientID2` (`ChickenRecipientID2`),
  KEY `IDX_ChickenMakerID1` (`ChickenMakerID1`),
  KEY `IDX_ChickenMakerID2` (`ChickenMakerID2`),
  KEY `IDX_CreateDate` (`CreateDate`),
  CONSTRAINT `FK_chicken_ChickenRecipientID1` FOREIGN KEY (`ChickenRecipientID1`) REFERENCES `hunter` (`ID`) ON DELETE CASCADE ON UPDATE NO ACTION,
  CONSTRAINT `FK_chicken_ChickenRecipientID2` FOREIGN KEY (`ChickenRecipientID2`) REFERENCES `hunter` (`ID`) ON DELETE CASCADE ON UPDATE NO ACTION,
  CONSTRAINT `FK_chicken_ChickenMakerID1` FOREIGN KEY (`ChickenMakerID1`) REFERENCES `hunter` (`ID`) ON DELETE CASCADE ON UPDATE NO ACTION,
  CONSTRAINT `FK_chicken_ChickenMakerID2` FOREIGN KEY (`ChickenMakerID2`) REFERENCES `hunter` (`ID`) ON DELETE CASCADE ON UPDATE NO ACTION,
  CONSTRAINT `FK_chicken_ReportedByHunterID` FOREIGN KEY (`ReportedByHunterID`) REFERENCES `hunter` (`ID`) ON DELETE CASCADE ON UPDATE NO ACTION
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
");

                c.ExecuteNonQuery(@"
CREATE TABLE IF NOT EXISTS `chicken_cube` (
  `HunterID` int NOT NULL,
  `Date` datetime NOT NULL,
  `Chickens` int NOT NULL,
  PRIMARY KEY (`HunterID`,`Date`),
  CONSTRAINT `FK_chicken_cube_HunterID` FOREIGN KEY (`HunterID`) REFERENCES `hunter` (`ID`) ON DELETE CASCADE ON UPDATE NO ACTION
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
");
            }
        }

        protected override void ApplyPatches()
        {
            base.ApplyPatches();

            using (var c = new MySqlConnection(ConnectionString))
            {
                c.Open();

                bool patchRequired = false;
                using (var r = new MySqlCommand("show columns in `hunter` where Field='ResetPasswordCode'", c).ExecuteReader())
                {
                    patchRequired = !r.HasRows;
                }
                if (patchRequired)
                {
                    c.ExecuteNonQuery(@"ALTER TABLE `hunter` ADD COLUMN `ResetPasswordCode` varchar(250) NULL");
                }


                // Add index for CreateDate column
                using (var r = new MySqlCommand("show indexes in `chicken` where Key_Name='IDX_CreateDate'", c).ExecuteReader())
                {
                    patchRequired = !r.HasRows;
                }
                if (patchRequired)
                {
                    c.ExecuteNonQuery(@"
create index `IDX_CreateDate` on `chicken`(`CreateDate`);
");
                }

                using (var r = new MySqlCommand("show columns in `chicken` where Field='DeletedByHunterID'", c).ExecuteReader())
                {
                    patchRequired = !r.HasRows;
                }
                if (patchRequired)
                {
                    c.ExecuteNonQuery(@"ALTER TABLE `chicken` ENGINE = InnoDB;");

                    c.ExecuteNonQuery(@"ALTER TABLE `chicken` ADD COLUMN `DeletedByHunterID` int NULL");
                    c.ExecuteNonQuery(@"ALTER TABLE `chicken` ADD CONSTRAINT `FK_chicken_DeletedByHunterID` FOREIGN KEY (`DeletedByHunterID`) REFERENCES `hunter` (`ID`) ON DELETE NO ACTION ON UPDATE NO ACTION;");
                    c.ExecuteNonQuery(@"ALTER TABLE `chicken` ADD CONSTRAINT `FK_chicken_ChickenRecipientID1` FOREIGN KEY (`ChickenRecipientID1`) REFERENCES `hunter` (`ID`) ON DELETE NO ACTION ON UPDATE NO ACTION;");
                    c.ExecuteNonQuery(@"ALTER TABLE `chicken` ADD CONSTRAINT `FK_chicken_ChickenRecipientID2` FOREIGN KEY (`ChickenRecipientID2`) REFERENCES `hunter` (`ID`) ON DELETE NO ACTION ON UPDATE NO ACTION;");
                    c.ExecuteNonQuery(@"ALTER TABLE `chicken` ADD CONSTRAINT `FK_chicken_ChickenMakerID1` FOREIGN KEY (`ChickenMakerID1`) REFERENCES `hunter` (`ID`) ON DELETE NO ACTION ON UPDATE NO ACTION;");
                    c.ExecuteNonQuery(@"ALTER TABLE `chicken` ADD CONSTRAINT `FK_chicken_ChickenMakerID2` FOREIGN KEY (`ChickenMakerID2`) REFERENCES `hunter` (`ID`) ON DELETE NO ACTION ON UPDATE NO ACTION;");
                    c.ExecuteNonQuery(@"ALTER TABLE `chicken` ADD CONSTRAINT `FK_chicken_ReportedByHunterID` FOREIGN KEY (`ReportedByHunterID`) REFERENCES `hunter` (`ID`) ON DELETE NO ACTION ON UPDATE NO ACTION;");
                }

                //ALTER TABLE `chicken_hunt`.`chicken`  ENGINE = InnoDB;
            }
        }

        public int InsertChickenCandidate(string name, string email, DateTime createDate, string password)
        {
            int result;
            var passwordSHA256 = SHA256.Create().ComputeHash(System.Text.Encoding.Unicode.GetBytes(password));
            using (var c = new MySqlConnection(ConnectionString))
            {
                c.Open();

                result = (int) c.ExecuteScalar<ulong>($"insert into hunter (CreateDate,Name,Email,PasswordSHA256) values (@CreateDate,@Name,@Email,@PasswordSHA256); select LAST_INSERT_ID();",
                    "@CreateDate", createDate,
                    "@Name", name,
                    "@Email", email,
                    "@PasswordSHA256", passwordSHA256
                );
            }
            return result;
        }

        public void UpdateChickenCandidate(int chickenCandidateID, string name, string email, DateTime createDate)
        {
            throw new NotImplementedException();
        }

        public int InsertChickenRecord(DateTime createDate, int chickenCandidateID1, int chickenCandidateID2, int? chickenCandidateGiverID1, int? chickenCandidateGiverID2, int reportedByHunterID)
        {
            using (var c = new MySqlConnection(ConnectionString))
            {
                c.Open();

                var r = (int) c.ExecuteScalar<ulong>($"insert into chicken (CreateDate,ChickenRecipientID1,ChickenRecipientID2,ChickenMakerID1,ChickenMakerID2,ReportedByHunterID) values (@CreateDate,@ChickenRecipientID1,@ChickenRecipientID2,@ChickenMakerID1,@ChickenMakerID2,@ReportedByHunterID); select LAST_INSERT_ID();",
                    "@CreateDate", createDate,
                    "@ChickenRecipientID1", chickenCandidateID1,
                    "@ChickenRecipientID2", chickenCandidateID2,
                    "@ChickenMakerID1", chickenCandidateGiverID1,
                    "@ChickenMakerID2", chickenCandidateGiverID2,
                    "@ReportedByHunterID", reportedByHunterID
                );
                return r;
            }
        }

        public void UpdateChickenRecord(int chickenRecordID, DateTime createDate, int chickenCandidateID1, int chickenCandidateID2, int? chickenCandidateGiverID1, int? chickenCandidateGiverID2)
        {
            throw new NotImplementedException();
        }

        public string GenerateToken(string email, string password)
        {
            string token;
            var passwordSHA256 = SHA256.Create().ComputeHash(System.Text.Encoding.Unicode.GetBytes(password));
            using (var c = new MySqlConnection(ConnectionString))
            {
                c.Open();

                var count = c.ExecuteScalar<long>("select count(*) from hunter where Email=@Email and PasswordSHA256=@PasswordSHA256",
                    "@Email", email,
                    "@PasswordSHA256", passwordSHA256);

                if (count == 0)
                    throw new InvalidCredentialsException();

//                var o = c.ExecuteScalar<object>("select token from hunter where Email=@Email and PasswordSHA256=@PasswordSHA256",
//                    "@Email", email,
//                    "@PasswordSHA256", passwordSHA256);
//
//                token = o == DBNull.Value ? null : (string) o;

//                if (string.IsNullOrEmpty(token))
                {
                    token = Guid.NewGuid().ToString();
                    c.ExecuteNonQuery($"update hunter set token='{token}' where Email=@Email",
                        "@Email", email);
                }
            }
            return token;
        }

        public string GetToken(string email, string password)
        {
            string token;
            var passwordSHA256 = SHA256.Create().ComputeHash(System.Text.Encoding.Unicode.GetBytes(password));
            using (var c = new MySqlConnection(ConnectionString))
            {
                c.Open();

                var count = c.ExecuteScalar<long>("select count(*) from hunter where Email=@Email and PasswordSHA256=@PasswordSHA256",
                    "@Email", email,
                    "@PasswordSHA256", passwordSHA256);

                if (count == 0)
                    throw new InvalidCredentialsException();

                {
                    token = Guid.NewGuid().ToString();
                    c.ExecuteReader(r =>
                        {
                            token = r["token"] == DBNull.Value ? null : r.GetString("token");
                        }, $"select token from hunter where Email=@Email",
                        "@Email", email);
                }
            }
            return token;
        }

        public ChickenCandidate GetHunterByEmail(string email)
        {
            var rr = new List<ChickenCandidate>();
            using (var c = new MySqlConnection(ConnectionString))
            {
                c.Open();

                c.ExecuteReader(
                    r =>
                    {
                        rr.Add(r.ToDataObject());
                    },
                    "select * from hunter where Email=@Email LIMIT 1", "@Email", email
                );
            }
            return rr.FirstOrDefault();

        }

        public ChickenCandidate GetHunterByResetPasswordCode(string resetPasswordCode)
        {
            var rr = new List<ChickenCandidate>();
            using (var c = new MySqlConnection(ConnectionString))
            {
                c.Open();

                c.ExecuteReader(
                    r =>
                    {
                        rr.Add(r.ToDataObject());
                    },
                    "select * from hunter where ResetPasswordCode=@ResetPasswordCode LIMIT 1", "@ResetPasswordCode", resetPasswordCode
                );
            }
            return rr.FirstOrDefault();
        }

        public bool IsNameAvailable(string name)
        {
            bool r;
            using (var c = new MySqlConnection(ConnectionString))
            {
                c.Open();

                r = c.ExecuteScalar<long>("select count(*) from hunter where Name=@Name",
                        "@Name", name
                    ) == 0;
            }
            return r;
        }

        public bool IsEmailAvailable(string email)
        {
            bool r;
            using (var c = new MySqlConnection(ConnectionString))
            {
                c.Open();

                r = c.ExecuteScalar<long>("select count(*) from hunter where Email=@Email",
                        "@Email", email
                    ) == 0;
            }
            return r;
        }

        public ChickenCandidate GetHunterByToken(string token)
        {
            var rr = new List<ChickenCandidate>();
            using (var c = new MySqlConnection(ConnectionString))
            {
                c.Open();

                c.ExecuteReader(
                    r =>
                    {
                        rr.Add(r.ToDataObject());
                    },
                    "select * from hunter where token=@Token", "@Token", token
                );
            }
            return rr.FirstOrDefault();
        }

        public ChickenCandidate[] GetHunters()
        {
            var rr = new List<ChickenCandidate>();
            using (var c = new MySqlConnection(ConnectionString))
            {
                c.Open();

                c.ExecuteReader(
                    r =>
                    {
                        rr.Add(r.ToDataObject());
                    },
                    "select * from hunter order by Name"
                );
            }
            return rr.ToArray();
        }

        public void UpdateCude(int hunterID, DateTime date, int count = 1)
        {
            using (var c = new MySqlConnection(ConnectionString))
            {
                c.Open();

                var expr = $"{(count >= 0 ? $"+" : "")}{count}";
                date = date.AddDays(-date.Day + 1).Date;

                var dateMax = date.AddMonths(1);
                c.ExecuteNonQuery($@"
start transaction;
select Chickens from chicken_cube where HunterID={hunterID} FOR UPDATE;
insert into chicken_cube (HunterID,Date,Chickens) values ({hunterID},@Date,{count})
-- ON DUPLICATE KEY update Chickens=(select count(*) from chicken where (ChickenRecipientID1={hunterID} or ChickenRecipientID2={hunterID}) and CreateDate>=@Date and CreateDate<@DateMax);
ON DUPLICATE KEY update Chickens=Chickens{expr};
commit;
",
                    "@Date", date,
                    "@DateMax", dateMax);
                // select count (*) from chicken_cube where 
            }
        }

        public ChickenHuntReport[] GetReport()
        {
            var rr = new List<ChickenHuntReport>();
            using (var c = new MySqlConnection(ConnectionString))
            {
                c.Open();

                c.ExecuteReader(
                    r =>
                    {
                        rr.Add(new ChickenHuntReport()
                        {
                            HunterID = r.GetInt32("ID"),
                            HunterName = r.GetString("Name"),
                            Date = r["Date"] == DBNull.Value ? DateTime.MinValue : r.GetDateTime("Date"),
                            Chickens = r["Chickens"] == DBNull.Value ? 0 : r.GetInt32("Chickens"),
                            Today = r.GetInt32("Today"),
                        });
                    },
                    $@"
select h.ID,h.Name,c.Date,c.Chickens, (select count(*) from chicken where DeletedByHunterID is null and (ChickenRecipientID1=h.ID or ChickenRecipientID2=h.ID) and CreateDate>=@Today and CreateDate<@Tomorrow) as 'Today' from hunter h
left join chicken_cube c on c.HunterID=h.ID
order by c.Date desc, c.Chickens desc, h.Name
",
                    "@Today", DateTime.Today,
                    "@Tomorrow", DateTime.Today.AddDays(1)
                );
            }
            return rr.ToArray();
        }

        public void UpdateResetPasswordCode(string email, string passwordResetCode)
        {
            using (var c = new MySqlConnection(ConnectionString))
            {
                c.Open();

                c.ExecuteNonQuery($"update hunter set ResetPasswordCode=@ResetPasswordCode where Email=@Email",
                    "@ResetPasswordCode", passwordResetCode,
                    "@Email", email
                );
            }
        }

        public void UpdatePassword(int hunterID, string password)
        {
            var passwordSHA256 = SHA256.Create().ComputeHash(System.Text.Encoding.Unicode.GetBytes(password));
            using (var c = new MySqlConnection(ConnectionString))
            {
                c.Open();

                c.ExecuteNonQuery($"update hunter set PasswordSHA256=@PasswordSHA256 where ID={hunterID}",
                    "@PasswordSHA256", passwordSHA256
                );
            }
        }

        public RecentChickenRecord[] GetRecentChickens()
        {
            var rr = new List<RecentChickenRecord>();
            using (var c = new MySqlConnection(ConnectionString))
            {
                c.Open();

                c.ExecuteReader(
                    r =>
                    {
                        rr.Add(new RecentChickenRecord
                        {
                            ID = r.GetInt32("ID"),
                            Recipient1ID = r.GetInt32("Recipient1ID"),
                            Recipient1Name = r.GetString("Recipient1Name"),
                            Recipient2ID = r.GetInt32("Recipient2ID"),
                            Recipient2Name = r.GetString("Recipient2Name"),
                            Maker1ID = r.GetInt32("Maker1ID"),
                            Maker1Name = r.GetString("Maker1Name"),
                            Maker2ID = r.GetInt32("Maker2ID"),
                            Maker2Name = r.GetString("Maker2Name"),
                            ReporterID = r.GetInt32("ReporterID"),
                            ReporterName = r.GetString("ReporterName"),
                            DeletedByHunterID = r["DeletedByHunterID"] == DBNull.Value ? (int?) null : r.GetInt32("DeletedByHunterID"),
                            DeletedByHunterName = r["DeletedByHunterName"] == DBNull.Value ? null : r.GetString("DeletedByHunterName"),
                            Date = r.GetDateTime("CreateDate"),
                        });
                    },
                    $@"
select c.*,
hr1.ID as 'Recipient1ID',hr1.Name as 'Recipient1Name',
hr2.ID as 'Recipient2ID',hr2.Name as 'Recipient2Name',
hm1.ID as 'Maker1ID',hm1.Name as 'Maker1Name',
hm2.ID as 'Maker2ID',hm2.Name as 'Maker2Name',
hr.ID as 'ReporterID',hr.Name as 'ReporterName',
hrdel.Name as 'DeletedByHunterName'
from chicken c
join hunter hr1 on hr1.ID = c.ChickenRecipientID1
join hunter hr2 on hr2.ID = c.ChickenRecipientID2
join hunter hm1 on hm1.ID = c.ChickenMakerID1
join hunter hm2 on hm2.ID = c.ChickenMakerID2
join hunter hr on hr.ID = c.ReportedByHunterID
left join hunter hrdel on hrdel.ID = c.DeletedByHunterID
order by c.CreateDate desc 
-- Limit 10
;
"
                );
            }
            return rr.ToArray();
        }

        public RecentChickenRecord[] GetHunterGames(int hunterID)
        {
            var rr = new List<RecentChickenRecord>();
            using (var c = new MySqlConnection(ConnectionString))
            {
                c.Open();

                c.ExecuteReader(
                    r =>
                    {
                        rr.Add(new RecentChickenRecord
                        {
                            Recipient1ID = r.GetInt32("Recipient1ID"),
                            Recipient1Name = r.GetString("Recipient1Name"),
                            Recipient2ID = r.GetInt32("Recipient2ID"),
                            Recipient2Name = r.GetString("Recipient2Name"),
                            Maker1ID = r.GetInt32("Maker1ID"),
                            Maker1Name = r.GetString("Maker1Name"),
                            Maker2ID = r.GetInt32("Maker2ID"),
                            Maker2Name = r.GetString("Maker2Name"),
                            ReporterID = r.GetInt32("ReporterID"),
                            ReporterName = r.GetString("ReporterName"),
                            Date = r.GetDateTime("CreateDate"),
                        });
                    },
                    $@"
select c.*,
hr1.ID as 'Recipient1ID',hr1.Name as 'Recipient1Name',
hr2.ID as 'Recipient2ID',hr2.Name as 'Recipient2Name',
hm1.ID as 'Maker1ID',hm1.Name as 'Maker1Name',
hm2.ID as 'Maker2ID',hm2.Name as 'Maker2Name',
hr.ID as 'ReporterID',hr.Name as 'ReporterName'
from chicken c
join hunter hr1 on hr1.ID = c.ChickenRecipientID1
join hunter hr2 on hr2.ID = c.ChickenRecipientID2
join hunter hm1 on hm1.ID = c.ChickenMakerID1
join hunter hm2 on hm2.ID = c.ChickenMakerID2
join hunter hr on hr.ID = c.ReportedByHunterID
where (c.ChickenRecipientID1={hunterID} or c.ChickenRecipientID2={hunterID} or c.ChickenMakerID1={hunterID} or c.ChickenMakerID2={hunterID}) and c.DeletedByHunterID is null
order by c.CreateDate desc ;
"
                );
            }
            return rr.ToArray();
        }

        public RecentChickenRecord GetChicken(int chickenID)
        {
            RecentChickenRecord record = null;
            using (var c = new MySqlConnection(ConnectionString))
            {
                c.Open();

                c.ExecuteReader(
                    r =>
                    {
                        record = new RecentChickenRecord
                        {
                            ID = r.GetInt32("ID"),
                            Recipient1ID = r.GetInt32("Recipient1ID"),
                            Recipient1Name = r.GetString("Recipient1Name"),
                            Recipient2ID = r.GetInt32("Recipient2ID"),
                            Recipient2Name = r.GetString("Recipient2Name"),
                            Maker1ID = r.GetInt32("Maker1ID"),
                            Maker1Name = r.GetString("Maker1Name"),
                            Maker2ID = r.GetInt32("Maker2ID"),
                            Maker2Name = r.GetString("Maker2Name"),
                            ReporterID = r.GetInt32("ReporterID"),
                            ReporterName = r.GetString("ReporterName"),
                            DeletedByHunterID = r["DeletedByHunterID"] == DBNull.Value ? (int?) null : r.GetInt32("DeletedByHunterID"),
                            DeletedByHunterName = r["DeletedByHunterName"] == DBNull.Value ? null : r.GetString("DeletedByHunterName"),
                            Date = r.GetDateTime("CreateDate"),
                        };
                    },
                    $@"
select c.*,
hr1.ID as 'Recipient1ID',hr1.Name as 'Recipient1Name',
hr2.ID as 'Recipient2ID',hr2.Name as 'Recipient2Name',
hm1.ID as 'Maker1ID',hm1.Name as 'Maker1Name',
hm2.ID as 'Maker2ID',hm2.Name as 'Maker2Name',
hr.ID as 'ReporterID',hr.Name as 'ReporterName',
hrdel.Name as 'DeletedByHunterName'
from chicken c
join hunter hr1 on hr1.ID = c.ChickenRecipientID1
join hunter hr2 on hr2.ID = c.ChickenRecipientID2
join hunter hm1 on hm1.ID = c.ChickenMakerID1
join hunter hm2 on hm2.ID = c.ChickenMakerID2
join hunter hr on hr.ID = c.ReportedByHunterID
left join hunter hrdel on hrdel.ID = c.DeletedByHunterID
where c.ID = {chickenID}
;
"
                );
            }
            return record;
        }

        public void DeleteChicken(int chickenID, int hunterID)
        {
            using (var c = new MySqlConnection(ConnectionString))
            {
                c.Open();
                c.ExecuteNonQuery($"update chicken set DeletedByHunterID={hunterID} where ID={chickenID};");
                var chicken = GetChicken(chickenID);

                // Update cube
                UpdateCude(chicken.Recipient1ID, chicken.Date, -1);
                UpdateCude(chicken.Recipient2ID, chicken.Date, -1);
            }
        }
    }
        internal class InvalidCredentialsException : Exception
        {
        }

        public class MaxDeadlockAttemptsReached : Exception
        {
        }

}