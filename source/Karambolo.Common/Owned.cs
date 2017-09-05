namespace Karambolo.Common
{
    public interface IOwned<out TOwner>
    {
        TOwner Owner { get; }
    }
}
