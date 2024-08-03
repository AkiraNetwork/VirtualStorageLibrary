using AkiraNetwork.VirtualStorageLibrary.WildcardMatchers;
using System.Globalization;

namespace AkiraNetwork.VirtualStorageLibrary.Test
{
    [TestClass]
    public class VirtualStorageSettingsTests : VirtualTestBase
    {
        private static readonly char[] InvalidCharsForTest = ['*', '?'];
        
        [TestInitialize]
        public override void TestInitialize()
        {
            base.TestInitialize();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
        }

        [TestMethod]
        public void Initialize_DefaultValues_Correct()
        {
            // Arrange
            VirtualStorageSettings.Initialize();

            // Act
            var settings = VirtualStorageSettings.Settings;

            // Assert
            Assert.AreEqual('/', settings.PathSeparator);
            Assert.AreEqual("/", settings.PathRoot);
            Assert.AreEqual(".", settings.PathDot);
            Assert.AreEqual("..", settings.PathDotDot);
            CollectionAssert.AreEqual(new char[] { settings.PathSeparator }, settings.InvalidNodeNameCharacters);
            CollectionAssert.AreEqual(new string[] { settings.PathDot, settings.PathDotDot }, settings.InvalidNodeNames);
            Assert.IsNotNull(settings.WildcardMatcher);
            Assert.AreEqual("item", settings.PrefixItem);
            Assert.AreEqual("dir", settings.PrefixDirectory);
            Assert.AreEqual("link", settings.PrefixSymbolicLink);
        }

        [TestMethod]
        public void Initialize_ResetsSettings_Correct()
        {
            // Arrange
            VirtualStorageSettings.Settings.PathSeparator = '\\';
            VirtualStorageSettings.Settings.PrefixItem = "customItem";

            // Act
            VirtualStorageSettings.Initialize();
            var settings = VirtualStorageSettings.Settings;

            // Assert
            Assert.AreEqual('/', settings.PathSeparator);
            Assert.AreEqual("item", settings.PrefixItem);
        }

        [TestMethod]
        public void SetPathSeparator_UpdatesValue_Correct()
        {
            // Arrange
            VirtualStorageSettings.Initialize();
            var settings = VirtualStorageSettings.Settings;

            // Act
            settings.PathSeparator = '\\';

            // Assert
            Assert.AreEqual('\\', settings.PathSeparator);
        }

        [TestMethod]
        public void SetInvalidNodeNameCharacters_UpdatesValue_Correct()
        {
            // Arrange
            VirtualStorageSettings.Initialize();
            var settings = VirtualStorageSettings.Settings;

            // Act
            settings.InvalidNodeNameCharacters = InvalidCharsForTest;

            // Assert
            CollectionAssert.AreEqual(InvalidCharsForTest, settings.InvalidNodeNameCharacters);
        }

        [TestMethod]
        public void SetWildcardMatcher_UpdatesValue_Correct()
        {
            // Arrange
            VirtualStorageSettings.Initialize();
            var settings = VirtualStorageSettings.Settings;

            // Act
            var customMatcher = new DefaultWildcardMatcher();
            settings.WildcardMatcher = customMatcher;

            // Assert
            Assert.AreEqual(customMatcher, settings.WildcardMatcher);
        }
    }
}
