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

        [TestMethod]
        public void IsValidWildcardPattern_ReturnsTrueForValidPatterns()
        {
            var matcher = new PowerShellWildcardMatcher();

            // Valid patterns
            Assert.IsTrue(matcher.IsValidWildcardPattern("*"));       // Matches zero or more characters
            Assert.IsTrue(matcher.IsValidWildcardPattern("?"));       // Matches any single character
            Assert.IsTrue(matcher.IsValidWildcardPattern("[a-z]"));   // Character class
            Assert.IsTrue(matcher.IsValidWildcardPattern("test*"));   // Matches "test" followed by any characters
            Assert.IsTrue(matcher.IsValidWildcardPattern("file[1-5].txt")); // Matches "file1.txt" to "file5.txt"
            Assert.IsTrue(matcher.IsValidWildcardPattern("a`*b"));    // Escaped asterisk as literal '*'
        }

        [TestMethod]
        public void IsValidWildcardPattern_ReturnsFalseForInvalidPatterns()
        {
            var matcher = new PowerShellWildcardMatcher();

            // Invalid patterns
            Assert.IsFalse(matcher.IsValidWildcardPattern("[a-z"));   // Missing closing bracket in character class
            Assert.IsFalse(matcher.IsValidWildcardPattern("test[`]")); // Improper escape within character class
            Assert.IsFalse(matcher.IsValidWildcardPattern("file[`")); // Unclosed escape character
        }
    }
}
