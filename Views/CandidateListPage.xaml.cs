using ShoppingList002.ViewModels;
using ShoppingList002.Services;
using ShoppingList002.Messages;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Messaging;
namespace ShoppingList002.Views;
[QueryProperty(nameof(CategoryId), "categoryId")]
[QueryProperty(nameof(CategoryTitle), "categoryTitle")]
[QueryProperty(nameof(CategoryTitleWithEmoji), "CategoryTitleWithEmoji")]
[QueryProperty(nameof(ColorId), "colorId")]
[QueryProperty(nameof(FromVoice), "fromVoice")]
public partial class CandidateListPage : ContentPage
{
    public string FromVoice { get; set; }

    public int CategoryId { get; set; }
    public string CategoryTitle { get; set; }
    public string CategoryTitleWithEmoji { get; set; }
    public int ColorId { get; set; }

    private readonly CandidateListPageViewModel _viewModel;
    private readonly INavigationThemeService _navTheme;
    //    public CandidateListPage(CandidateListPageViewModel viewModel, int categoryId, string categoryTitle)
    public CandidateListPage(CandidateListPageViewModel viewModel
                            , INavigationThemeService navTheme)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _navTheme = navTheme;
        BindingContext = _viewModel;
    }
    public void SetCategoryId(int categoryId) => _viewModel.InitializeAsync(categoryId, "", "", 0);

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        if (FromVoice == "true")
        {
            Shell.SetBackButtonBehavior(this, new BackButtonBehavior { IsVisible = false });
        }
        else
        {
            Shell.SetBackButtonBehavior(this, new BackButtonBehavior { IsVisible = true });
        }
    }
    public void SetCategory(int categoryId, string categoryTitle, string categoryTitleWithEmoji, int colorId)
    {
        _viewModel.InitializeAsync(categoryId, categoryTitle, categoryTitleWithEmoji, colorId); // ← 非同期でも非awaitでOK
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is CandidateListPageViewModel vm)
        {
            await vm.InitializeAsync(CategoryId, CategoryTitle, CategoryTitleWithEmoji, ColorId);
        }
    }
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        WeakReferenceMessenger.Default.Unregister<VoiceSearch_VM_to_CandidateCategoryPage_CategoryCreatedMessage>(this);
    }
   
    private async void OnVoiceAddClicked(object sender, EventArgs e)
    {
        // ここで手動 new して VoiceAddPage へ遷移
        var sp = ((App)App.Current).Services;
        var currentRoute = Shell.Current.CurrentState.Location.OriginalString;

        var viewModel = new VoiceAddViewModel(
            CategoryId,
            CategoryTitle,
            _navTheme,
            sp.GetService<ISpeechToTextService>(),
            sp.GetService<ISoundService>(),
            sp.GetService<ICandidateService>(),
            sp.GetService<IShoppingListService>(),
            sp.GetService<IActivityLogService>(),
            sp.GetService<IUserDictService>(),
            sp.GetService<IDatabaseService>(),
            sp.GetService<ISettingsService>()
        );
        

        await Shell.Current.Navigation.PushAsync(new VoiceAddPage(viewModel));


        //await Shell.Current.Navigation.PushAsync(new VoiceAddPage(viewModel));
    }

    private async void OnAddClicked(object sender, EventArgs e)
    {
        var popup = new AddCandidateItemPopup();
        var result = await this.ShowPopupAsync(popup);

        if (result is not null)
        {
            var name = result.GetType().GetProperty("Name")?.GetValue(result)?.ToString();
            var detail = result.GetType().GetProperty("Detail")?.GetValue(result)?.ToString();

            if (!string.IsNullOrWhiteSpace(name))
            {
                var vm = BindingContext as CandidateListPageViewModel;
                if (vm != null)
                    await vm.AddItemFromPopupAsync(name, detail);
            }
            else
            {
                await DisplayAlert("エラー", "名前は必須です！", "OK");
            }
        }
    }
  
}