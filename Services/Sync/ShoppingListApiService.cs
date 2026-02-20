//using Newtonsoft.Json;
using ShoppingList002.Models.Dto;
using System.Text;
using System.Text.Json;

namespace ShoppingList002.Services.Sync
{
    public class ShoppingListApiService
    {
        private readonly HttpClient _http;

        public ShoppingListApiService(HttpClient http)
        {
            _http = http;
        }
        public async Task InsertAsync(ShoppingListPostDto dto)
        {
            var json = JsonSerializer.Serialize(dto);
            var content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json"
            );

            await _http.PostAsync("/shopping-list", content);

            //var json = JsonSerializer.Serialize(dto);
            //var content = new StringContent(json, Encoding.UTF8, "application/json");

            //var res = await _http.PostAsync("/checklist", content);
            //res.EnsureSuccessStatusCode();
        }
        public async Task<List<ShoppingListDto>> GetShoppingListAsync()
        {
            //var res = await _http.GetAsync("shopping-list");
            //var res = await _http.GetAsync(
            //        "https://min-kai-server-production.up.railway.app/checklist/1"
            //    );
            var res = await _http.GetAsync("/shopping-list?family_id=1");
            res.EnsureSuccessStatusCode();

            var json = await res.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var list = JsonSerializer.Deserialize<List<ShoppingListDto>>(json, options);
            return list;
        }
    }

}
