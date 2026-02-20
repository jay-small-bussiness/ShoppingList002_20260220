using ShoppingList002.Models.DbModels;
using ShoppingList002.Services;

namespace ShoppingList002.Repositories
{
    public class ShoppingListRepository : IShoppingListRepository
    {
        private readonly IDatabaseService _databaseService;
        public ShoppingListRepository(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
        }
        // 有効（未購入・未キャンセル）の ItemId 指定取得
        public Task<ShoppingListItemDbModel?> GetActiveByItemIdAsync(int itemId)
        {
            return _databaseService.GetFirstOrDefaultAsync<ShoppingListItemDbModel>(
                x => x.ItemId == itemId && x.Status == null
            );
        }

        // 名前指定（already 判定用）
        public Task<ShoppingListItemDbModel?> GetByNameAsync(string itemName)
        {
            return _databaseService.GetFirstOrDefaultAsync<ShoppingListItemDbModel>(
                x => x.Name == itemName && x.Status == null
            );
        }

        // 追加
        public Task InsertAsync(ShoppingListItemDbModel item)
        {
            return _databaseService.InsertAsync(item);
        }

        // 更新
        public Task UpdateAsync(ShoppingListItemDbModel item)
        {
            return _databaseService.UpdateAsync(item);
        }
        public Task<bool> ExistsActiveByItemIdAsync(int itemId)
        {
            return _databaseService.ExistsAsync<ShoppingListItemDbModel>(
                x => x.ItemId == itemId && x.Status == null
            );
        }
        public Task<List<ShoppingListItemDbModel>> QueryAsync(string sql, int itemId)
        {
            //string sql = "SELECT * FROM ShoppingListItem WHERE ItemId = ? AND Status IS NULL";
            return _databaseService.QueryAsync<ShoppingListItemDbModel>(sql, itemId);
        }
        // 有効アイテム一覧
        //public Task<List<ShoppingListItemDbModel>> GetActiveItemsByItemIDAsync(int itemId)
        //{
        //    string sql = "SELECT * FROM ShoppingListItem WHERE ItemId = ? AND Status IS NULL";
        //    return _databaseService.QueryAsync<ShoppingListItemDbModel>(sql, itemId);
        //}
        // 有効アイテム一覧
        public Task<List<ShoppingListItemDbModel>> GetActiveItemsAsync()
        {
            return _databaseService.QueryAsync<ShoppingListItemDbModel>(
            "SELECT * FROM ShoppingListItem WHERE Status IS NULL OR Status = ''"
            );
        }
        public async Task<ShoppingListItemDbModel?> GetActiveShoppingItemAsync(int itemId)
        {
            return await _databaseService.GetFirstOrDefaultAsync<ShoppingListItemDbModel>(
                x => x.ItemId == itemId && x.Status == null);
        }
        public async Task<ShoppingListItemDbModel?> GetFirstOrDefaultAsync(DateTime limit)
        {
            //var latest = await _databaseService.QueryFirstOrDefaultAsync<ShoppingListItemDbModel>(
            //    "SELECT * FROM ShoppingListItem WHERE Status = '済' AND UpdatedDate >= ? ORDER BY UpdatedDate DESC LIMIT 1", limit);
            return await _databaseService.QueryFirstOrDefaultAsync<ShoppingListItemDbModel>(
                "SELECT * FROM ShoppingListItem WHERE Status = '済' AND UpdatedDate >= ? ORDER BY UpdatedDate DESC LIMIT 1", limit);
        }
        //レコード削除
        public async Task DeleteShoppingListItemAsync(int Id)
        {
            await _databaseService.DeleteShoppingListItemAsync(Id);
        }
        // キャンセル
        public async Task CancelAsync(int itemId)
        {
            var item = await GetActiveByItemIdAsync(itemId);
            if (item == null) return;

            item.Status = "C";
            item.UpdatedDate = DateTimeOffset.Now;
            await UpdateAsync(item);
        }

        // 購入済みにする
        public async Task MarkAsPurchasedAsync(int itemId)
        {
            var item = await GetActiveByItemIdAsync(itemId);
            if (item == null) return;

            item.Status = "済";
            item.UpdatedDate = DateTimeOffset.Now;
            await UpdateAsync(item);
        }
        public async Task ReplaceShoppingListAsync(List<ShoppingListItemDbModel> items)
        {
            await _databaseService.DeleteAllShoppingListItemAsync();

            foreach (var item in items)
            {
                await _databaseService.InsertAsync(item);
            }
        }

    }
}
