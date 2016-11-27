public interface IAttackable
{
    PlayerCityController GetOwnerCity();
    void TakeDamage(int damage);
    bool Targetable();
    int X();
    int Z();
}