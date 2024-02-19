<!--<a href="https://www.buymeacoffee.com/lyras" target="_blank"><img src="https://cdn.buymeacoffee.com/buttons/v2/default-violet.png" alt="Buy Me A Coffee" style="height: 60px !important;width: 217px !important;" ></a>-->
<a href='https://ko-fi.com/K3K2UOPPF' target='_blank'><img height='36' style='border:0px;height:36px;' src='https://storage.ko-fi.com/cdn/kofi2.png?v=3' border='0' alt='Buy Me a Coffee at ko-fi.com' /></a>

## Environments

Integration: https://valhallalootlist-linux-dev.azurewebsites.net

Production: https://www.valhalla-wow.com

## Building Instructions

### Prerequisites

1. Dotnet 8 SDK
2. Connection string to a SQL Server database
3. Discord Application credentials

### Database Setup

Initializing and upgrading the database is done using the `SeedAndMigrate` tool. Using this tool requires a connection to your development database. Visual Studio includes a built-in version of SQL Server Express, which may be used. If running with Visual studio, you may use this connection string: `Data Source=(localdb)\\MSSQLLocalDB;Database=ValhallaLootListDatabase;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False`

Set your connection string by running this command:
```
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<your application connection string>" -p src/Server
```

Note: The secret manager is configured to be shared across all projects. You only need to set secrets in one project for them to be available in all of them.

Once a connection string is set up, run the following command to set up and seed the database:
```
dotnet run -p src/SeedAndMigrate
```

### Discord integration

Discord is used for authentication and authorization via OAuth. To use it, the client secret and bot token must be configured. These secrets should **never** be present in source control. When you have access to these values, set them using the Secret Manager tool via the following command:
```
dotnet user-secrets set "Discord:ClientSecret" "<discord client secret>" -p src/Server
dotnet user-secrets set "Discord:BotToken" "<discord bot token>" -p src/Server
```

### Running

Once the database and discord integrations are set, run the project in your IDE or by running `dotnet run -p src/Server`.
