using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShoppingList002.Models.UiModels;
using ShoppingList002.Services;
using ShoppingList002.Views;
using ShoppingList002.Messages;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Android.Media;

namespace ShoppingList002.ViewModels
{
    public partial class ShoppingListPageViewModel : ObservableObject
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IDialogService _dialogService;
        private readonly IShoppingListService _shoppingListService;
        private INavigationThemeService _navTheme;
        //public IRelayCommand<int> MarkAsPurchasedCommand { get; }
        private ICommand _markAsPurchasedCommand;
        public ICommand MarkAsPurchasedCommand => _markAsPurchasedCommand;


        public IRelayCommand UndoLastPurchasedCommand { get; }
        public IRelayCommand GoToCategoryPageCommand { get; }
        public ICommand OpenVoiceSearchCommand { get; }

        [ObservableProperty]
        private ObservableCollection<ShoppingListUiModel> items = new();

        public ShoppingListPageViewModel(
            IShoppingListService shoppingListService,
            IDialogService dialogService,
            INavigationThemeService navigationThemeService)
        {
            UndoLastPurchasedCommand = new RelayCommand(async () =>
            {
                var restoredName = await _shoppingListService.UndoLastPurchasedItemAsync();
                await RefreshAsync(); // 表示更新
                if (!string.IsNullOrEmpty(restoredName))
                {
                    Toast.Make($"「{restoredName}」を元に戻しました").Show();
                }
            });
            OpenVoiceSearchCommand = new Command(async () => await OpenVoiceSearchPageAsync());

            GoToCategoryPageCommand = new RelayCommand(async () =>
            {
                await Shell.Current.GoToAsync("CandidateCategoryPage");
            });
            _shoppingListService = shoppingListService;
            _navTheme = navigationThemeService;
            _dialogService = dialogService;
            _markAsPurchasedCommand = new Command<ShoppingListUiModel>(async (item) =>
            {
                if (item == null)
                    return;

                if (!item.ItemId.HasValue)
                {
                    // 👇 ここが「メモ用の出口」
                    await DeleteMemoAsync(item);
                    return;
                }

                await MarkAsPurchasedAsync(item.ItemId.Value);
            });

            //MarkAsPurchasedCommand = new RelayCommand<int>((itemId) =>
            //{
            //    System.Diagnostics.Debug.WriteLine($"✅ COMMAND TRIGGERED! itemId={itemId}");
            //    _ = MarkAsPurchasedAsync(itemId);
            //});
            _ = LoadAsync();
        }
        private async Task OpenVoiceSearchPageAsync()
        {
            //var page = _serviceProvider.GetRequiredService<VoiceSearchPage>();
            //await Shell.Current.Navigation.PushAsync(page);
            try
            {
                //var page = _serviceProvider.GetService<VoiceSearchPage>();
                var page = ((App)App.Current).Services.GetService<VoiceSearchPage>();

                if (page == null)
                {
                    Console.WriteLine("❌ VoiceSearchPage がDIに登録されてへん！");
                    await Shell.Current.DisplayAlert("エラー", "VoiceSearchPageが生成できん", "OK");
                    return;
                }
                var route = "//ShoppingListPage";
                _navTheme.BeginTheme("voiceadd");   //ボイス追加系の遷移記憶
                _navTheme.Push(route);              //対象カテゴリー詳細へのルートを記憶しておく
                
                await Shell.Current.Navigation.PushAsync(page);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("できません", $"{ex.GetType()}\n{ex.Message}", "OK");
                //Debug.Print("できません", $"{ex.GetType()}\n{ex.Message}", "OK");
                //Debug.Print(ex.Message);
            }
        }
        private async Task DeleteMemoAsync(ShoppingListUiModel item)
        {
            var ok = await _dialogService.ConfirmAsync(
                "確認",
                "このメモを削除しますか？");

            if (!ok)
                return;
            await _shoppingListService.DeleteMemoAsync(item);
            await RefreshAsync(); // 更新
        }
        private async Task MarkAsPurchasedAsync(int itemId)
        {
            System.Diagnostics.Debug.WriteLine($"[✓] MarkAsPurchasedAsync → itemId={itemId}");
            await _shoppingListService.MarkAsPurchasedAsync(itemId);
            await RefreshAsync(); // 更新
        }
        private async Task LoadAsync()
        {
            var list = await _shoppingListService.GetDisplayItemsAsync();
            Items = new ObservableCollection<ShoppingListUiModel>(list);
        }
        public async Task RefreshAsync()
        {
            var list = await _shoppingListService.GetDisplayItemsAsync();
            Items = new ObservableCollection<ShoppingListUiModel>(list);
        }
     

    }
}
