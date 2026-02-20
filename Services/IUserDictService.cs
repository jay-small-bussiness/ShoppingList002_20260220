using System.Text;
using ShoppingList002.Models.DicModels;

namespace ShoppingList002.Services
{
    public interface IUserDictService
    {
        Task InitializeAsync(string filePath);
        UserDictEntry? FindEntry(string surface);
        IEnumerable<UserDictEntry> GetAll();
    }
}
