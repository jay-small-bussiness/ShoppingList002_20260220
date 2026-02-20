using SQLite;

namespace ShoppingList002.Models.DbModels
{
    namespace ShoppingList002.Models.DbModels
    {
        [Table("ShoppingListItem_Work")]
        public class ShoppingListItem_WorkDbModel
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }

            public int? ItemId { get; set; }   // nullable に変更

            [NotNull]
            public string Name { get; set; } = string.Empty;

            public string? Detail { get; set; }

            [NotNull]
            public DateTimeOffset AddedDate { get; set; }

            [NotNull]
            public DateTimeOffset UpdatedDate { get; set; }

            public string? Status { get; set; }

            [NotNull]
            public bool IsMemo { get; set; }    // 新規追加
        }
    }
}