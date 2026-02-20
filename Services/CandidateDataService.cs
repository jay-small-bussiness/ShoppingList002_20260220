using ShoppingList002.Services.Converters;
using ShoppingList002.Models.DbModels;
using ShoppingList002.Models.UiModels;

namespace ShoppingList002.Services
{
    public class CandidateDataService : ICandidateDataService
    {
        private readonly IDatabaseService _dbService;
        private bool _initialized;
        private int _maxColorID;
        private int _minColorID;
        private const int DefaultColorId = 1;

        public IReadOnlyList<CandidateCategoryUiModel> Categories { get; private set; }
        public IReadOnlyList<CandidateListItemUiModel> Items { get; private set; }
        public IReadOnlyList<ColorUiModel> Colors { get; private set; }



        public CandidateDataService(IDatabaseService dbService)
        {
            _dbService = dbService;
        }

        public async Task EnsureInitializedAsync()
        {
            if (_initialized) return;

            // ここでカテゴリ・候補をDBからロード
            // キャッシュ構築とかもここ
            await LoadCandidateDataAsync();
            Colors = await LoadColorsAsync(); // ← 追加
            _maxColorID = Colors.Any()
                ? Colors.Max(c => c.ColorId)
                : DefaultColorId;
            _minColorID = Colors.Any()
                ? Colors.Min(c => c.ColorId)
                : DefaultColorId;
            _initialized = true;
        }

        private async Task LoadCandidateDataAsync()
        {
            // ① DBからカテゴリ取得（表示順で）
            var categoryDbList = await _dbService.GetTable<CandidateCategoryDbModel>()
                                          .OrderBy(c => c.DisplayOrder)
                                          .ToListAsync();

            // ② DBから候補アイテム取得
            var itemDbList = await _dbService.GetTable<CandidateListItemDbModel>()
                                      .ToListAsync();

            // ③ UIモデルに変換
            var categories = categoryDbList
                .Select(c => CandidateCategoryModelConverter.DbToUiModel(c))
                .ToList();

            var items = itemDbList
                .Select(i => CandidateListItemModelConverter.DbToUiModel(i))
                .ToList();

            // ④ カテゴリごとにアイテム紐づけ
            foreach (var cat in categories)
            {
                cat.Items = items
                    .Where(i => i.CategoryId == cat.CategoryId)
                    .OrderBy(i => i.DisplaySeq)
                    .ToList();
            }

            // ⑤ メモリに保持
            Categories = categories;
            Items = items;
        }
        public void AddCategory(CandidateCategoryUiModel newCategory)
        {
            ((List<CandidateCategoryUiModel>)Categories).Add(newCategory);
        }
        public void RemoveCategory(int categoryId)
        {
            var list = (List<CandidateCategoryUiModel>)Categories;
            var target = list.FirstOrDefault(x => x.CategoryId == categoryId);
            if (target != null)
            {
                list.Remove(target);
            }
        }
        public void ReplaceCategory(CandidateCategoryUiModel updated)
        {
            var list = (List<CandidateCategoryUiModel>)Categories;
            var index = list.FindIndex(x => x.CategoryId == updated.CategoryId);
            if (index >= 0)
            {
                list[index] = updated;
            }
        }

        public void AddCandidateListItem(CandidateListItemUiModel newItem)
        {
            ((List<CandidateListItemUiModel>)Items).Add(newItem);
        }

        public void RemoveCandidateListItem(int itemId)
        {
            var list = (List<CandidateListItemUiModel>)Items;
            var target = list.FirstOrDefault(x => x.ItemId == itemId);
            if (target != null)
            {
                list.Remove(target);
            }
        }

        public void ReplaceCandidateListItem(CandidateListItemUiModel updated)
        {
            var list = (List<CandidateListItemUiModel>)Items;
            var index = list.FindIndex(x => x.ItemId == updated.ItemId);
            if (index >= 0)
            {
                list[index] = updated;
            }
        }
        private async Task<List<ColorUiModel>> LoadColorsAsync()
        {
            // ColorMaster を全部読む
            var dbColors = await _dbService.GetAllAsync<ColorMasterDbModel>();
            _maxColorID = -1;
            // 表示順で揃えて UI モデルに変換
            return dbColors
                //.OrderBy(c => c.DisplayOrder)
                .Select(c => new ColorUiModel
                {
                    ColorId = c.ColorId,
                    Name = c.Name,
                    //Color = Color.FromArgb(c.ColorCode)
                })
                .ToList();
        }

        public int GetNextCategoryColorId()
        {
            if (!Categories.Any())
                return DefaultColorId;

            var lastColorId = Categories
                .OrderByDescending(x => x.DisplayOrder)
                .First()
                .ColorId;

            var next = lastColorId + 1;
            if (next > _maxColorID)
            {
                next = _minColorID;
            }
            return next;
        }
        public int GetNextDisplayOrder()
        {
            var last = Categories
                .OrderByDescending(x => x.DisplayOrder)
                .FirstOrDefault();

            return last?.DisplayOrder + 1 ?? 0;
        }
    }

}
