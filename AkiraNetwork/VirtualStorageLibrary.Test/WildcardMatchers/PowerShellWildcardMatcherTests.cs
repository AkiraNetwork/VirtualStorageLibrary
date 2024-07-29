using AkiraNetwork.VirtualStorageLibrary.WildcardMatchers;

namespace AkiraNetwork.VirtualStorageLibrary.Test.WildcardMatchers
{
    [TestClass]
    public class PowerShellWildcardMatcherTests
    {
        [TestMethod]
        public void WildcardDictionary_ContainsExpectedPatterns()
        {
            var matcher = new PowerShellWildcardMatcher();
            var dictionary = matcher.WildcardDictionary;

            Assert.AreEqual(4, dictionary.Count);
            Assert.AreEqual(".*", dictionary["*"]);
            Assert.AreEqual(".", dictionary["?"]);
            Assert.AreEqual("[", dictionary["["]);
            Assert.AreEqual("]", dictionary["]"]);
        }

        [TestMethod]
        public void Wildcards_ContainsExpectedKeys()
        {
            var matcher = new PowerShellWildcardMatcher();
            var wildcards = matcher.Wildcards.ToList();

            Assert.AreEqual(4, wildcards.Count);
            CollectionAssert.Contains(wildcards, "*");
            CollectionAssert.Contains(wildcards, "?");
            CollectionAssert.Contains(wildcards, "[");
            CollectionAssert.Contains(wildcards, "]");
        }

        [TestMethod]
        public void Patterns_ContainsExpectedValues()
        {
            var matcher = new PowerShellWildcardMatcher();
            var patterns = matcher.Patterns.ToList();

            Assert.AreEqual(4, patterns.Count);
            CollectionAssert.Contains(patterns, ".*");
            CollectionAssert.Contains(patterns, ".");
            CollectionAssert.Contains(patterns, "[");
            CollectionAssert.Contains(patterns, "]");
        }

        [TestMethod]
        public void Count_ReturnsCorrectValue()
        {
            var matcher = new PowerShellWildcardMatcher();
            Assert.AreEqual(4, matcher.Count);
        }

        [TestMethod]
        public void PatternMatcher_MatchesCorrectly()
        {
            var matcher = new PowerShellWildcardMatcher();

            // "*" matches zero or more characters
            Assert.IsTrue(matcher.PatternMatcher("test.txt", "*"));
            Assert.IsTrue(matcher.PatternMatcher("test.txt", "t*"));
            Assert.IsTrue(matcher.PatternMatcher("test.txt", "*t"));
            Assert.IsTrue(matcher.PatternMatcher("test.txt", "test*"));
            Assert.IsTrue(matcher.PatternMatcher("test.txt", "*txt"));
            Assert.IsFalse(matcher.PatternMatcher("test.txt", "testX*"));

            // "?" matches exactly one character
            Assert.IsTrue(matcher.PatternMatcher("test.txt", "t?st.txt"));
            Assert.IsTrue(matcher.PatternMatcher("test.txt", "te?t.txt"));
            Assert.IsFalse(matcher.PatternMatcher("test.txt", "test?.txt"));

            // "[" and "]" for character class
            Assert.IsTrue(matcher.PatternMatcher("test.txt", "tes[stu].txt"));
            Assert.IsFalse(matcher.PatternMatcher("test.txt", "tes[pqr].txt"));
        }
    }
}
