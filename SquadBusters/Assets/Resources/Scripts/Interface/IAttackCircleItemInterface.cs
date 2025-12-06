public interface IAttackCircleItemInterface
{
    public void GainTreasureBox();
    public int GetCoin();
    public void SetCoin(int newCoin);
    public void OnDetectedItem(NoticeType type, Item tree);
    public void OnUnDetectedItem(Item tree);
    public void ShowNotice(NoticeType type, Item item);
}
