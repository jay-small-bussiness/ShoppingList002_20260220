using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShoppingList002.Platforms.Android;

namespace ShoppingList002.Services.AndroidKanaService
{
    public interface IUserDictService
    {
        Task InitializeAsync(string csvFileName);
        string ToKatakana(string text);
    }

}
