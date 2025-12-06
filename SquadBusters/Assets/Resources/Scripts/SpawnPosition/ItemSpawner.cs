using UnityEngine;

public class ItemSpawner : Spawner
{
    [SerializeField] protected ItemType itemType = ItemType.TreasureBox;

    protected override void Awake()
    {
        base.Awake();
        repeatInterval = 20f;
        SetPath($"Prefabs/Item/{itemType.ToString()}");
    }
}
