using ShoppingList002.Models.DbModels;

namespace ShoppingList002.Repositories
{
    public interface IShoppingListRepository
    {
        Task<ShoppingListItemDbModel?> GetActiveByItemIdAsync(int itemId);
        Task<ShoppingListItemDbModel?> GetByNameAsync(string itemName);
        Task<List<ShoppingListItemDbModel>> GetActiveItemsAsync();
        Task<List<ShoppingListItemDbModel>> QueryAsync(string sql, int itemId);
        //Task<List<ShoppingListItemDbModel>> GetActiveItemsByItemIDAsync(int itemId);
        Task InsertAsync(ShoppingListItemDbModel item);
        Task UpdateAsync(ShoppingListItemDbModel item);
        //Task DeleteAllAsync();
        Task<bool> ExistsActiveByItemIdAsync(int itemId);
        Task<ShoppingListItemDbModel?> GetActiveShoppingItemAsync(int itemId);
        Task DeleteShoppingListItemAsync(int Id);
        Task<ShoppingListItemDbModel?> GetFirstOrDefaultAsync(DateTime limit);
        Task ReplaceShoppingListAsync(List<ShoppingListItemDbModel> items);
    }

}
