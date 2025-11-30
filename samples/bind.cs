#!/usr/bin/env dotnet --

#:project ../src/Argx

using Argx;
using Argx.Actions;
using Argx.Extensions;
using Argx.Parsing;

var parser = new ArgumentParser();
parser.Add("--name", usage: "Application name");
parser.Add<int>("--max-connections", usage: "Maximum number of connections");
parser.Add("--database-host", usage: "Database host");
parser.Add("--database-username", usage: "Database username");
parser.Add<bool>("--database-use-ssl", action: ArgumentActions.StoreTrue, usage: "Use SSL for the connection");
parser.Add<int>("--database-port", usage: "Database port");
parser.Add<List<string>>("--database-tags", arity: Arity.Any, usage: "Tags for the database");
parser.Add("--cache-provider", usage: "Cache provider");
parser.Add<int>("--cache-expiration-minutes", usage: "Cache expiration time in minutes");

var argx = parser.Parse(
[
    "--name", "argx",
    "--max-connections", "100",
    "--database-host", "localhost",
    "--database-port", "5432",
    "--database-username", "admin",
    "--database-use-ssl",
    "--database-tags", "production", "primary",
    "--cache-provider", "redis",
    "--cache-expiration-minutes", "60"
]);

var config = new AppConfig();
argx.Bind(config);

Console.WriteLine($"Name: {config.Name}");
Console.WriteLine($"Max Connections: {config.MaxConnections}");
Console.WriteLine($"Host: {config.Database.Host}");
Console.WriteLine($"Port: {config.Database.Port}");
Console.WriteLine($"Username: {config.Database.Username}");
Console.WriteLine($"Use SSL: {config.Database.UseSSL}");
Console.WriteLine($"Tags: {string.Join(", ", config.Database.Tags)}");
Console.WriteLine($"Cache Provider: {config.Cache.Provider}");
Console.WriteLine($"Cache Expiration Minutes: {config.Cache.ExpirationMinutes}");

public class AppConfig
{
    public string Name { get; set; } = null!;
    public int MaxConnections { get; set; }
    public DatabaseConfig Database { get; set; } = null!;
    public CacheConfig Cache { get; set; } = null!;
}

public class DatabaseConfig
{
    public string Host { get; set; } = null!;
    public int Port { get; set; }
    public string Username { get; set; } = null!;
    public bool UseSSL { get; set; }

    public List<string> Tags { get; set; } = [];
}

public class CacheConfig
{
    public string Provider { get; set; } = null!;
    public int ExpirationMinutes { get; set; }
}