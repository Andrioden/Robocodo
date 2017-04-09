public interface IAttackable
{
    PlayerController GetOwner();
    void TakeDamage(int damage);
    bool Targetable();
    int GetX();
    int GetZ();
}