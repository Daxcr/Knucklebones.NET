using Knucklebones.DB;

namespace Knucklebones;

public class Banner : ICollectable
{
    public required int RequiredCoins { get; set; }
    public required string Name { get; set; }
    public required string URL { get; set; }
    string ICollectable.Purchase(UserData userdata)
    {
        if (userdata.Inventory
            .Where(item => item is Banner)
            .Any(item => (item as Banner)!.Name == Name)
        ) return "You already own this banner";

        userdata.Coins -= RequiredCoins;

        ICollectable clone = (ICollectable)MemberwiseClone();
        userdata.Inventory.Add(clone);

        return string.Empty;
    }
}