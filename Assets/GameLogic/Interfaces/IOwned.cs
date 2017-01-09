public interface IOwned
{
    PlayerController GetOwner();
    void SetOwner(PlayerController owner);
}