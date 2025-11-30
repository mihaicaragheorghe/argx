#!/usr/bin/env dotnet --

#:project ../src/Argx

using Argx;
using Argx.Actions;
using Argx.Extensions;
using Argx.Parsing;

var parser = new ArgumentParser();
parser.Add("--host", dest: "host", usage: "Database host");
parser.Add<bool>("--use-ssl", dest: "use-ssl", action: ArgumentActions.StoreTrue, usage: "Use SSL for the connection");
parser.Add<int>("--port", dest: "port", usage: "Database port");
parser.Add("--username", dest: "username", usage: "Database username");
parser.Add<List<string>>("--tags", dest: "tags", arity: Arity.Any, usage: "Tags for the database");

var argx = parser.Parse(
[
    "--host", "localhost",
    "--port", "5432",
    "--username", "admin",
    "--use-ssl",
    "--tags", "production", "primary"
]);

var config = new DatabaseConfig();
argx.Bind(config);

Console.WriteLine($"Host: {config.Host}");
Console.WriteLine($"Port: {config.Port}");
Console.WriteLine($"Username: {config.Username}");
Console.WriteLine($"Use SSL: {config.UseSSL}");
Console.WriteLine($"Tags: {string.Join(", ", config.Tags)}");

public class DatabaseConfig
{
    public string Host { get; set; } = null!;
    public int Port { get; set; }
    public string Username { get; set; } = null!;
    public bool UseSSL { get; set; }

    public List<string> Tags { get; set; } = [];
}