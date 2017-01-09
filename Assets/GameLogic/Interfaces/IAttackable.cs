public interface IAttackable
{
    PlayerController GetOwner();
    void TakeDamage(int damage);
    bool Targetable();
    int X();
    int Z();
}