using ShoppingList002.Models.DbModels;
using ShoppingList002.Services;
using System.Linq.Expressions;

namespace ShoppingList002.Repositories
{
    public class CandidateListRepository : ICandidateListRepository
    {
        private readonly IDatabaseService _databaseService;
        public CandidateListRepository(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
        }
        public async Task<List<CandidateListItemDbModel>> GetAllCandidateListAsync()
        {
            return await _databaseService.GetAllAsync<CandidateListItemDbModel>();

            //return await _databaseService.GetAllAsync<CandidateListItemDbModel>(
            //    x => x.Name == name);       
            //return await _databaseService.QueryFirstOrDefaultAsync<CandidateListItemDbModel?>(
            //    "SELECT * FROM CandidateListItem ");
        }
        public async Task<CandidateListItemDbModel?> GetFirstOrDefaultAsync(int itemId)
        {
            return await _databaseService.GetFirstOrDefaultAsync<CandidateListItemDbModel>(x => x.ItemId == itemId);
        }

    }
}
