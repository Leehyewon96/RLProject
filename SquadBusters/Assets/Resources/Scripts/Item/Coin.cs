public class Coin : Item
{
    protected override void Awake()
    {
        base.Awake();
        type = ItemType.Coin;
    }
}
