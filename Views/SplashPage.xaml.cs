using Android.Runtime;
using ShoppingList002.Platforms.Android;
using ShoppingList002.Services;
using ShoppingList002.Models.Sync;
using ShoppingList002.Models.Dto;
using ShoppingList002.Services.Sync;

namespace ShoppingList002.Views;

public partial class SplashPage : ContentPage
{
    private readonly IInitializationService _init;
    private readonly SyncService _syncService;
    private readonly SyncContext _syncContext;
    private readonly IAuthApiService _authApiService;

    private bool _initialized = false;
    public SplashPage(
         SyncService syncService
        , IAuthApiService authApiService
        , SyncContext syncContext)
	{
		InitializeComponent();
        _syncService = syncService;
        _syncContext = syncContext;
        _authApiService = authApiService;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        LogoLabel.Opacity = 0;
        await LogoLabel.FadeTo(1, 100);
        await Task.Delay(1000);
        await LogoLabel.FadeTo(0, 2000);

        
        // 初期化処理をここで！
        var initializer = ServiceHelper.GetService<IInitializationService>();
        await initializer.InitializeAppAsync();

        if (_initialized) return;
        _initialized = true;
        await _syncService.PullAndReplaceShoppingListAsync();
        // ① サーバから契約状態取得
        //var result = await _authApiService.GetSyncContextAsync();
        var result = new SyncContextDto
        {
            Plan = "Family",
            FamilyId = 1,
            UserId = 1
        };
        //_syncContext.Plan = result.Plan;
        if (Enum.TryParse<SyncPlan>(result.Plan, true, out var plan))
        {
            _syncContext.Plan = plan;
        }
        else
        {
            _syncContext.Plan = SyncPlan.Free; // フォールバック
        }
        _syncContext.FamilyId = result.FamilyId;
        _syncContext.UserId = result.UserId;

        // ② Family のときだけ Pull
        if (_syncContext.IsFamilyMode)
        {
            await _syncService.PullAndReplaceShoppingListAsync();
        }

        await Shell.Current.GoToAsync("//shoppinglist");


        //var appShell = ServiceHelper.GetService<AppShell>();
        //Application.Current.MainPage = appShell;
    }
}