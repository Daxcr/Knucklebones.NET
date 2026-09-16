namespace CotLMinigames.DB.UserInventory;

public class Inventory
{
    public long ID { get; set; }
    public long Coins { get; set; } = 5;
    public long Wool { get; set; } = 5;
    public long GodTears { get; set; } = 0;
    public List<InventoryGenericItem> GenericItems { get; set; } = new();
    public List<Banner> Banners { get; set; } = new();
}