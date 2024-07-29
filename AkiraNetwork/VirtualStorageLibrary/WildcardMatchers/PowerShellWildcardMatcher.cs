using System.Collections.ObjectModel;

namespace AkiraNetwork.VirtualStorageLibrary.WildcardMatchers
{
    public class PowerShellWildcardMatcher : IVirtualWildcardMatcher
    {
        // ワイルドカードとそれに対応する正規表現のパターンの配列
        private static readonly ReadOnlyDictionary<string, string> _wildcardDictionary = new(
            new Dictionary<string, string>
            {
                { "*", ".*" },  // 0文字以上に一致
                { "?", "." },   // 任意の1文字に一致
                { "[", "[" },   // 文字クラスの開始
                { "]", "]" }    // 文字クラスの終了
            });

        public ReadOnlyDictionary<string, string> WildcardDictionary => _wildcardDictionary;

        public IEnumerable<string> Wildcards => _wildcardDictionary.Keys;

        public IEnumerable<string> Patterns => _wildcardDictionary.Values;

        public int Count => _wildcardDictionary.Count;

        // ワイルドカードの実装（PowerShell）
        public bool PatternMatcher(string nodeName, string pattern)
        {
            // エスケープ処理を考慮して正規表現のパターンを作成
            string regexPattern = "^";
            for (int i = 0; i < pattern.Length; i++)
            {
                if (pattern[i] == '`' && i + 1 < pattern.Length && Wildcards.Contains(pattern[i + 1].ToString()))
                {
                    // エスケープされたワイルドカード文字をリテラルとして扱う
                    regexPattern += Regex.Escape(pattern[i + 1].ToString());
                    i++; // エスケープされた文字をスキップ
                }
                else
                {
                    string currentChar = pattern[i].ToString();
                    if (WildcardDictionary.TryGetValue(currentChar, out string? wildcard))
                    {
                        regexPattern += wildcard;
                    }
                    else
                    {
                        regexPattern += Regex.Escape(currentChar);
                    }
                }
            }
            regexPattern += "$";

            // 正規表現を用いてマッチングを行う
            return Regex.IsMatch(nodeName, regexPattern);
        }
    }
}
