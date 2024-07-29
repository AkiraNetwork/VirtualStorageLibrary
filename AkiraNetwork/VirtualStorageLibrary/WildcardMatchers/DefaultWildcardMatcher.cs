using System.Collections.ObjectModel;

namespace AkiraNetwork.VirtualStorageLibrary.WildcardMatchers
{
    public class DefaultWildcardMatcher : IVirtualWildcardMatcher
    {
        // 正規表現の記号とそのまま対応するディクショナリ
        private static readonly ReadOnlyDictionary<string, string> _wildcardDictionary = new(
            new Dictionary<string, string>
            {
                { @".", @"." },   // 任意の1文字
                { @"*", @"*" },   // 0文字以上に一致
                { @"+", @"+" },   // 1文字以上に一致
                { @"?", @"?" },   // 0または1文字に一致
                { @"^", @"^" },   // 行の先頭
                { @"$", @"$" },   // 行の末尾
                { @"|", @"|" },   // OR条件
                { @"(", @"(" },   // グループの開始
                { @")", @")" },   // グループの終了
                { @"[", @"[" },   // 文字クラスの開始
                { @"]", @"]" },   // 文字クラスの終了
                { @"{", @"{" },   // 繰り返しの開始
                { @"}", @"}" },   // 繰り返しの終了
                { @"\", @"\" }    // エスケープ文字
            });

        public ReadOnlyDictionary<string, string> WildcardDictionary => _wildcardDictionary;

        public IEnumerable<string> Wildcards => _wildcardDictionary.Keys;

        public IEnumerable<string> Patterns => _wildcardDictionary.Values;

        public int Count => _wildcardDictionary.Count;

        // シンプルな正規表現によるワイルドカードマッチング
        public bool PatternMatcher(string nodeName, string pattern)
        {
            // パターン文字列をそのまま正規表現として使う
            return Regex.IsMatch(nodeName, pattern);
        }
    }
}
