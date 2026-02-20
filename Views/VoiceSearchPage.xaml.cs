//using AndroidX.Lifecycle;
using ShoppingList002.ViewModels;
using ShoppingList002.ViewModels.Base;
using ShoppingList002.Models.UiModels;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.Messaging;
using ShoppingList002.Messages;
//using AndroidX.Lifecycle;
namespace ShoppingList002.Views;

public partial class VoiceSearchPage : ContentPage
{
    private readonly VoiceSearchViewModel _viewModel;
    private readonly INavigationThemeService _naviTheme;
    private readonly CandidateCategoryViewModel _candidateCategoryViewModel;
    private EventHandler<int>? _categoryCreatedHandler;

    public VoiceSearchPage(VoiceSearchViewModel viewModel,
                           CandidateCategoryViewModel candidateCategoryViewModel,
                           INavigationThemeService naviTheme)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _candidateCategoryViewModel = candidateCategoryViewModel;
        _naviTheme = naviTheme;
        BindingContext = _viewModel;
        if (BindingContext is VoiceSearchViewModel vm)
        {
            vm.AddedHistory.CollectionChanged += (s, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    if (e.NewStartingIndex == 0) // Insert(0, …) の場合
                    {
                        HistoryCollection.ScrollTo(0, position: ScrollToPosition.Start, animate: true);
                    }
                    else
                    {
                        HistoryCollection.ScrollTo(vm.AddedHistory.Count - 1, position: ScrollToPosition.End, animate: true);
                    }
                }
            };
        }
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.Activate();
        await _viewModel.StartListeningAsync();
        try
        {
            WeakReferenceMessenger.Default.Register<VoiceSearch_VM_to_VoiceSearchPage_CategoryCreatedMessage>(
                this, async (r, m) =>
                {
                    var categoryId = m.Value;
                    var sp = ((App)App.Current).Services;

                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        // ページ遷移
                        await Shell.Current.Navigation.PushAsync(
                            new CandidateCategoryPage(_candidateCategoryViewModel, sp, _naviTheme)
                        );

                        // 遷移後、CandidateCategoryPage に知らせたいならここで再送信
                        WeakReferenceMessenger.Default.Send(
                            new VoiceSearch_VM_to_CandidateCategoryPage_CategoryCreatedMessage(categoryId)
                        );
                    });
                });

        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
        }

        //_categoryCreatedHandler = async (s, id) =>
        //{
        //    try
        //    {
        //        MainThread.BeginInvokeOnMainThread(async () =>
        //        {
        //            var sp = ((App)App.Current).Services;
        //            await Shell.Current.Navigation.PushModalAsync(new CandidateCategoryPage(_candidateCategoryViewModel, sp));
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
        //    }
        //};

        //_viewModel.CategoryCreated += _categoryCreatedHandler;

    }
    private async void OnItemTapped(object sender, EventArgs e)
    {
        if (sender is Frame frame && frame.BindingContext is SearchResultItemModel selectedItem)
        {
            if (BindingContext is VoiceSearchViewModel vm)
            {
                await vm.OnItemSelectedAsync(selectedItem);
            }
        }
    }
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (BindingContext is VoiceSearchViewModel vm)
            (BindingContext as BaseVoiceAddViewModel)?.EndSessionAsync();
//        vm.StopListeningCommand?.Execute(null);
        if (_categoryCreatedHandler != null)
            _viewModel.CategoryCreated -= _categoryCreatedHandler;
    }

 
   
}