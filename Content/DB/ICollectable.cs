using Knucklebones.DB;

namespace Knucklebones;

public interface ICollectable
{
    public int RequiredCoins { get; set; }
    public virtual string Purchase(UserData userdata) => string.Empty;
}