using ShoppingList002.Services.Sync;

namespace ShoppingList002
{
    public partial class MainPage : ContentPage
    {
        int count = 0;
        private readonly SyncService _syncService;
        private bool _initialized = false;
        public MainPage(SyncService syncService)
        {
            InitializeComponent();
            _syncService = syncService;
        }

        private void OnCounterClicked(object sender, EventArgs e)
        {
            count++;

            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";

            SemanticScreenReader.Announce(CounterBtn.Text);
        }
    }

}
