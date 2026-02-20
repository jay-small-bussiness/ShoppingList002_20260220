using ShoppingList002.Models.UiModels;
using ShoppingList002.Services.Sync; 
using ShoppingList002.Views;
namespace ShoppingList002
{
    public partial class App : Application
    {
    public IServiceProvider Services { get; }
        // public App(
        //IServiceProvider services,
        //IInitializationService initializer,
        //AppShell appShell,
        //IDatabaseService databaseService)
        // {
        private readonly SyncService _syncService;
        public App(IServiceProvider services,
                    SyncService syncService)
        {
            InitializeComponent();

            Services = services;
            _syncService = syncService;

            //MainPage = new SplashPage(_syncService);
            MainPage = Services.GetRequiredService<SplashPage>();

        }
        protected override void OnStart()
    {
        // ここでは呼ばん（残してもOKやけど使わん）
    }
    }
}
