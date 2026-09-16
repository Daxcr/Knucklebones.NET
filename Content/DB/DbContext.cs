using System.Text.Json;
using CotLMinigames.Knucklebones;
using Microsoft.EntityFrameworkCore;

namespace CotLMinigames.DB;

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
    }
}
public static class Database
{
    public static DatabaseContext Create() => new DatabaseContext();
    public async static Task<UserData> GetUser(ulong uid, DatabaseContext db)
    {
        UserData? user = await db.Users
            .Include(u => u.Inventory)
            .FirstOrDefaultAsync(u => u.UserID == uid);

        if (user == null)
        {
            user = new() { UserID = uid };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            user = await db.Users
                .Include(u => u.Inventory)
                .FirstAsync(u => u.UserID == uid);
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