public abstract class EnemyState
{
    protected EnemyController enemy;
    public EnemyState(EnemyController enemy) { this.enemy = enemy; }
    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void FixedUpdate() { }
    public virtual void Exit() { }
}