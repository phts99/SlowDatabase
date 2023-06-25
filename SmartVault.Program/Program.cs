using System;
using System.IO;
using Dapper;
using System.Data.SQLite;
using SmartVault.Program.BusinessObjects;
using System.Linq;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace SmartVault.Program
{
    partial class Program
    {
        public static IConfiguration Configuration { get; private set; }

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                return;
            }

            LoadConfiguration();

            WriteEveryThirdFileToFile(args[0]);

            DisplayTotalFileSize();
        }

        private static void LoadConfiguration()
        {
            Configuration = new ConfigurationBuilder().SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\..\SmartVault.DataGeneration"))
                                                      .AddJsonFile("appsettings.json")
                                                      .Build();
        }

        private static void DisplayTotalFileSize()
        {
            string totalSize = GetAllFileSizes();

            Console.WriteLine($"Total size of all files: {totalSize}");
        }

        private static string GetAllFileSizes()
        {
            string dataGenerationPath = Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\..\SmartVault.DataGeneration");

            string databasePath = Path.Combine(dataGenerationPath, Configuration["DatabaseFileName"]);

            using (var connection = new SQLiteConnection(string.Format(Configuration?["ConnectionStrings:DefaultConnection"] ?? "", databasePath)))
            {
                var totalSize = connection.ExecuteScalar<long>("SELECT SUM(Length) FROM Document");

                return FormatBytes(totalSize);
            }
        }

        public static string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };

            int order = 0;

            while (bytes >= 1024 && order < sizes.Length - 1)
            {
                order++;
                bytes /= 1024;
            }

            return $"{bytes:0.##} {sizes[order]}";
        }

        private static void WriteEveryThirdFileToFile(string accountId)
        {
            string dataGenerationPath = Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\..\SmartVault.DataGeneration");

            string databasePath = Path.Combine(dataGenerationPath, Configuration["DatabaseFileName"]);

            string programPath = Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\..\SmartVault.Program");

            string everyThirdFilePath = Path.Combine(programPath, "EveryThirdFile.txt");

            using (var connection = new SQLiteConnection(string.Format(Configuration?["ConnectionStrings:DefaultConnection"] ?? "", databasePath)))
            {
                var documents = connection.Query<Document>($"SELECT * FROM Document WHERE AccountId = {accountId}");
                var everyThirdDocument = documents.Where((x, i) => i % 3 == 0);

                WriteDocumentsToFile(everyThirdDocument, everyThirdFilePath);
            }
        }

        private static void WriteDocumentsToFile(IEnumerable<Document> documents, string filePath)
        {
            using (var writer = new StreamWriter(filePath))
            {
                foreach (var document in documents)
                {
                    var content = File.ReadAllText(document.FilePath);
                    writer.WriteLine(content);
                }
            }
        }
    }
}
