public class CannonItem : Item
{
    protected override void Awake()
    {
        base.Awake();
        type = ItemType.Cannon;
    }
}
