using ShoppingList002.Models.DbModels;

namespace ShoppingList002.Repositories
{
    public interface ICandidateListRepository
    {
        Task<List<CandidateListItemDbModel>> GetAllCandidateListAsync();
        Task<CandidateListItemDbModel?> GetFirstOrDefaultAsync(int itemId);
    }
}
