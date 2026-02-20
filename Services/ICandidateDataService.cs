using ShoppingList002.Models.UiModels;
using ShoppingList002.Models.DbModels;

namespace ShoppingList002.Services
{
    public interface ICandidateDataService
    {
        Task EnsureInitializedAsync();
        void AddCategory(CandidateCategoryUiModel model);
        void RemoveCategory(int categoryId);
        void ReplaceCategory(CandidateCategoryUiModel updated);
        void AddCandidateListItem(CandidateListItemUiModel newItem);
        void RemoveCandidateListItem(int itemId);
        void ReplaceCandidateListItem(CandidateListItemUiModel updated);
        int GetNextCategoryColorId();
        int GetNextDisplayOrder();
    }

}
