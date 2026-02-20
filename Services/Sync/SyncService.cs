using ShoppingList002.Repositories;
using ShoppingList002.Services.Converters;

namespace ShoppingList002.Services.Sync
{
    public class SyncService
    {
        private readonly ShoppingListApiService _shoppingListApiService;
        private readonly IShoppingListRepository _shoppingListRepository;
        public SyncService(
            ShoppingListApiService api,
            IShoppingListRepository repo)
        {
            _shoppingListApiService = api;
            _shoppingListRepository = repo;
        }
        public async Task PullAndReplaceShoppingListAsync()
        {
            var serverItems = await _shoppingListApiService.GetShoppingListAsync();

            var dbItems = serverItems
                .Select(x => ShoppingListConverter.ToDbModel(x))
                .ToList();

            await _shoppingListRepository.ReplaceShoppingListAsync(dbItems);
        }
    }

}
