using ShoppingList002.Models.DbModels;

namespace ShoppingList002.Models.Results
{
    public record AddItemResult
    {
        public bool IsAdded { get; init; }
        public bool IsAlready { get; init; }
        public ShoppingListItemDbModel? Item { get; init; }

        public static AddItemResult Added(ShoppingListItemDbModel item)
            => new() { IsAdded = true, Item = item };

        public static AddItemResult Already(ShoppingListItemDbModel item)
            => new() { IsAlready = true, Item = item };
    }

}
