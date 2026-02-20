
using Android.Runtime;
using Microsoft.Maui.Controls.PlatformConfiguration;
using ShoppingList002.Platforms.Android;
using System.Diagnostics;

namespace ShoppingList002.Services.AndroidKanaService
{
    public class AndroidUserDictService : IUserDictService
    {
        public async Task InitializeAsync(string csvFileName)
        {
            // JNIチェック
            var cls = JNIEnv.FindClass("com/yourapp/kana/KanaConverter");
            if (cls == IntPtr.Zero)
                throw new Exception("KanaConverter class not found");

            var mid = JNIEnv.GetStaticMethodID(
                cls,
                "getReadingKatakana",
                "(Ljava/lang/String;)Ljava/lang/String;");

            if (mid == IntPtr.Zero)
                throw new Exception("getReadingKatakana method not found");

            // Java側で辞書ロード（SetUserDictionary は Java）
            using var stream = Android.App.Application.Context.Assets.Open(csvFileName);
            using var reader = new StreamReader(stream);
            var csvText = await reader.ReadToEndAsync();

            AndroidKanaBridge.SetUserDictionary(csvText);

            // 簡易自己テスト（ログのみ）
            var test = AndroidKanaBridge.ToKatakana("豚こま");
            Debug.WriteLine($"[UserDict Init Test] {test}");
        }

        public string ToKatakana(string text)
            => AndroidKanaBridge.ToKatakana(text);
        }
    }
