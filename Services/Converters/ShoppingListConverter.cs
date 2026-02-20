using ShoppingList002.Models.DbModels;
using ShoppingList002.Models.Dto;

namespace ShoppingList002.Services.Converters
{
    public static class ShoppingListConverter
    {
        public static ShoppingListItemDbModel ToDbModel(ShoppingListDto dto)
        {
            return new ShoppingListItemDbModel
            {
                // ローカルDBのIdは自動採番なのでセットしない

                ItemId = dto.ItemId,          // マスターItemID
                Name = dto.Name,
                Detail = null,                // サーバー非管理
                AddedDate = dto.AddedAt,      // サーバーの値を信じる
                UpdatedDate = dto.UpdatedAt,
                Status = dto.Status,          // ← そのまま使う
                IsMemo = dto.IsMemo == 1           
            };
        }

    }

}
