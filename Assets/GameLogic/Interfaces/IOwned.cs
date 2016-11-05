public interface IOwned
{
    PlayerCityController GetOwnerCity();
    void SetAndSyncOwnerCity(string connectionId);
}