namespace ShoppingList002.Messages
{
    public class NavigationThemeService : INavigationThemeService
    {
        private readonly Dictionary<string, Stack<string>> _themes = new();
        private string _currentTheme;

        public string CurrentTheme => _currentTheme;

        public void BeginTheme(string themeId)
        {
            // 前のテーマを破棄
            if (!string.IsNullOrEmpty(_currentTheme))
                _themes.Remove(_currentTheme);
            _currentTheme = themeId;

            // ←ここで必ず新しいStackを用意しておく
            _themes[themeId] = new Stack<string>();

            //if (_currentTheme != themeId)
            //{
            //    _currentTheme = themeId;
            //    if (!_themes.ContainsKey(themeId))
            //        _themes[themeId] = new Stack<string>();
            //}
        }

        public void Push(string route)
        {
            if (_currentTheme != null)
                _themes[_currentTheme].Push(route);
        }

        public string Pop()
        {
            return _currentTheme != null && _themes[_currentTheme].Count > 0
                ? _themes[_currentTheme].Pop()
                : null;
        }

        public string Peek()
        {
            return _currentTheme != null && _themes[_currentTheme].Count > 0
                ? _themes[_currentTheme].Peek()
                : null;
        }

        public void Clear() => _themes.Clear();
    }

}
