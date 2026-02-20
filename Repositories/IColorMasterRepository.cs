using ShoppingList002.Models;
namespace ShoppingList002.Repositories
{
    public interface IColorMasterRepository
    {
        Task<Dictionary<int, ColorSet>> GetColorSetMapAsync();
    }
}
