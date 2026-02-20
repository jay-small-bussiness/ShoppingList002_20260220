using ShoppingList002.ViewModels;
using ShoppingList002.Models.UiModels;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.Messaging;
using ShoppingList002.Messages;

namespace ShoppingList002.Views;

public partial class VoiceAddPage : ContentPage
{
    private readonly VoiceAddViewModel _viewModel;
    private readonly CandidateCategoryViewModel _candidateCategoryViewModel;
    //private readonly string _returnRoute;
    public VoiceAddPage(
        VoiceAddViewModel viewModel)
//        CandidateCategoryViewModel candidateCategoryViewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        //_returnRoute = returnRoute;

        //_candidateCategoryViewModel = candidateCategoryViewModel;
        BindingContext = _viewModel;

        // 追加履歴スクロール制御
        if (BindingContext is VoiceAddViewModel vm)
        {
            vm.AddedHistory.CollectionChanged += (s, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    if (e.NewStartingIndex == 0)
                        HistoryCollection.ScrollTo(0, position: ScrollToPosition.Start, animate: true);
                    else
                        HistoryCollection.ScrollTo(vm.AddedHistory.Count - 1, position: ScrollToPosition.End, animate: true);
                }
            };
        }
    }
    
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        _viewModel.Activate();
        await _viewModel.StartListeningAsync();

        //try
        //{
        //    // VoiceAdd系の通知を受け取るなら、専用Messageに変更
        //    WeakReferenceMessenger.Default.Register<
        //        VoiceAdd_VM_to_VoiceAddPage_CategoryCreatedMessage>(
        //        this, async (r, m) =>
        //        {
        //            var categoryId = m.Value;
        //            var sp = ((App)App.Current).Services;

        //            await MainThread.InvokeOnMainThreadAsync(async () =>
        //            {
        //                await Shell.Current.Navigation.PushAsync(
        //                    new CandidateCategoryPage(_candidateCategoryViewModel, sp)
        //                );

        //                // ページ遷移後に通知を再送信
        //                WeakReferenceMessenger.Default.Send(
        //                    new VoiceAdd_VM_to_CandidateCategoryPage_CategoryCreatedMessage(categoryId)
        //                );
        //            });
        //        });
        //}
        //catch (Exception ex)
        //{
        //    System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
        //}
    }

    //private async void OnItemTapped(object sender, EventArgs e)
    //{
    //    if (sender is Frame frame && frame.BindingContext is SearchResultItemModel selectedItem)
    //    {
    //        if (BindingContext is VoiceAddViewModel vm)
    //            await vm.OnItemSelectedAsync(selectedItem);
    //    }
    //}

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        // 必要ならセッション終了
        //_viewModel.EndSessionAsync();
        WeakReferenceMessenger.Default.UnregisterAll(this);
    }
}
