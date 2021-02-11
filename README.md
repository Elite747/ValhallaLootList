## Building Instructions

### Prerequisites

1. ASP.Net Core 5.x SDK
2. MySQL server
3. "World of Warcraft: The Burning Crusade" world database (not provided)

4. Discord Application credentials

### Database Setup

For local development, a MySQL database server needs to be used. Initializing and upgrading the database is done using the `ItemImporter` tool. Using this tool requires a connection to your development database and a WoW: TBC world database. This database is not provided. Once you have a local MySQL server running (or access to an existing MySQL server), you need to store the connection strings using the .net Secret Manager tool.

In the root directory, run the following commands:

```
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<your application connection string>" -p src/server
```

``` 
dotnet user-secrets set "ConnectionStrings:WowConnection" "<WoW world DB connection string>" -p src/server
```

Note: The Secret Manager is configured to be shared across all projects. You only need to set secrets in one project for them to be available in all of them.

Once the world DB is set up, and your development MySQL server is running, run the following command to either initialize and populate the development database or update your existing database:

```
dotnet run -p src/ItemImporter
```

This application may take several minutes to complete.

### Item Restriction Determinations

Item restrictions are done separate from database initialization as they are expected to change at a rapid pace. The `ItemDeterminer` tool is what contains all of the rules used to restrict items from being listed by certain classes or specs, and will update the database to match the rules defined. This tool is safe to run at any time, and should be run after any rule code is added, changed, or removed.

To update item restrictions, run the following command:

```
dotnet run -p src/ItemDeterminer
```

### Discord integration

Discord is used for authentication and authorization via OAuth. To use it, the client secret must be configured. The client secret must **never** be present in source control. When you have access to the client secret, set it using the Secret Manager tool via the following command:

```
dotnet user-secrets set "Discord:ClientSecret" "<discord client secret>" -p src/server
```

### Conclusion

Once these steps are taken, the project is ready to be run. Either start the `ValhallaLootList.Server` project in Visual Studio, or run it using `dotnet run -p src/server`.