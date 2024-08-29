// This file is part of VirtualStorageLibrary.
//
// Copyright (C) 2024 Akira Shimodate
//
// VirtualStorageLibrary is free software, and it is distributed under the terms of 
// the GNU General Public License (version 3, or at your option, any later version). 
// This license is published by the Free Software Foundation.
//
// VirtualStorageLibrary is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY, without even the implied warranty of MERCHANTABILITY or 
// FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along 
// with VirtualStorageLibrary. If not, see https://www.gnu.org/licenses/.

using AkiraNetwork.VirtualStorageLibrary.WildcardMatchers;

namespace AkiraNetwork.VirtualStorageLibrary.Test.WildcardMatchers
{
    [TestClass]
    public class DefaultWildcardMatcherTests
    {
        [TestMethod]
        public void WildcardDictionary_ContainsExpectedPatterns()
        {
            var matcher = new DefaultWildcardMatcher();
            var dictionary = matcher.WildcardDictionary;

            Assert.AreEqual(14, dictionary.Count);
            Assert.AreEqual(@".", dictionary[@"."]);
            Assert.AreEqual(@"*", dictionary[@"*"]);
            Assert.AreEqual(@"+", dictionary[@"+"]);
            Assert.AreEqual(@"?", dictionary[@"?"]);
            Assert.AreEqual(@"^", dictionary[@"^"]);
            Assert.AreEqual(@"$", dictionary[@"$"]);
            Assert.AreEqual(@"|", dictionary[@"|"]);
            Assert.AreEqual(@"(", dictionary[@"("]);
            Assert.AreEqual(@")", dictionary[@")"]);
            Assert.AreEqual(@"[", dictionary[@"["]);
            Assert.AreEqual(@"]", dictionary[@"]"]);
            Assert.AreEqual(@"{", dictionary[@"{"]);
            Assert.AreEqual(@"}", dictionary[@"}"]);
            Assert.AreEqual(@"\", dictionary[@"\"]);
        }

        [TestMethod]
        public void Wildcards_ContainsExpectedKeys()
        {
            var matcher = new DefaultWildcardMatcher();
            var wildcards = matcher.Wildcards.ToList();

            Assert.AreEqual(14, wildcards.Count);
            CollectionAssert.Contains(wildcards, @".");
            CollectionAssert.Contains(wildcards, @"*");
            CollectionAssert.Contains(wildcards, @"+");
            CollectionAssert.Contains(wildcards, @"?");
            CollectionAssert.Contains(wildcards, @"^");
            CollectionAssert.Contains(wildcards, @"$");
            CollectionAssert.Contains(wildcards, @"|");
            CollectionAssert.Contains(wildcards, @"(");
            CollectionAssert.Contains(wildcards, @")");
            CollectionAssert.Contains(wildcards, @"[");
            CollectionAssert.Contains(wildcards, @"]");
            CollectionAssert.Contains(wildcards, @"{");
            CollectionAssert.Contains(wildcards, @"}");
            CollectionAssert.Contains(wildcards, @"\");
        }

        [TestMethod]
        public void Patterns_ContainsExpectedValues()
        {
            var matcher = new DefaultWildcardMatcher();
            var patterns = matcher.Patterns.ToList();

            Assert.AreEqual(14, patterns.Count);
            CollectionAssert.Contains(patterns, @".");
            CollectionAssert.Contains(patterns, @"*");
            CollectionAssert.Contains(patterns, @"+");
            CollectionAssert.Contains(patterns, @"?");
            CollectionAssert.Contains(patterns, @"^");
            CollectionAssert.Contains(patterns, @"$");
            CollectionAssert.Contains(patterns, @"|");
            CollectionAssert.Contains(patterns, @"(");
            CollectionAssert.Contains(patterns, @")");
            CollectionAssert.Contains(patterns, @"[");
            CollectionAssert.Contains(patterns, @"]");
            CollectionAssert.Contains(patterns, @"{");
            CollectionAssert.Contains(patterns, @"}");
            CollectionAssert.Contains(patterns, @"\");
        }

        [TestMethod]
        public void Count_ReturnsCorrectValue()
        {
            var matcher = new DefaultWildcardMatcher();
            Assert.AreEqual(14, matcher.Count);
        }

        [TestMethod]
        public void PatternMatcher_MatchesCorrectly()
        {
            var matcher = new DefaultWildcardMatcher();

            // "." matches any one character
            Assert.IsTrue(matcher.PatternMatcher("test.txt", "test.t?t"));

            // "*" matches zero or more characters
            Assert.IsTrue(matcher.PatternMatcher("test.txt", "test.*"));
            Assert.IsTrue(matcher.PatternMatcher("test.txt", "t.*"));
            Assert.IsTrue(matcher.PatternMatcher("test.txt", ".*t"));
            Assert.IsTrue(matcher.PatternMatcher("test.txt", "testX*"));

            // "+" matches one or more characters
            Assert.IsTrue(matcher.PatternMatcher("test.txt", "test.+"));
            Assert.IsFalse(matcher.PatternMatcher("test.txt", "testX+"));

            // "?" matches zero or one character
            Assert.IsTrue(matcher.PatternMatcher("test.txt", "t.?st.txt"));
            Assert.IsTrue(matcher.PatternMatcher("test.txt", "te.?t.txt"));
            Assert.IsTrue(matcher.PatternMatcher("test.txt", "test?.txt"));
            Assert.IsTrue(matcher.PatternMatcher("test.txt", "testX?.txt"));

            // "[" and "]" for character class
            Assert.IsTrue(matcher.PatternMatcher("test.txt", "tes[stu].txt"));
            Assert.IsFalse(matcher.PatternMatcher("test.txt", "tes[pqr].txt"));

            // "^" matches the beginning of the string
            Assert.IsTrue(matcher.PatternMatcher("test.txt", "^test.txt"));

            // "$" matches the end of the string
            Assert.IsTrue(matcher.PatternMatcher("test.txt", "test.txt$"));

            // "|" matches either the expression before or the expression after
            Assert.IsTrue(matcher.PatternMatcher("test.txt", "test.txt|sample.txt"));
            Assert.IsFalse(matcher.PatternMatcher("sample.txt", "test.txt|sample1.txt"));
        }

        [TestMethod]
        public void IsValidWildcardPattern_ReturnsTrueForValidPatterns()
        {
            var matcher = new DefaultWildcardMatcher();

            // Valid patterns
            Assert.IsTrue(matcher.IsValidWildcardPattern(".*"));     // Matches any number of any characters
            Assert.IsTrue(matcher.IsValidWildcardPattern("a.*"));    // Matches 'a' followed by any characters
            Assert.IsTrue(matcher.IsValidWildcardPattern("^a.*$"));  // Matches 'a' at the start and any characters followed by end of line
            Assert.IsTrue(matcher.IsValidWildcardPattern("[a-z]"));  // Matches any lowercase letter
            Assert.IsTrue(matcher.IsValidWildcardPattern("test|sample")); // Matches "test" or "sample"
        }

        [TestMethod]
        public void IsValidWildcardPattern_ReturnsFalseForInvalidPatterns()
        {
            var matcher = new DefaultWildcardMatcher();

            // Invalid patterns
            Assert.IsFalse(matcher.IsValidWildcardPattern("[a-z"));     // Missing closing bracket
            Assert.IsFalse(matcher.IsValidWildcardPattern("(*)"));      // UnEscaped special characters
            Assert.IsFalse(matcher.IsValidWildcardPattern("*test"));    // Asterisk at the start without preceding characters
            Assert.IsFalse(matcher.IsValidWildcardPattern("(test"));  // Missing closing parenthesis
        }
    }
}
