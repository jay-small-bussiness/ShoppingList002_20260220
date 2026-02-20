using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShoppingList002.Services;
using ShoppingList002.Models.DbModels;
using ShoppingList002.Models.UiModels;
using ShoppingList002.Messages;
using ShoppingList002.ViewModels.Base;
using System.Collections.ObjectModel;
using static SQLite.SQLite3;

namespace ShoppingList002.ViewModels
{
    public partial class VoiceAddViewModel : BaseVoiceAddViewModel
    {
        private readonly IShoppingListService _shoppingListService;
        private readonly IActivityLogService _logService;
        private readonly IUserDictService _userDictService;
        private readonly ISettingsService _settings;
        private INavigationThemeService _navTheme;
        //private readonly INavigation _navigation;
        private readonly string _returnRoute;
        [ObservableProperty] private string modeChipText = "モード：カテゴリ内追加";
        [ObservableProperty] private string promptText = "このカテゴリに追加する品名を話してください";
        [ObservableProperty] private string hintText = "マイクボタンを押して話しかけてください";
        [ObservableProperty] private ObservableCollection<string> addedHistory = new();

        private readonly int _categoryId;
        private readonly string _categoryName;

        public VoiceAddViewModel(
            int categoryId,
            string categoryName,
            //string returnRoute,
            INavigationThemeService navigationThemeService,
            ISpeechToTextService speechService,
            ISoundService soundService,
            ICandidateService candidateService,
            IShoppingListService shoppingListService,
            IActivityLogService logService,
            IUserDictService userDictService,
            IDatabaseService databaseService,
            ISettingsService settings)
            : base(speechService, soundService, navigationThemeService, candidateService, databaseService, shoppingListService)
        {
            _categoryId = categoryId;
            _categoryName = categoryName;
            _shoppingListService = shoppingListService;
            _logService = logService;
            _userDictService = userDictService;
            _settings = settings;
            //_returnRoute = returnRoute;
            _navTheme = navigationThemeService;
        }
        private bool IsEndCommand(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return false;

            text = text.Trim();
            return text.Contains("終了") || text.Contains("おしまい");
        }
        [RelayCommand]
        private async Task StartVoiceAsync()
        {
            await StartListeningAsync();
        }
        protected override async Task<bool> ProcessRecognizedTextAsync(string recognizedText)
        {
            if (IsEndCommand(recognizedText))
            {
                await Ending();
                return false;
            }

            // 1. 無効入力スキップ
            if (string.IsNullOrWhiteSpace(recognizedText))
                return true;

            // 2. 同一カテゴリ内で重複チェック
            var existing = await _candidateService.SearchItemInCategoryAsync(_categoryId, recognizedText);

            if (existing != null)
            {
                // すでに登録済み → ログ・効果音・履歴に反映
                AddedHistory.Insert(0, $"{existing.Name} はすでに「{_categoryName}」に登録されています。");
                _soundService.Play("already.mp3");
             
                await _logService.InsertLogAsync(
                    "登録済み",
                    existing.Name,
                    _categoryName,
                    existing.ItemId
                );
                return true;
            }
            else
            {
                // 3. 未登録 → 新規候補を作成して登録
                var newItem = new CandidateListItemDbModel
                {
                    CategoryId = _categoryId,
                    Name = recognizedText,
                    Detail = string.Empty,
                    DisplaySeq = 9999, // 仮値：後で並び替え
                    Kana = null,
                    SearchKana = null,
                    DeleteFlg = 0,
                    UpdatedAt = DateTimeOffset.Now,
                    IsSynced = 0
                };

                await _databaseService.InsertAsync(newItem);

                // 4. ログ記録
                await _logService.InsertLogAsync(
                    "新規登録",
                    recognizedText,
                    _categoryName,
                    newItem.ItemId
                );

                // 5. 効果音・履歴更新
                _soundService.Play("add.mp3");
                AddedHistory.Insert(0, $"{recognizedText} を「{_categoryName}」に追加しました。");
                return true;
            }

            // 6. 再度音声入力へ（連続登録モード）
            //await RestartListeningIfNeededAsync();
            await RetryAsync();  // ← 再開判断は基底に一元化
        }
        public async Task Ending()
        {
            // 「おしまい」処理
            CurrentState = VoiceInputState.Done;
            UpdateUiTexts("");

            IsListening = false;
            RecognizedText = "音声入力を終了しました";
            if (IsEndCommand("おしまい")) // 「おしまい」「終了」「ストップ」等
            {
                await EndSessionAsync();                    // ← まず自動再開を完全停止
                                                            //await _nav.GoToAsync("//ShoppingListPage"); // ← それから遷移
                                                            //await Task.Delay(100);
                                                            //await Shell.Current.GoToAsync("..");
                                                            //await Shell.Current.Navigation.PopAsync();
                                                            //await App.Current.MainPage.Navigation.PopAsync(); // ← GoToAsync("..") やなくてこっち
                                                            //await Shell.Current.Navigation.PopToRootAsync();
                                                            //await _navigation.PopAsync();
                                                            //await Shell.Current.GoToAsync(_returnRoute);
                var returnRoute = _navTheme.Peek();
                if (!string.IsNullOrEmpty(returnRoute))
                    await Shell.Current.GoToAsync(returnRoute);


            }

            // 通常の検索・追加ロジック…
            // 失敗・未ヒット時だけ：
            await RetryAsync();  // ← 再開判断は基底に一元化
                                 //await Task.Delay(1000);
                                 //await Shell.Current.GoToAsync(".."); // ← ひとつ前のページに戻る
                                 //return false; // 終了

        }
     
    }
}
