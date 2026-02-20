using ShoppingList002.Models.Dto;

namespace ShoppingList002.Services.Sync
{
    public interface IAuthApiService
    {
        Task<SyncContextDto> GetSyncContextAsync();
    }

}
