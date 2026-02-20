using System.Text.Json.Serialization;

namespace ShoppingList002.Models.Dto
{
    public class ShoppingListPostDto
    {
        [JsonPropertyName("family_id")]
        public int FamilyId { get; set; }

        [JsonPropertyName("item_id")]
        public int ItemId { get; set; }

        [JsonPropertyName("category_id")]
        public int CategoryId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("updated_by")]
        public int UpdatedBy { get; set; }
    }
}
