using ShoppingList002.Models.UiModels;
using ShoppingList002.Models.DbModels;
using ShoppingList002.Models.Sync;
using ShoppingList002.Repositories;
using ShoppingList002.Services.Sync;
using ShoppingList002.Models.Results;
using ShoppingList002.Models.Enums;
using ShoppingList002.Models;
using System.Reflection;
//using Android.App.Job;

namespace ShoppingList002.Services
{
    public class ShoppingListService : IShoppingListService
    {
        //private readonly IDatabaseService _databaseService;
        private readonly ActivityLogService _activityLogService;
        private readonly IShoppingListRepository _shoppingListRepository;
        private readonly ICandidateListRepository _candidateListRepository;
        private readonly ICategoriesRepository _categoriesRepository;
        private readonly IColorMasterRepository _colorMasterRepository;
        private readonly ShoppingListApiService _shoppingListApiService;
        private readonly SyncContext _syncContext;

        public ShoppingListService(
            //IDatabaseService databaseService, 
            ActivityLogService activityLogService,
            ShoppingListApiService shoppingListApiService,
            IShoppingListRepository shoppingListRepository,
            ICandidateListRepository candidateListRepository,
            ICategoriesRepository categoriesRepository,
            IColorMasterRepository colorMasterRepository,
            SyncContext syncContext)
        {
            //_databaseService = databaseService;
            _activityLogService = activityLogService;
            _shoppingListApiService = shoppingListApiService;
            _shoppingListRepository = shoppingListRepository;
            _candidateListRepository = candidateListRepository;
            _categoriesRepository = categoriesRepository;
            _colorMasterRepository = colorMasterRepository;
            _syncContext = syncContext;
        }
        private async Task<AddItemResult> AddItemInternalAsync(
            ShoppingListItemSource source,
            int? candidateItemId,
            string itemName,
            string categoryName,
            string actor,
            DateTimeOffset now)
        {
            // ① 既存チェック（already 判定）
            var existing = await _shoppingListRepository
                .GetByNameAsync(itemName);

            if (existing != null)
            {
                return AddItemResult.Already(existing);
            }

            // ② Entity生成
            var item = new ShoppingListItemDbModel
            {
                ItemId = candidateItemId,
                Name = itemName,
                Status = null,
                AddedDate = now,
                UpdatedDate = now,
            };

            // ③ ローカルDB保存
            await _shoppingListRepository.InsertAsync(item);

            // ④ ActivityLog
            await _activityLogService.LogAsync("追加",item.Id,itemName,categoryName, itemName + "追加");

            // ⑤ SyncContext 通知
            //_syncContext.MarkDirty(SyncTarget.ShoppingList);

            return AddItemResult.Added(item);
        }

        public async Task AddToShoppingListAsync(ShoppingListItemDbModel model)
        {
            // 同じItemIdですでに登録中のやつがあるかチェック
            //var existing = await _databaseService.GetFirstOrDefaultAsync<ShoppingListItemDbModel>(
            //    x => x.ItemId == model.ItemId && x.Status == null);
            var existing = await _shoppingListRepository.GetActiveByItemIdAsync(model.ItemId.Value);

            if (existing != null)
            {
                // すでに登録済なので何もしない（orトースト表示など）
                return;
            }

            var now = DateTimeOffset.Now;

            var newItem = new ShoppingListItemDbModel
            {
                ItemId = model.ItemId,
                Name = model.Name,
                Detail = model.Detail,
                AddedDate = now,
                UpdatedDate = now,
                Status = null // 登録中
            };

            await _shoppingListRepository.InsertAsync(newItem);
        }
        public async Task CancelShoppingListItemAsync(int itemId)
        {
            //var existing = await _databaseService.GetFirstOrDefaultAsync<ShoppingListItemDbModel>(
            //    x => x.ItemId == itemId && x.Status == null);
            var existing = await _shoppingListRepository.GetActiveByItemIdAsync(itemId);

            if (existing != null)
            {
                existing.Status = "C";
                existing.UpdatedDate = DateTime.Now;
                await _shoppingListRepository.UpdateAsync(existing);
            }
        }
        public async Task<bool> ExistsAsync(int itemId)
        {
            string sql = "SELECT * FROM ShoppingListItem WHERE ItemId = ? AND Status IS NULL";
            //var result = await _databaseService.QueryAsync<ShoppingListItemDbModel>(sql, itemId);
            var result = await _shoppingListRepository.QueryAsync(sql, itemId);
            return result.Any();
        }
        public async Task AddItemsAsync(IEnumerable<CandidateListItemUiModel> items)
        {
            foreach (var item in items)
            {
                // 例：すでに登録済みかチェック（重複防止）
                //var exists = await _databaseService.ExistsAsync<ShoppingListItemDbModel>(
                //    x => x.ItemId == item.ItemId && x.Status == null);
                var exists = await _shoppingListRepository
                    .ExistsActiveByItemIdAsync(item.ItemId);
                if (!exists)
                {
                    var newItem = new ShoppingListItemDbModel
                    {
                        ItemId = item.ItemId,
                        Name = item.Name,
                        Detail = item.Detail,
                        AddedDate = DateTime.Now
                    };

                    await _shoppingListRepository.InsertAsync(newItem);
                }
            }
        }

        public async Task<List<ShoppingListUiModel>> GetDisplayItemsAsync()
        {
            //var shoppingItems = await _databaseService.QueryAsync<ShoppingListItemDbModel>(
            //    "SELECT * FROM ShoppingListItem WHERE Status IS NULL OR Status = ''");
            var shoppingItems = await _shoppingListRepository.GetActiveItemsAsync();
            //var allCandidateItems = await _databaseService.GetAllAsync<CandidateListItemDbModel>();
            var allCandidateItems = await _candidateListRepository.GetAllCandidateListAsync();
            var allCategories = await _categoriesRepository.GetAllCandidateCategoryAsync(); // ←カテゴリー
            var colorMap = await _colorMasterRepository.GetColorSetMapAsync();

            var result = new List<ShoppingListUiModel>();

            foreach (var sItem in shoppingItems)
            {
                var cItem = allCandidateItems.FirstOrDefault(x => x.ItemId == sItem.ItemId);
                //if (cItem == null) continue;
                if (cItem == null)
                {
                    result.Add(new ShoppingListUiModel
                    {
                        Id = sItem.Id,
                        ItemId = null,
                        Name = sItem.Name,
                        Detail = "",
                        CategoryId = -1,
                        CategoryTitle = "List",
                        CategoryDisplayOrder = int.MaxValue,
                        ItemDisplaySeq = int.MaxValue,
                        AddedDate = sItem.AddedDate,
                        BackgroundColor = Colors.LightSkyBlue
                    });
                    continue;
                }

                var category = allCategories.FirstOrDefault(x => x.CategoryId == cItem.CategoryId);
                if (category == null) continue;

                if (!colorMap.TryGetValue(category.ColorId, out var colorSet))
                    continue;

                result.Add(new ShoppingListUiModel
                {
                    Id = sItem.Id,
                    ItemId = sItem.ItemId,
                    Name = sItem.Name,
                    Detail = cItem.Detail,
                    CategoryId = category.CategoryId,
                    CategoryTitle = category.Title,
                    CategoryDisplayOrder = category.DisplayOrder,
                    ItemDisplaySeq = cItem.DisplaySeq,
                    AddedDate = sItem.AddedDate,
                    BackgroundColor = colorSet.Unselected // ← 選択中表示にはUnselected色
                });
            }

            return result
                .OrderBy(x => x.CategoryDisplayOrder)
                .ThenBy(x => x.ItemDisplaySeq)
                .ToList();
        }
        public async Task MarkAsPurchasedAsync(int? itemId)
        {
            var item = await _shoppingListRepository.GetActiveShoppingItemAsync(itemId.Value);

            if (item != null)
            {
                item.Status = "済";
                item.UpdatedDate = DateTime.Now;
                await _shoppingListRepository.UpdateAsync(item);
            }
            await LogPurchasedAddAsync(item.Name, itemId.Value, "");
        }
        public async Task DeleteMemoAsync(ShoppingListUiModel item)
        {
            int id = item.Id;
            await _shoppingListRepository.DeleteShoppingListItemAsync(id);
        }
        public async Task<string?> UndoLastPurchasedItemAsync()
        {
            var limit = DateTime.Now.AddHours(-24);

            // 最新の「済」を1件だけ取得（UpdatedDateの降順）
            //var latest = await _databaseService.QueryFirstOrDefaultAsync<ShoppingListItemDbModel>(
            //    "SELECT * FROM ShoppingListItem WHERE Status = '済' AND UpdatedDate >= ? ORDER BY UpdatedDate DESC LIMIT 1", limit);
            var latest = await _shoppingListRepository.GetFirstOrDefaultAsync(limit);
            if (latest != null)
            {
                latest.Status = null; // 戻す
                latest.UpdatedDate = DateTime.Now;
                //await _databaseService.UpdateAsync(latest);
                await _shoppingListRepository.UpdateAsync(latest);
                return latest.Name; // ← UIに返してToast出せる
            }
            return null;
        }
        private async Task LogPurchasedAddAsync(string itemName, int itemId, string categoryName)
        {
            await _activityLogService.InsertLogAsync(
                actionType: "購入",
                itemName: itemName,
                categoryName: categoryName,
                itemId: itemId
            );
        }
        //public async Task<List<int>> GetActiveItemIdsAsync()
        //{
        //    var items = await _databaseService.QueryAsync<ShoppingListItemDbModel>(
        //        "SELECT ItemId FROM ShoppingListItem WHERE Status IS NULL OR Status = ''");

        //    return items.Select(x => x.ItemId).Distinct().ToList();
        //}
        public async Task<List<int>> GetActiveItemIdsAsync()
        {
            string sql = "SELECT ItemId FROM ShoppingListItem WHERE Status IS NULL OR Status = ''";
            var items = await _shoppingListRepository.QueryAsync(sql, 0);
            //var items = await _databaseService.QueryAsync<ShoppingListItemDbModel>(
            //    "SELECT ItemId FROM ShoppingListItem WHERE Status IS NULL OR Status = ''");

            return items
                .Where(x => x.ItemId != null)
                .Select(x => x.ItemId.Value)
                .Distinct()
                .ToList();
        }

        public async Task AddItemAsync(int itemId)
        {
            // 候補アイテム取得
            //var candidate = await _databaseService.GetFirstOrDefaultAsync<CandidateListItemDbModel>(x => x.ItemId == itemId);
            var candidate = await _candidateListRepository.GetFirstOrDefaultAsync(itemId);

            if (candidate == null)
                return; // なければスルー

            var newItem = new ShoppingListItemDbModel
            {
                Id = 0,  // AutoIncrement
                ItemId = candidate.ItemId,
                Name = candidate.Name,
                //ca  = candidate.CategoryId, // ← 修正ポイント
                AddedDate = DateTimeOffset.Now,
                UpdatedDate = DateTimeOffset.Now,
                Status = null
            };

            //await _databaseService.InsertAsync(newItem);
            await _shoppingListRepository.InsertAsync(newItem);

        }
        public async Task AddMemoAsync(string memo)
        {
            var newItem = new ShoppingListItemDbModel
            {
                Id = 0,  // AutoIncrement
                ItemId = null,
                Name = memo,
                //ca  = candidate.CategoryId, // ← 修正ポイント
                AddedDate = DateTimeOffset.Now,
                UpdatedDate = DateTimeOffset.Now,
                IsMemo = true,
                Status = null
            };

            //await _databaseService.InsertAsync(newItem);
            await _shoppingListRepository.InsertAsync(newItem);

        }
    }
}
