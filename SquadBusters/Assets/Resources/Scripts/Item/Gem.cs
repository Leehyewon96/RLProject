public class Gem : Item
{
    protected override void Awake()
    {
        base.Awake();
        type = ItemType.Gem;
    }
}
