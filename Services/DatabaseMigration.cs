using ShoppingList002.Models.DbModels;
using ShoppingList002.Models.DbModels.ShoppingList002.Models.DbModels;
using ShoppingList002.Services.Converter;
using ShoppingList002.Services.Converters;
using System;
using System.Threading.Tasks;


namespace ShoppingList002.Services
{
    public class DatabaseMigration
    {
        private readonly IDatabaseService _databaseService;

        public DatabaseMigration(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task MigrateIfNeededAsync()
        {
            var currentVersion = await _databaseService.GetCurrentDbVersionAsync();

            while (true)
            {
                if (currentVersion == "1.0.0")
                {
                    await Migrate_1_0_0_to_1_0_1();
                    currentVersion = "1.0.1";
                    await _databaseService.SetVersionAsync(currentVersion);
                    continue;
                }

                // これ以降のバージョンアップが追加されたらここに追記

                break; // 最新状態ならループ終了
            }
            currentVersion = await _databaseService.GetCurrentDbVersionAsync();

            if (currentVersion == "1.0.1")
            {
                await Migrate_1_0_1_to_1_0_2();
                await _databaseService.SetVersionAsync("1.0.2");
            }
            currentVersion = await _databaseService.GetCurrentDbVersionAsync();
            if (currentVersion == "1.0.2")
            {
                await Migrate_1_0_2_to_1_1_0();
                await _databaseService.SetVersionAsync("1.1.0");
            }
            currentVersion = await _databaseService.GetCurrentDbVersionAsync();
            if (currentVersion == "1.1.0")
            {
                await Migrate_1_1_0_to_1_2_0();
                await _databaseService.SetVersionAsync("1.2.0");
            }

            // 将来的にはここに追加されていく形：
            // if (currentVersion == "1.0.2") { await Migrate_1_0_2_to_2_0_0(); ... }
        }
        private async Task Migrate_1_0_0_to_1_0_1()
        {
            // AppSetting テーブルが存在しない前提で作成
            await _databaseService.CreateTableAsync<AppSettingDbModel>();

            // デフォルト設定追加
            var setting = new AppSettingDbModel
            {
                Key = "SoftDeleteRetentionDays",
                Value = "365",
                UpdatedDate = DateTimeOffset.Now
            };
            await _databaseService.InsertOrReplaceAsync(setting);
        }
        private async Task Migrate_1_0_1_to_1_0_2()
        {
            Console.WriteLine("🔧 マイグレーション開始: 1.0.1 → 1.0.2");

            await _databaseService.CreateTableAsync<ActivityLogDbModel>();

            Console.WriteLine("✅ マイグレーション完了: 1.0.2 に更新");
        }
        private async Task Migrate_1_0_2_to_1_1_0()
        {
            Console.WriteLine("🔧 マイグレーション開始: 1.0.2 → 1.1.0");

            var conn = await _databaseService.GetConnectionAsync();

            try
            {
                // カラム追加（既存なら落ちる→catchでスキップ）
                await conn.ExecuteAsync("ALTER TABLE CandidateListItem ADD COLUMN Kana TEXT");
                await conn.ExecuteAsync("ALTER TABLE CandidateListItem ADD COLUMN SearchKana TEXT");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ カラム追加スキップまたは失敗: {ex.Message}");
            }

            // 🔁 ここから先の補完は“今回決めた統一ロジック”で一括
            var fixedCount = await KanaFieldUpdater.RebuildAllAsync(_databaseService);
            Console.WriteLine($"🛠 Kana/SearchKana 再計算: {fixedCount} 件更新");

            Console.WriteLine("✅ マイグレーション完了: 1.1.0 に更新");
        }
        private async Task Migrate_1_1_0_to_1_2_0()
        {
            Console.WriteLine("🔧 マイグレーション開始: 1.1.0 → 1.2.0");

            // ① 作業用テーブル作成（SQLite-net に任せる）
            await _databaseService.CreateTableAsync<ShoppingListItem_WorkDbModel>();

            var conn = await _databaseService.GetConnectionAsync();

            // ② 既存データをコピー（IsMemo は全部 false）
            await conn.ExecuteAsync(@"
                INSERT INTO ShoppingListItem_Work
                (Id, ItemId, Name, Detail, AddedDate, UpdatedDate, Status, IsMemo)
                SELECT
                    Id, ItemId, Name, Detail, AddedDate, UpdatedDate, Status, 0
                FROM ShoppingListItem;
            ");

            // ③ 旧テーブル削除
            await conn.ExecuteAsync("DROP TABLE ShoppingListItem;");

            // ④ 作業用テーブルを本名に変更
            await conn.ExecuteAsync(
                "ALTER TABLE ShoppingListItem_Work RENAME TO ShoppingListItem;"
            );

            Console.WriteLine("✅ マイグレーション完了: 1.2.0");
        }

    }

}
