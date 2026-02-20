using System.Text.Json;
using ShoppingList002.Models.Dto;

namespace ShoppingList002.Services.Sync
{
    public class AuthApiService : IAuthApiService
    {
        private readonly HttpClient _http;

        public AuthApiService(HttpClient http)
        {
            _http = http;
        }

        public async Task<SyncContextDto> GetSyncContextAsync()
        {
            //return await _shoppingListApiService.GetSyncContextAsync();
            ////return Task.FromResult(new SyncContextDto
            ////{
            ////    Plan = "Family",
            ////    FamilyId = 1
            ////});
            var res = await _http.GetAsync("/me/sync-context");
            res.EnsureSuccessStatusCode();

            var json = await res.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<SyncContextDto>(json)!;
        }
    }

}
