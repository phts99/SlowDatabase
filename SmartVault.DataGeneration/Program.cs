using Dapper;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SmartVault.Library;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Xml.Serialization;

namespace SmartVault.DataGeneration
{
    partial class Program
    {
        static void Main(string[] args)
        {
            var configuration = LoadConfiguration();

            string databasePath = CreateDatabase(configuration);

            string textFilePath = CreateTestDocument();

            using (var connection = new SQLiteConnection(string.Format(configuration?["ConnectionStrings:DefaultConnection"] ?? "", databasePath)))
            {
                connection.Open();

                ExecuteBusinessObjectScripts(connection);

                GenerateData(connection, textFilePath);

                DisplayDataCounts(connection);
            }
        }

        static IConfiguration LoadConfiguration()
        {
            return new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                                             .AddJsonFile("appsettings.json")
                                             .Build();
        }

        static string CreateDatabase(IConfiguration configuration)
        {
            string dataGenerationPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;

            string databasePath = Path.Combine(dataGenerationPath, configuration["DatabaseFileName"]);

            SQLiteConnection.CreateFile(databasePath);

            return databasePath;
        }

        static string CreateTestDocument()
        {
            string dataGenerationPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;

            string textFilePath = Path.Combine(dataGenerationPath, "TestDoc.txt");

            File.WriteAllText(textFilePath, "This is my test document");

            return textFilePath;
        }

        static void ExecuteBusinessObjectScripts(SQLiteConnection connection)
        {
            var files = Directory.GetFiles(@"..\..\..\..\BusinessObjectSchema");

            for (int i = 0; i <= 2; i++)
            {
                var serializer = new XmlSerializer(typeof(BusinessObject));

                var businessObject = serializer.Deserialize(new StreamReader(files[i])) as BusinessObject;

                connection.Execute(businessObject?.Script);
            }
        }

        static void GenerateData(SQLiteConnection connection, string textFilePath)
        {
            var documentNumber = 0;

            using (var transaction = connection.BeginTransaction())
            {
                for (int i = 0; i < 100; i++)
                {
                    var randomDayIterator = RandomDay().GetEnumerator();
                    randomDayIterator.MoveNext();

                    connection.Execute($"INSERT INTO Account (Id, Name) VALUES('{i}','Account{i}')", transaction: transaction);

                    for (int d = 0; d < 10000; d++, documentNumber++)
                    {
                        connection.Execute($"INSERT INTO Document (Id, Name, FilePath, Length, AccountId) VALUES('{documentNumber}','Document{i}-{d}.txt','{textFilePath}','{new FileInfo(textFilePath).Length}','{i}')", transaction: transaction);
                    }

                    connection.Execute($"INSERT INTO User (Id, FirstName, LastName, DateOfBirth, AccountId, Username, Password) VALUES('{i}','FName{i}','LName{i}','{randomDayIterator.Current.ToString("yyyy-MM-dd")}','{i}','UserName-{i}','e10adc3949ba59abbe56e057f20f883e')", transaction: transaction);
                }

                transaction.Commit();
            }
        }

        static void DisplayDataCounts(SQLiteConnection connection)
        {
            var accountData = connection.Query("SELECT COUNT(*) FROM Account;");
            Console.WriteLine($"AccountCount: {JsonConvert.SerializeObject(accountData)}");

            var documentData = connection.Query("SELECT COUNT(*) FROM Document;");
            Console.WriteLine($"DocumentCount: {JsonConvert.SerializeObject(documentData)}");

            var userData = connection.Query("SELECT COUNT(*) FROM User;");
            Console.WriteLine($"UserCount: {JsonConvert.SerializeObject(userData)}");
        }

        static IEnumerable<DateTime> RandomDay()
        {
            DateTime startDate = new DateTime(1985, 1, 1);

            Random randomGenerator = new Random();

            int dateRange = (DateTime.Today - startDate).Days;

            while (true)
                yield return startDate.AddDays(randomGenerator.Next(dateRange));
        }
    }
}
