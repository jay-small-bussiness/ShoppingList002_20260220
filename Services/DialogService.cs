namespace ShoppingList002.Services
{
    public class DialogService : IDialogService
    {
        public async Task<bool> ConfirmAsync(string title, string message)
        {
            return await Application.Current.MainPage.DisplayAlert(
                title,
                message,
                "OK",
                "キャンセル");
        }
    }

}
