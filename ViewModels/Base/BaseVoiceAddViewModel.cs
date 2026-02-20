using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShoppingList002.Services;
using ShoppingList002.Messages;
using System.Windows.Input;
using Android.App.AppSearch;

namespace ShoppingList002.ViewModels.Base
{
    // 🔁 ステート制御
    public enum VoiceInputState
    {
        Idle,           // 何もしていない
        Listening,      // マイク入力中
        Processing,     // 結果処理中
        Choosing,       // 複数候補からの選択待ち
        NoInput,        // 無言
        NotFound,       // 0件ヒット
        Done            // 終了
    }    // 🧩 BaseVoiceAddViewModel
    public abstract partial class BaseVoiceAddViewModel : ObservableObject
    {
        protected readonly ISpeechToTextService _speech;
        protected readonly ISoundService _soundService;
        protected readonly ICandidateService _candidateService;
        protected readonly IShoppingListService _shopping;
        protected readonly IDatabaseService _databaseService;
        private INavigationThemeService _navTheme;

        protected bool _allowAutoRestart = true;
        private bool _isListening;
        private string _recognizedText;
        protected CancellationTokenSource? _listeningCts;
        private bool _isActive;
        //public ICommand StopListeningCommand { get; }
        [ObservableProperty]
        private VoiceInputState currentState = VoiceInputState.Idle;
        private int _noInputCount = 0;
        private const int MaxNoInputRetries = 5;

        protected void SetState(VoiceInputState newState)
        {
            CurrentState = newState;
            OnStateChanged(newState); // ← 派生でフックできる
        }

        protected virtual void OnStateChanged(VoiceInputState newState) { }
        public void Activate() => _isActive = true;
        public void Deactivate()
        {
            _isActive = false;
            _allowAutoRestart = false;
            _ = StopListeningAsync(); // fire & forgetでOK（例外は内部吸収）
        }
        public bool IsListening
        {
            get => _isListening;
            set
            {
                if (_isListening != value)
                {
                    _isListening = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(MicStatusColor));
                    OnPropertyChanged(nameof(MicStatusText));
                }
            }
        }
        public string RecognizedText
        {
            get => _recognizedText;
            set
            {
                if (_recognizedText != value)
                {
                    _recognizedText = value;
                    OnPropertyChanged(); // ← これ
                }
            }
        }
        public Color MicStatusColor => IsListening ? Colors.Red : Colors.LightSeaGreen;
        public string MicStatusText => IsListening ? "🎤 入力中" : "🎤×🎤×🎤";

        //[ObservableProperty]
        //private bool isListening;

        //[ObservableProperty]
        //private string recognizedText = "";

        protected BaseVoiceAddViewModel(
            ISpeechToTextService speech,
            ISoundService sound,
            INavigationThemeService navigationThemeService,
            ICandidateService candidate,
            IDatabaseService databaseService,
            IShoppingListService shopping)
            //: base (speech, sound, navigationThemeService, candidate, databaseService, shopping)
        {
            _speech = speech;
            _soundService = sound;
            _candidateService = candidate;
            _shopping = shopping;
            _databaseService = databaseService;
            _navTheme = navigationThemeService;
            //StopListeningCommand = new Command(async () => await StopListeningAsync());
        }
        [RelayCommand]
        public async Task StartListeningAsync()
        {
            if (!_isActive) return;                 // 画面非表示なら起動しない
                                                    // 同時起動ガード/権限チェック/CTS生成…（既存ロジック）
            PrepareListening();        // 入口（共通）
            var result = await _speech.RecognizeAsync();
            RecognizedText = result;
            if (string.IsNullOrWhiteSpace(result))
            {
                await HandleNoVoice();
                return;
            }

            var end = await ProcessRecognizedTextAsync(result);  // 🎯中身を派生に任せる

            FinishListening(end);      // 出口（共通）
        }
        [RelayCommand]
        public async Task StopListeningAsync()
        {
            await StopListeningCore();
            try { _listeningCts?.Cancel(); }
            catch { /* swallow */ }
            finally { _listeningCts = null; }
            // マイク解放など既存処理
        }
        public async Task StopListeningCore()
        {
            _listeningCts?.Cancel();
            _listeningCts = null;
            IsListening = false;
            UpdateUiTexts("");
        }

        protected virtual void UpdateUiTexts(string Result) { /* 何もしない */ }
        [RelayCommand]
        private async Task CloseAsync()
        {
            Deactivate();
            _listeningCts?.Cancel();
            _listeningCts = null;
            // “おしまい”：終了表示→前ページへ戻る
            IsListening = false;

            CurrentState = VoiceInputState.Done;
            UpdateUiTexts("");
            IsListening = false;
            RecognizedText = "音声入力を終了しました";
            await Task.Delay(600);
            //await Shell.Current.GoToAsync("//ShoppingListPage");
            var returnRoute = _navTheme.Peek();
            if (!string.IsNullOrEmpty(returnRoute))
                await Shell.Current.GoToAsync(returnRoute);

        }
        protected virtual void PrepareListening()
        {
            _allowAutoRestart = true;
            IsListening = true;
            RecognizedText = "";
        }
        [RelayCommand]
        public async Task RetryAsync()
        {
            // 再開前の片付けを派生に任せるフック
            await OnBeforeRestartAsync();

            if (!_allowAutoRestart || !_isActive) return;

            // 必要ならディレイもここで一元管理
            await Task.Delay(100);

            await StartListeningAsync();
        }
        private async Task HandleNoVoice()
        {
            //SearchResults.Clear();
            CurrentState = VoiceInputState.NoInput;
            UpdateUiTexts("");

            _noInputCount++;
            if (_noInputCount >= MaxNoInputRetries)
            {
                //_allowAutoRestart = false;
                //IsListening = false;
                //RecognizedText = "入力がありませんでした（終了します）";
                //return;

                await EndSessionAsync();                    // ← まず自動再開を完全停止
                                                            //await _nav.GoToAsync("//ShoppingListPage"); // ← それから遷移
                await Shell.Current.GoToAsync("//ShoppingListPage");

                return;                                      // ← 以降の再試行に落ちない
            }
            await Task.Delay(800);
            CurrentState = VoiceInputState.Listening;
            UpdateUiTexts("");
            if (!_allowAutoRestart) return;   // ← 追加
            await StartListeningAsync();
        }

        // 派生が片付けたいときだけここをオーバーライド
        protected virtual Task OnBeforeRestartAsync() => Task.CompletedTask;

        // セッションを明示的に終わらせるAPI
        public async Task EndSessionAsync()
        {
            _allowAutoRestart = false;
            await StopListeningAsync();
        }

        protected void FinishListening(bool shouldContinue)
        {
            IsListening = false;

            // “セッション終了”や “画面非アクティブ”なら即やめ
            if (!shouldContinue) return;

            // 再開の判定と準備は全部ここで
            _ = RetryAsync();   // ← StartListeningAsync を直に叩かず必ずここ経由
        }

        // 🔽 ここを派生で実装するだけ
        protected abstract Task<bool> ProcessRecognizedTextAsync(string result);
    }

}
