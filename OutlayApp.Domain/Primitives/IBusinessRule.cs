namespace OutlayApp.Domain.Primitives
{
    public interface IBusinessRule
    {
        bool IsBroken();
        string Message { get; }
    }
}