using System.Text.Json;
using CotLMinigames.DB.UserInventory;
using CotLMinigames.Flockade;
using CotLMinigames.Knucklebones;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static CotLMinigames.Knucklebones.KBGameMetadata;

namespace CotLMinigames.DB;

public class DatabaseContext : DbContext
{
    public DbSet<ServerSettings> Servers { get; set; }
    public DbSet<UserData> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseSqlite("Data Source=knucklebones.db");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserData>(e =>
        {
            e.HasKey(u => u.UserID);

            e.HasOne(u => u.Inventory)
                .WithOne()
                .HasForeignKey<Inventory>("UserID");
        });

        modelBuilder.Entity<Inventory>(e =>
        {
            e.HasKey(i => i.ID);
            e.HasMany(i => i.GenericItems)
                .WithOne(g => g.Inventory)
                .HasForeignKey(g => g.InventoryID);
        });

        modelBuilder.Entity<GenericItem>().HasKey(g => g.ID);

        modelBuilder.Entity<InventoryGenericItem>(e =>
        {
            e.HasKey(g => g.ID);
            e.HasOne(g => g.GenericItem)
                .WithMany()
                .HasForeignKey(g => g.GenericItemID);
        });

        modelBuilder.Entity<ServerSettings>().HasKey(s => s.ServerID);

        modelBuilder.Entity<UserData>()
            .Property(u => u.LastTenGames)
            .HasConversion(
                v => GameJson.ToJson(v),
                v => GameJson.FromJson(v),
                new ValueComparer<List<GameMetadata>>(
                    (a, b) => GameJson.ToJson(a) == GameJson.ToJson(b),
                    v => GameJson.ToJson(v).GetHashCode(),
                    v => GameJson.FromJson(GameJson.ToJson(v))))
            .HasDefaultValueSql("'[]'");
    }
}
public static class GameJson
{
    static readonly JsonSerializerOptions Options = new() { IncludeFields = true };

    public static string ToJson(List<GameMetadata>? v) =>
        JsonSerializer.Serialize(v ?? new(), Options);

    public static List<GameMetadata> FromJson(string? v) =>
        string.IsNullOrEmpty(v) ? new() : JsonSerializer.Deserialize<List<GameMetadata>>(v, Options) ?? new();
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