using ShoppingList002.Models;
using ShoppingList002.Models.DbModels;
using ShoppingList002.Services;

namespace ShoppingList002.Repositories
{
    public class ColorMasterRepository : IColorMasterRepository
    {
        private readonly IDatabaseService _databaseService;
        public ColorMasterRepository(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
        }
        public async Task<Dictionary<int, ColorSet>> GetColorSetMapAsync()
        {
            return await _databaseService.GetColorSetMapAsync();
        }
    }
}
