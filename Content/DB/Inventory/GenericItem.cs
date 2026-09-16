namespace CotLMinigames.DB.UserInventory;

public class GenericItem
{
    public long ID { get; set; }
    public string? Name { get; set; }
}
public class InventoryGenericItem
{
    public long ID { get; set; }
    public long InventoryID { get; set; }
    public Inventory? Inventory { get; set; }
    public long GenericItemID { get; set; }
    public GenericItem? GenericItem { get; set; }
    public long Count { get; set; }
}