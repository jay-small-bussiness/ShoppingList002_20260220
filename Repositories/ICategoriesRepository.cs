using ShoppingList002.Models.DbModels;

namespace ShoppingList002.Repositories
{
    public interface ICategoriesRepository
    {
        Task<List<CandidateCategoryDbModel>> GetAllCandidateCategoryAsync();
    }
}
