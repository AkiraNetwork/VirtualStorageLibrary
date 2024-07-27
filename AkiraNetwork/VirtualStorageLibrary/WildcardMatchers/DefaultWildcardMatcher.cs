namespace AkiraNetwork.VirtualStorageLibrary.WildcardMatchers
{
    public class DefaultWildcardMatcher : IVirtualWildcardMatcher
    {
        // 正規表現の記号とそのまま対応するディクショナリ
        private static readonly Dictionary<string, string> _wildcardDictionary = new()
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
        { @"\", @"\" }  // エスケープ文字
    };

        public Dictionary<string, string> WildcardDictionary => _wildcardDictionary;

        public IEnumerable<string> Wildcards => _wildcardDictionary.Keys;

        public IEnumerable<string> Patterns => _wildcardDictionary.Values;

        // シンプルな正規表現によるワイルドカードマッチング
        public bool PatternMatcher(string nodeName, string pattern)
        {
            // パターン文字列をそのまま正規表現として使う
            return Regex.IsMatch(nodeName, pattern);
        }

        public IVirtualWildcardMatcher DeepClone()
        {
            // まずは浅いクローンを作成
            DefaultWildcardMatcher clone = (DefaultWildcardMatcher)this.MemberwiseClone();

            // 次に他のインスタンスメンバーが追加された場合はここでクローンする

            return clone;
        }
    }
}
