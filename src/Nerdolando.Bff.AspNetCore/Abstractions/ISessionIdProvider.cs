namespace Nerdolando.Bff.AspNetCore.Abstractions
{
    internal interface ISessionIdProvider
    {
        Task<Guid?> GetSessionIdFromCurrentContextAsync();
    }
}
