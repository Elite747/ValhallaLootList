## Environments

Integration: https://valhallalootlistdev.azurewebsites.net

Production: https://www.valhalla-wow.com

## Building Instructions

### Prerequisites

1. ASP.Net Core 5.x SDK (Included with Visual Studio 2019)
2. SQL Server
3. Discord Application credentials

### Database Setup

For local development, a SQL Server database needs to be used. Initializing and upgrading the database is done using the `SeedAndMigrate` tool. Using this tool requires a connection to your development database. Visual Studio includes a built-in version of SQL Server Express, which may be used. If running with Visual studio, you may use this connection string: `Data Source=(localdb)\\MSSQLLocalDB;Database=ValhallaLootListDatabase;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False`

Once you have a valid connection string, run the following command in the root directory:

```
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<your application connection string>" -p src/server
```

Note: The Secret Manager is configured to be shared across all projects. You only need to set secrets in one project for them to be available in all of them.

Once a valid connection string is set up, run the following command to set up and seed the database:

```
dotnet run -p src/SeedAndMigrate
```

### Generating Items

The repo contains a pre-generated collection of items that are inserted into the database when seeded. The items are in `/src/seed.items.json`. This can be re-created using the [Item Importer](https://github.com/Elite747/ValhallaLootListItemImporter) project.

### Discord integration

Discord is used for authentication and authorization via OAuth. To use it, the client secret and bot token must be configured. These secrets should **never** be present in source control. When you have access to these values, set them using the Secret Manager tool via the following command:

```
dotnet user-secrets set "Discord:ClientSecret" "<discord client secret>" -p src/server
dotnet user-secrets set "Discord:BotToken" "<discord bot token>" -p src/server
```

### Conclusion

Once these steps are taken, the project is ready to be run. Either start the `ValhallaLootList.Server` project in Visual Studio, or run it using `dotnet run -p src/server`.
