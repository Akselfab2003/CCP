namespace CCP.Sdk.utils.Abstractions
{
    public interface IKiotaApiClient<T>
    {
        T Client { get; }
    }
}
