using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using ShoppingList002.Services;

namespace ShoppingList002.Services
{
    public class InitializationService : IInitializationService
    {
        private readonly IDatabaseService _dbService;
        private readonly ICandidateDataService _candidateDataService;
        private readonly IUserDictService _userDictService;
        private readonly AppShell _appShell;
        public bool IsInitialized { get; private set; }

        public InitializationService(
            IDatabaseService dbService,
            ICandidateDataService candidateDataService,
            IUserDictService userDictService,
            AppShell appShell)
        {
            _dbService = dbService;
            _candidateDataService = candidateDataService;
            _userDictService = userDictService;
            _appShell = appShell;
        }
        public async Task InitializeAppAsync()
        {
            Console.WriteLine("🔧 DB初期化 開始");
            await _dbService.InitializeDatabaseAsync();
            Console.WriteLine("✅ DB初期化 完了");

            await _candidateDataService.EnsureInitializedAsync();
            await _userDictService.InitializeAsync("userdict.csv");

            await _appShell.InitializeFlyoutItems();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Application.Current.MainPage = _appShell;
            });
            IsInitialized = true;
        }

        //public async Task InitializeAppAsync()
        //{
        //    Console.WriteLine("🔧 DB初期化 開始");
        //    await _dbService.InitializeDatabaseAsync();
        //    Console.WriteLine("✅ DB初期化 完了");

        //    Console.WriteLine("📦 Candidate初期化 開始");
        //    await _candidateDataService.EnsureInitializedAsync();
        //    Console.WriteLine("✅ Candidate初期化 完了");
        //    await _appShell.InitializeFlyoutItems();

        //    MainThread.BeginInvokeOnMainThread(() =>
        //    {
        //        Application.Current.MainPage = _appShell;
        //    });
        //}
    }

}
