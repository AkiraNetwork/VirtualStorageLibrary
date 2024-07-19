using System.Globalization;
using System.Resources;

namespace AkiraNetwork.VirtualStorageLibrary
{
    public class VirtualResourceManager : IReadOnlyDictionary<string, string>
    {
        // シングルトンインスタンス
        private static readonly VirtualResourceManager instance = new();

        private readonly ResourceManager resourceManager;
        private readonly Dictionary<string, string> _messages;

        // プライベートコンストラクタで外部からのインスタンス化を防ぐ
        private VirtualResourceManager()
        {
            resourceManager = new ResourceManager("AkiraNetwork.VirtualStorageLibrary.Resources.Resources", typeof(VirtualResourceManager).Assembly);
            _messages = [];
        }

        // 公開されたインスタンスプロパティ
        public static VirtualResourceManager Resources => instance;

        // カルチャに基づいてリソースを初期化
        public void Initialize(CultureInfo? culture = null)
        {
            _messages.Clear();
            CultureInfo effectiveCulture = culture ?? CultureInfo.InvariantCulture;
            ResourceSet? resourceSet = resourceManager.GetResourceSet(effectiveCulture, true, true);

            if (resourceSet == null)
            {
                string cultureName = culture?.Name ?? "InvariantCulture";
                throw new InvalidOperationException($"Resources for culture '{cultureName}' could not be found.");
            }

            foreach (DictionaryEntry entry in resourceSet)
            {
                if (entry.Key is string key && entry.Value is string value)
                {
                    _messages[key] = value;
                }
            }
        }

        // メッセージを取得
        public string GetString(string key, params object?[]? args)
        {
            if (_messages.TryGetValue(key, out var format))
            {
                return args == null || args.Length == 0 ? format : string.Format(format, args);
            }
            return $"[{key}]";
        }

        // インデクサーでメッセージを取得
        public string this[string key, params object?[]? args]
        {
            get
            {
                return GetString(key, args);
            }
        }

        // カルチャ情報を取得
        public static CultureInfo GetCultureInfo(string locale)
        {
            // string.Empty をニュートラルカルチャとして扱う
            if (locale == string.Empty)
            {
                return CultureInfo.InvariantCulture;
            }

            // その他のロケールは通常の CultureInfo を返す
            return new CultureInfo(locale);
        }

        #region IReadOnlyDictionary<string, string> インターフェースの実装

        public IEnumerable<string> Keys => _messages.Keys;

        public IEnumerable<string> Values => _messages.Values;

        public int Count => _messages.Count;

        public bool ContainsKey(string key)
        {
            return _messages.ContainsKey(key);
        }

        public bool TryGetValue(string key, out string value)
        {
            if (_messages.TryGetValue(key, out var tempValue))
            {
                value = tempValue;
                return true;
            }
            else
            {
                value = string.Empty;
                return false;
            }
        }

        public string this[string key] => GetString(key);

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return _messages.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => _messages.GetEnumerator();

        #endregion
    }
}
