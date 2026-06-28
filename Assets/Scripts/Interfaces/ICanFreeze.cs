public interface ICanFreeze
{
    bool IsFrozen { get; }
    void Freeze();
    void Unfreeze();
}