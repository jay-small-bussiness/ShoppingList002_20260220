namespace ShoppingList002.Messages
{
    public interface INavigationThemeService
    {
        string CurrentTheme { get; }
        void BeginTheme(string themeId);
        void Push(string route);
        string Pop();
        string Peek();
        void Clear();
    }

}
