//{
//    "id": 12,
//  "name": "牛乳",
//  "status": null,
//  "memo": "",
//  "updated_at": "2025-01-10T09:12:00Z"
//}

//using System.Text.Json.Serialization;
//[JsonPropertyName("updated_at")]
//public DateTimeOffset UpdatedAt { get; set; }
using System;
using System.Text.Json.Serialization;

namespace ShoppingList002.Models.Dto
{
    public class ShoppingListDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("family_id")]
        public int FamilyId { get; set; }

        [JsonPropertyName("item_id")]
        public int? ItemId { get; set; }

        [JsonPropertyName("category_id")]
        public int CategoryId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("is_memo")]
        public int IsMemo { get; set; }

        [JsonPropertyName("added_at")]
        public DateTimeOffset AddedAt { get; set; }

        [JsonPropertyName("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }

        [JsonPropertyName("updated_by")]
        public int UpdatedBy { get; set; }
    }
}

