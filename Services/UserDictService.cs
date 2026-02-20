using System.Text;
using ShoppingList002.Models.DicModels;
using ShoppingList002.Services.Converters;
using static Android.Renderscripts.ScriptGroup;

namespace ShoppingList002.Services
{
    public class UserDictService : IUserDictService
    {
        private readonly List<UserDictEntry> _entries = new();

        public async Task InitializeAsync(string assetFileName)
        {
#if ANDROID
            using var assetStream = Android.App.Application.Context.Assets.Open(assetFileName);
            using var reader = new StreamReader(assetStream, Encoding.UTF8);
            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split(',');
                if (parts.Length >= 3)
                {
                    _entries.Add(new UserDictEntry
                    {
                        Surface = KanaHelper.ToSearchKana(parts[0].Trim()),
                        Normalized = parts[1].Trim(),
                        Reading = KanaHelper.ToSearchKana(parts[2].Trim())
                    });
                }
            }
#else
    // Windows/iOS/macOS の場合は普通にFile.ReadAllLinesでOK
    //var lines = await File.ReadAllLinesAsync(assetFileName, Encoding.UTF8);
    //foreach (var line in lines) { ... }
#endif
        }

        public UserDictEntry? FindEntry(string surface)
        {
            //return _entries.FirstOrDefault(e => e.Surface == surface);
            var normalizedInput = KanaHelper.ToSearchKana(surface);
            return _entries.FirstOrDefault(e => KanaHelper.ToSearchKana(e.Surface) == normalizedInput);
        }

        public IEnumerable<UserDictEntry> GetAll() => _entries;
    }
}
