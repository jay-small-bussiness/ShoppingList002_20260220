namespace ShoppingList002.Services
{
    public interface IDialogService
    {
        Task<bool> ConfirmAsync(string title, string message);
    }

}
