# SlowDatabase Challenge

This is a simple project that demonstrates the generation and manipulation of data using SQLite, Dapper, and .NET Core. It showcases the use of good coding practices, clean architecture, and efficient data handling.

## Key Features

1. **Data Generation**: The application generates a SQLite database with a predefined schema and populates it with a large amount of data. This includes accounts, documents, and users.

2. **File Handling**: The application creates a test document and writes the content of every third file associated with a given account ID to a new text file.

3. **Efficient Data Retrieval**: The application uses Dapper, a high-performance micro-ORM, for data access. This allows for efficient querying and manipulation of the SQLite database.

4. **Configuration Management**: The application uses the Microsoft.Extensions.Configuration library for managing app settings, demonstrating good practices for configuration management.

## Getting Started

### Prerequisites

- .NET Core 3.1 or later
- SQLite

### Running the Application

1. Clone the repository to your local machine.
2. Navigate to the `SmartVault.DataGeneration` directory. This directory will contain the SQLite database file and the test document **once it's generated**.
3. Run the `SmartVault.DataGeneration` project to generate the data.
4. Navigate to the `SmartVault.Program` directory. This directory will contain the output text file **once it's generated**.
5. Run the `SmartVault.Program` project with an argument that represents the account ID. For example, `dotnet run 1`. This argument is used to select every third file associated with the given account ID.

   **Note**: The account ID argument can't be set in the console app build configurations. It needs to be provided when running the application.

6. The application will create a text file in the `SmartVault.Program` directory containing the content of every third file associated with the given account ID.
