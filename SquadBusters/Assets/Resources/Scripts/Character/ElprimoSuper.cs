public class ElprimoSuper : ElPrimo
{
    protected override void Awake()
    {
        base.Awake();
        flyingElbowDamage = 150f;
        characterLevel = CharacterLevel.Super;
    }
}
