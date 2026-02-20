using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
//using Java.Util.Regex;
using ShoppingList002.Models.DbModels;
using ShoppingList002.Models.UiModels;
using ShoppingList002.Services;
using ShoppingList002.Messages;
using ShoppingList002.Services.Converters;
using ShoppingList002.ViewModels.Base;
using System.Collections.ObjectModel;
using static System.Net.Mime.MediaTypeNames;
//using Microsoft.UI.Xaml.Controls.Primitives;

namespace ShoppingList002.ViewModels;

//public partial class VoiceSearchViewModel : ObservableObject
public partial class VoiceSearchViewModel : BaseVoiceAddViewModel
{
    private readonly IUserDictService _userDictService;

    private readonly ISettingsService _settings;
    private CancellationTokenSource _cts;
    private INavigationThemeService _navTheme;

    //public Color MicStatusColor => IsListening ? Colors.Red : Colors.Gray;
    //public string MicStatusText => IsListening ? "🎤 入力中" : "";

    //private string _recognizedText;
    //private bool _isListening;
    //public bool IsListening
    //{
    //    get => _isListening;
    //    set
    //    {
    //        if (_isListening != value)
    //        {
    //            _isListening = value;
    //            OnPropertyChanged();
    //            OnPropertyChanged(nameof(MicStatusColor));
    //            OnPropertyChanged(nameof(MicStatusText));
    //        }
    //    }
    //}
    private bool _showExpandButton;
    public bool ShowExpandButton
    {
        get => _showExpandButton;
        set => SetProperty(ref _showExpandButton, value);
    }

    //public string RecognizedText
    //{
    //    get => _recognizedText;
    //    set
    //    {
    //        if (_recognizedText != value)
    //        {
    //            _recognizedText = value;
    //            OnPropertyChanged(); // ← これ
    //        }
    //    }
    //}
    public ObservableCollection<SearchResultItemModel> SearchResults { get; set; } = new();
    public ObservableCollection<string> AddedHistory { get; } = new();

    //// 🔁 ステート制御===>Baseに移行
    //public enum VoiceInputState
    //{
    //    Idle,           // 何もしていない
    //    Listening,      // マイク入力中
    //    Processing,     // 結果処理中
    //    Choosing,       // 複数候補からの選択待ち
    //    NoInput,        // 無言
    //    NotFound,       // 0件ヒット
    //    Done            // 終了
    //}

    //[ObservableProperty]===>Baseに移行
    //private VoiceInputState currentState = VoiceInputState.Idle;
    // 追加：UI文言
    [ObservableProperty] private string modeChipText = "モード：通常検索";
    [ObservableProperty] private string promptText = "追加する品名を話してください";
    //[ObservableProperty] private string hintText = "例：『トマト』『牛乳』／『カテゴリー ○○』でページへ／『おしまい』で終了";
    [ObservableProperty] private string hintText = "入力中";


    //protected readonly ISpeechToTextService _speech;
    //protected readonly ISoundService _sound;
    //protected readonly ICandidateService _candidate;
    //protected readonly IShoppingListService _shopping;
    // 🔧 DIサービス
    //private readonly ISpeechToTextService _speechService;
    //private readonly ISoundService _soundService;
    //private readonly ICandidateService _candidateService;
    private readonly IShoppingListService _shoppingListService;
    private readonly IActivityLogService _logService;
    private readonly ICandidateDataService _candidateDataService;
    private readonly CandidateCategoryViewModel _candidateCategoryViewModel;
    public event EventHandler<int> CategoryCreated;
    private bool _isCreatingCategory = false;

    public VoiceSearchViewModel(
        CandidateCategoryViewModel candidateCategoryViewModel,
        INavigationThemeService navigationThemeService,
        ISpeechToTextService speechService,
        ISoundService soundService,
        ICandidateService candidateService,
        IShoppingListService shoppingListService,
        IActivityLogService logService,
        IUserDictService userDictService,
        IDatabaseService databaseService,
        ICandidateDataService candidateDataService,
        ISettingsService settings) // ★ 追加
        : base(speechService, soundService, navigationThemeService, candidateService, databaseService, shoppingListService)
    {
        _candidateCategoryViewModel = candidateCategoryViewModel;
        //_speechService = speechService;
        //_soundService = soundService;
        //_candidateService = candidateService;
        _shoppingListService = shoppingListService;
        _logService = logService;
        _userDictService = userDictService;
        _settings = settings; // ★ 追加
        _navTheme = navigationThemeService;
        _candidateDataService = candidateDataService;
        // 応答形式の初期値（Preferences）
        SelectedVoiceModeIndex = _settings.LoadVoiceFeedbackMode() == VoiceFeedbackMode.BeepOnly ? 0 : 1;

        UpdateUiTexts(""); // ★ 初期表示文言
    }

    // 📋 候補一覧（複数ヒット時）
    public ObservableCollection<CandidateListItemUiModel> CandidateItems { get; } = new();

    // 📝 入力文字列保持（0件時の処理で使う）
    [ObservableProperty]
    private string lastRecognizedText;
    //[RelayCommand]
    //private async Task RetryAsync()
    //{
    //    // “やり直し”：結果クリア→Idle→再聴取
    //    SearchResults.Clear();
    //    RecognizedText = "";
    //    CurrentState = VoiceInputState.Idle;
    //    UpdateUiTexts();
    //    await Task.Delay(100);
    //    if (!_allowAutoRestart) return;   // ← 追加
    //    await StartListeningAsync();
    //}

    //[RelayCommand]
    //private async Task CloseAsync()
    //{
    //    // “おしまい”：終了表示→前ページへ戻る
    //    _allowAutoRestart = false;         // ← 自動再開を抑止
    //    _cts?.Cancel();                    // ← 認識を止める（対応してるなら）
    //    IsListening = false;

    //    CurrentState = VoiceInputState.Done;
    //    UpdateUiTexts();
    //    IsListening = false;
    //    RecognizedText = "音声入力を終了しました";
    //    await Task.Delay(600);
    //    await Shell.Current.GoToAsync("..");
    //}

    //[RelayCommand]
    //public async Task StopListeningAsync()
    //{
    //    _allowAutoRestart = false;         // ← 同上
    //    _cts?.Cancel();
    //    IsListening = false;
    //    CurrentState = VoiceInputState.Idle;
    //    UpdateUiTexts();
    //}

    protected override void OnStateChanged(VoiceInputState newState)
    {
        Console.WriteLine($"OnStateChanged: {newState}");
        UpdateUiTexts("");
    }


    private async Task HandleSingleHit(SearchResultItemModel item)
    {
        bool already = await _shoppingListService.ExistsAsync(item.ItemId);

        if (already)
        {
            _soundService.Play("already");           // 登録済みメッセージ表示（UI側で監視）
            AddedHistory.Insert(0, $"{DateTime.Now:HH:mm:ss} 「{item.ItemName}」登録済！");
        }
        else
        {
            SearchResults.Clear();
            SearchResults.Add(new SearchResultItemModel
            {
                CategoryName = item.CategoryName,
                ItemId = item.ItemId,
                BackgroundColor = item.BackgroundColor,
                ItemName = item.ItemName
            });

            _soundService.Play("added");
            await _shoppingListService.AddItemAsync(item.ItemId);
            AddedHistory.Insert(0, $"{DateTime.Now:HH:mm:ss} 「{item.ItemName}」追加！");

            await _logService.LogAsync("ADD", item.ItemId, item.ItemName, "", "");

        }

        await Task.Delay(500);
        if (!_allowAutoRestart) return;   // ← 追加
        //await StartListeningAsync();
    }

    private async Task HandleMultipleHits(List<SearchResultItemModel> items)
    {
        CurrentState = VoiceInputState.Choosing;
        UpdateUiTexts("");
        await EndSessionAsync();                    // ← まず自動再開を完全停止

        SearchResults.Clear();
        //CandidateItems.Clear();
        foreach (var item in items)
            //CandidateItems.Add(item);
            SearchResults.Add(new SearchResultItemModel
            {
                CategoryName = item.CategoryName,
                ItemId = item.ItemId,
                BackgroundColor = item.BackgroundColor,
                ItemName = item.ItemName
            });
        _soundService.Play("multiple");
    }
    private async Task HandleNull()
    {
        SearchResults.Clear();
        CurrentState = VoiceInputState.NotFound;
        
        UpdateUiTexts(RecognizedText);
        //HintText = RecognizedText;
        _soundService.Play("nohit");

        await Task.Delay(1200);
        CurrentState = VoiceInputState.Listening;
        UpdateUiTexts("");
        if (!_allowAutoRestart) return;   // ← 追加
        //await StartListeningAsync();
    }
    private bool TryParseMemoAddCommand(string input, out string MemoString)
    {
        MemoString = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        input = input.Trim();

        if (input.StartsWith("メモ追加") || input.StartsWith("メモ 追加"))
        {
            MemoString = input.Replace("メモ追加", "")
                                .Replace("メモ 追加", "")
                                .Trim();
            return !string.IsNullOrEmpty(MemoString);
        }
        return false;
    }

    private bool TryParseCategoryAddCommand(string input, out string categoryName)
    {
        categoryName = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        input = input.Trim();

        if (input.StartsWith("カテゴリー追加") || input.StartsWith("カテゴリ追加"))
        {
            categoryName = input.Replace("カテゴリー追加", "")
                                .Replace("カテゴリ追加", "")
                                .Trim();
            return !string.IsNullOrEmpty(categoryName);
        }
        return false;
    }
    private bool TryParseCategoryCommand(string input, out string categoryName)
    {
        categoryName = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        input = input.Trim();

        if (input.StartsWith("カテゴリー") || input.StartsWith("カテゴリ"))
        {
            categoryName = input.Replace("カテゴリー", "")
                                .Replace("カテゴリ", "")
                                .Trim();
            return !string.IsNullOrEmpty(categoryName);
        }

        return false;
    }

    public async Task OnItemSelectedAsync(SearchResultItemModel item)
    {
        bool already = await _shoppingListService.ExistsAsync(item.ItemId);

        if (already)
        {
            _soundService.Play("already");           // 登録済みメッセージ表示（UI側で監視）
            AddedHistory.Insert(0, $"{DateTime.Now:HH:mm:ss} 「{item.ItemName}」登録済！");
        }
        else
        {
            await _shoppingListService.AddItemAsync(item.ItemId);
            AddedHistory.Insert(0, $"{DateTime.Now:HH:mm:ss} 「{item.ItemName}」追加！");
            _soundService.Play("added");
        }
        SearchResults.Clear();

        await Task.Delay(500);
        await StartListeningAsync();
        if (!_allowAutoRestart) return;   // ← 追加
        //await StartListeningAsync();
    }
    private bool IsEndCommand(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return false;

        text = text.Trim();
        return text.Contains("終了") || text.Contains("おしまい");
    }
    public async Task SearchAsync(string input, bool loosen = false)
    {
        var results = await _candidateService.SearchByNameAsync(input);
        SearchResults = new ObservableCollection<SearchResultItemModel>(results);

        ShowExpandButton = !loosen && results.Count <= 2;
        OnPropertyChanged(nameof(ShowExpandButton));

        // SingleHitなら0.5秒後に自動追加（既存挙動）
        if (results.Count == 1 && !loosen)
        {
            await Task.Delay(500);
            await _shoppingListService.AddItemAsync(results[0].ItemId);
        }
    }

    public async Task ExpandSearchAsync()
    {
        await SearchAsync(LastRecognizedText, loosen: true);
        ShowExpandButton = false;
    }

    public async Task HandleSelection(CandidateListItemUiModel selected)
    {
        bool already = await _shoppingListService.ExistsAsync(selected.ItemId);

        if (already)
        {
            _soundService.Play("already");
        }
        else
        {
            _soundService.Play("added");
            await _shoppingListService.AddItemAsync(selected.ItemId);
            await _logService.LogAsync("ADD", selected.ItemId, selected.Name, "", "");
        }

        await Task.Delay(500);
        if (!_allowAutoRestart) return;   // ← 追加
        //await StartListeningAsync();
    }
    public List<string> VoiceModeOptions { get; } = new() { "ピンポン音のみ", "説明型" };
    private int _selectedVoiceModeIndex;
    public int SelectedVoiceModeIndex
    {
        get => _selectedVoiceModeIndex;
        set
        {
            if (_selectedVoiceModeIndex != value)
            {
                _selectedVoiceModeIndex = value;
                OnPropertyChanged();
                var mode = value == 0 ? VoiceFeedbackMode.BeepOnly : VoiceFeedbackMode.ExplainTTS;
                _settings.SaveVoiceFeedbackMode(mode);
            }
        }
    }
    protected override void UpdateUiTexts(string Result)
    {
        switch (CurrentState)
        {
            case VoiceInputState.Idle:
                ModeChipText = "モード：通常検索";
                PromptText = "追加する品名を話してください";
                HintText = "待機中です\n🎤 聴くを押してください";
                break;

            case VoiceInputState.Listening:
                ModeChipText = "モード：通常検索（聴取中）";
                PromptText = "お話しください";
                HintText = "例：『トマト』『牛乳』\n『カテゴリー ○○』でページへ\n『おしまい』で終了";
                break;

            case VoiceInputState.Choosing:
                ModeChipText = "モード：候補から選択";
                PromptText = "候補をタップしてください";
                HintText = "複数見つかりました\n候補をタップして選んでください\n";
                break;

            case VoiceInputState.NotFound:
                ModeChipText = "モード：0件";
                PromptText = Result + "は見つかりませんでした。もう一度どうぞ";
                HintText = "「" + Result + "」が見つかりません\n再度検索するには🎤 聴くをタップ\n";
                break;

            case VoiceInputState.Done:
                ModeChipText = "モード：終了";
                PromptText = "音声入力を終了しました";
                HintText = "また使うときは🎤を押してください\n\n";
                break;

            default:
                break;
        }
    }
    private async Task SpeakOrBeepAsync(string textKanaOrPlain)
    {
        var mode = _settings.LoadVoiceFeedbackMode();
        if (mode == VoiceFeedbackMode.BeepOnly)
        {
            _soundService.Play("notify"); // 既存のadded/multiple/alreadyでも可
        }
        else
        {
            try
            {
                _soundService.Play("notify");
            }
            catch
            {
                _soundService.Play("notify");
            }
        }
    }
    private async Task HandleAddNewMemoByVoiceAsync(string memo)
    {
        _soundService.Play("added");

        await _shoppingListService.AddMemoAsync(memo);
        AddedHistory.Insert(0, $"{DateTime.Now:HH:mm:ss} 「{memo}」メモ追加！");

        await _logService.LogAsync(
            "MEMO_ADD",
            null,
            memo,
            "",
            ""
        );

        await Task.Delay(500);
        if (!_allowAutoRestart) return;
    }

    //private async Task HandleAddNewMemoByVoiceAsync(string newMemoString)
    //{
    //    _soundService.Play("added");
    //    await _shoppingListService.AddItemAsync(item.ItemId);
    //    AddedHistory.Insert(0, $"{DateTime.Now:HH:mm:ss} 「{newMemoString}」追加！");

    //    await _logService.LogAsync("ADD", 0, newMemoString, "", "");

    //    await Task.Delay(500);
    //    if (!_allowAutoRestart) return;   // ← 追加
    //}
    private async Task HandleAddNewCategoryByVoiceAsync(string newCategoryName)
    {
        // 現存の最大CategoryIDを取得
        var categories = _candidateCategoryViewModel.Categories;
        int maxCategoryId = categories.Any() ? categories.Max(c => c.CategoryId) : 0;

        var newUiModel = new CandidateCategoryUiModel
        {
            Title = newCategoryName,
            IconName = "🆕", // ひとまず固定。あとで工夫OK
            ColorId = 5
        };

        var allCategories = await _candidateService.GetCandidateCategoriesAsync();
        var dbModel = CandidateCategoryModelConverter.ToDbModel(newUiModel);
        //dbModel.DisplayOrder = allCategories.Any()
        //    ? allCategories.Max(c => c.DisplayOrder) + 1
        //    : 0;
        //int nextColorId = 1;
        //if (allCategories.Any())
        //{
        //    var last = allCategories
        //        .OrderByDescending(c => c.DisplayOrder)
        //        .First();

        //    nextColorId = last.ColorId + 1;
        //    if (nextColorId > _candidateCategoryViewModel.AvailableColors.Count)
        //        nextColorId = 1;
        //}
        //dbModel.ColorId = nextColorId;
        dbModel.DisplayOrder = _candidateDataService.GetNextDisplayOrder();
        dbModel.CategoryId = 0; // 新規なので 0 にリセット
        dbModel.ColorId = _candidateDataService.GetNextCategoryColorId();

        await InsertCategoryAsync(dbModel);
        var route = $"candidatelist?categoryId={dbModel.CategoryId}&categoryTitle={dbModel.Title}&CategoryTitleWithEmoji={dbModel.IconName}&colorId={dbModel.ColorId}&fromVoice=true";
        _navTheme.BeginTheme("voiceadd");   //ボイス追加系の遷移記憶
        _navTheme.Push(route);              //対象カテゴリー詳細へのルートを記憶しておく
        await Shell.Current.GoToAsync(route);
    }
    public enum VoiceInputMode { GlobalSearch, AddToSpecificCategory }

    [ObservableProperty] private VoiceInputMode currentMode = VoiceInputMode.GlobalSearch;
    private int? _fixedCategoryId = null;

    // カテゴリページから呼ぶ（遷移時に）
    public void StartAddToSpecificCategory(int categoryId)
    {
        _fixedCategoryId = categoryId;
        CurrentMode = VoiceInputMode.AddToSpecificCategory;
        CurrentState = VoiceInputState.Idle;
        UpdateUiTexts("");

        ModeChipText = $"モード：カテゴリ連続追加（ID={categoryId}）";
        PromptText = "追加するアイテムを話してください";
        HintText = "例：『あらびき黒こしょう』『ねりからし』／『おしまい』で終了";
    }
    public async Task InsertCategoryAsync(CandidateCategoryDbModel dbModel)
    {
        if (_isCreatingCategory)
            return;

        _isCreatingCategory = true;

        await _candidateCategoryViewModel.InsertCategoryAsync(dbModel);




        //WeakReferenceMessenger.Default.Send(
        //    new VoiceSearch_VM_to_VoiceSearchPage_CategoryCreatedMessage(dbModel.CategoryId)
        //);
        //WeakReferenceMessenger.Default.Send(
        //    new VoiceSearch_VM_to_CandidateCategoryPage_CategoryCreatedMessage(dbModel.CategoryId)
        //);

        _isCreatingCategory = false;
    }
    protected override async Task<bool> ProcessRecognizedTextAsync(string result)
    {
        CurrentState = VoiceInputState.Listening ;
        UpdateUiTexts("");

        // ✅ メモ追加コマンド
        if (TryParseMemoAddCommand(result, out var memoText))
        {
            await HandleAddNewMemoByVoiceAsync(memoText);
            return true;
        }

        // ✅ カテゴリ追加のコマンド判定
        if (TryParseCategoryAddCommand(result, out var newCategoryName))
        {
            await HandleAddNewCategoryByVoiceAsync(newCategoryName);
            Deactivate();
            return true;
        }
        // ✅ カテゴリ指定のコマンド判定
        if (TryParseCategoryCommand(result, out var categoryName))
        {
            var category = await _candidateService.FindCategoryByNameAsync(categoryName);
            if (category != null)
            {
                Deactivate();
                var route = $"candidatelist?categoryId={category.CategoryId}&categoryTitle={category.Title}&CategoryTitleWithEmoji={category.IconName}&colorId={category.ColorId}";

                _navTheme.BeginTheme("voiceadd");   //ボイス追加系の遷移記憶
                _navTheme.Push(route);              //対象カテゴリー詳細へのルートを記憶しておく
                await Shell.Current.GoToAsync(route);
                return true;
            }
            else
            {
                // 見つからなかった場合：表示だけして終わる？
                IsListening = false;
                RecognizedText = $"「{categoryName}」カテゴリは見つかりませんでした";
                await Task.Delay(2000);
                
                return true;
            }
        }
        if (IsEndCommand(result))
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
                await Shell.Current.GoToAsync("//ShoppingListPage");

                return false;                                      // ← 以降の再試行に落ちない
            }

            // 通常の検索・追加ロジック…
            // 失敗・未ヒット時だけ：
            await RetryAsync();  // ← 再開判断は基底に一元化
            //await Task.Delay(1000);
            //await Shell.Current.GoToAsync(".."); // ← ひとつ前のページに戻る
            //return false; // 終了
        }

        var matches = await _candidateService.SearchByNameAsync(result);

        // 1) まずStrictキーの完全一致を探す（1件なら即AutoPick）
        var keyStrict = KanaHelper.ToSearchKana(result);
       
        var exacts = matches.Where(r => KanaHelper.ToSearchKana(r.ItemName) == keyStrict).ToList();
        if (exacts.Count() == 1)
        {
            await HandleSingleHit(exacts[0]);
            return true;
        }
        // 2) 完全一致が無い場合：トップ=100 かつ 2位<100 ならAutoPick
        if (matches.Count >= 1 && matches[0].Score == 100 &&
            (matches.Count == 1 || matches[1].Score < 100))
        {
            await HandleSingleHit(matches[0]);
            return true;
        }

        if (matches.Count == 1)
        {
            await HandleSingleHit(matches[0]);
            return true;
        }
        else if (matches.Count > 1)
        {

            await HandleMultipleHits(matches);
            return true;
        }
        if (matches.Count == 0)
        {
            await HandleNull();
            return true; // 再スタート
        }
        await _shopping.AddItemAsync(matches[0].ItemId);
        _soundService.Play("added");
        return true; // 継続
    }
}
