namespace CotLMinigames.DB.UserInventory;

public class Banner
{
    public long ID { get; set; }
    public required int Required { get; set; }
    public required string Name { get; set; }
    public required string URL { get; set; }
    public int RequiredCurrency { get; set; }
    public Currency UseCurrency { get; set; }
}