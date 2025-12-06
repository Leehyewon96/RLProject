public class BombItem : Item
{
    protected override void Awake()
    {
        base.Awake();
        type = ItemType.Bomb;
    }
}
