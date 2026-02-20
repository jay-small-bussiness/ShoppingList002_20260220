using SQLite;

namespace ShoppingList002.Models.DbModels
{
    [Table("ShoppingListItem")]
    public class ShoppingListItemDbModel
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int? ItemId { get; set; }   // nullable

        [NotNull]
        public string Name { get; set; } = string.Empty;

        public string? Detail { get; set; }

        [NotNull]
        public DateTimeOffset AddedDate { get; set; }

        [NotNull]
        public DateTimeOffset UpdatedDate { get; set; }

        public string? Status { get; set; }

        [NotNull]
        public bool IsMemo { get; set; } = false; // 追加
    }
}
