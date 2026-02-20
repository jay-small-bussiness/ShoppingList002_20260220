using ShoppingList002.Models.DbModels;
using ShoppingList002.Services;

namespace ShoppingList002.Repositories
{
    public class CategoriesRepository : ICategoriesRepository
    {
        private readonly IDatabaseService _databaseService;
        public CategoriesRepository(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
        }
        public async Task<List<CandidateCategoryDbModel>> GetAllCandidateCategoryAsync()
        {
            return await _databaseService.GetAllAsync<CandidateCategoryDbModel>(); // ←カテゴリー
        }
    }
}
