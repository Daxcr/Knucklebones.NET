using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace Knucklebones.DB;

public class DatabaseContext : DbContext
{
    public DbSet<ServerSettings> Servers { get; set; }
    public DbSet<UserData> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseSqlite("Data Source=knucklebones.db");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserData>()
            .HasKey(u => u.UserID);

        modelBuilder.Entity<ServerSettings>()
            .HasKey(s => s.ServerID);

        modelBuilder.Entity<UserData>()
            .Property(u => u.Inventory)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<ICollectable>>(v, (JsonSerializerOptions?)null) ?? new()
            );
        
        modelBuilder.Entity<UserData>()
            .Property(u => u.ActiveBanner)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<Banner>(v, (JsonSerializerOptions?)null)
            );

        modelBuilder.Entity<UserData>()
            .Property(u => u.LastTenGames)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<GameMetadata>>(v, (JsonSerializerOptions?)null) ?? new()
            );
    }
}
public static class Database
{
    public static DatabaseContext Create() => new DatabaseContext();
    public async static Task<UserData> GetUser(ulong uid, DatabaseContext db)
    {
        UserData? user = await db.Users.FindAsync(uid);
        if (user == null)
        {
            user = new() { UserID = uid };
            db.Users.Add(user);
            await db.SaveChangesAsync();
        }

        return user;
    }

    public async static Task<ServerSettings> GetGuild(ulong gid, DatabaseContext db)
    {
        ServerSettings? server = await db.Servers.FindAsync(gid);
        if (server == null)
        {
            server = new() { ServerID = gid };
            db.Servers.Add(server);
            await db.SaveChangesAsync();
        }

        return server;
    }
}