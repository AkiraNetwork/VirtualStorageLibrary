using System.Diagnostics;

namespace AkiraNet.VirtualStorageLibrary.Test
{
    [TestClass]
    public class GuidV1Tests
    {
        [TestMethod]
        public void Test_NewGuid_GeneratesValidGuid()
        {
            // Arrange
            GuidV1 guidV1 = new();

            // デバッグ出力
            Debug.WriteLine($"Generated GUID: {guidV1.Value}");
            Debug.WriteLine($"Version: {guidV1.Version}");
            Debug.WriteLine($"Variant: {guidV1.Variant}");
            Debug.WriteLine($"Clock Sequence: {guidV1.ClockSequence}");
            Debug.WriteLine($"Node: {guidV1.NodeAsString}");
            Debug.WriteLine($"UTC Timestamp: {guidV1.Timestamp}");
            Debug.WriteLine($"Local Timestamp: {guidV1.TimestampAsLocalTime}");
            Debug.WriteLine(Environment.NewLine);

            // Assert
            Assert.AreNotEqual(Guid.Empty, guidV1.Value, "生成されたGUIDが空であってはなりません。");
            ValidateVersion1Guid(guidV1);
        }

        [TestMethod]
        public void Test_NewGuid_GeneratesUniqueGuids()
        {
            // Act
            HashSet<Guid> guidSet = [];
            List<GuidV1> guidList = [];

            for (int i = 0; i < 10; i++)
            {
                GuidV1 guidV1 = new();

                // デバッグ出力
                Debug.WriteLine($"Generated GUID: {guidV1.Value}");
                Debug.WriteLine($"Version: {guidV1.Version}");
                Debug.WriteLine($"Variant: {guidV1.Variant}");
                Debug.WriteLine($"Clock Sequence: {guidV1.ClockSequence}");
                Debug.WriteLine($"Node: {guidV1.NodeAsString}");
                Debug.WriteLine($"UTC Timestamp: {guidV1.Timestamp}");
                Debug.WriteLine($"Local Timestamp: {guidV1.TimestampAsLocalTime}");
                Debug.WriteLine(Environment.NewLine);

                // Assert
                Assert.IsFalse(guidSet.Contains(guidV1.Value), "生成されたGUIDは一意でなければなりません。");
                ValidateVersion1Guid(guidV1);

                guidSet.Add(guidV1.Value);
                guidList.Add(guidV1);
            }

            // タイムスタンプの順序を検証
            for (int i = 1; i < guidList.Count; i++)
            {
                DateTime prevTimestamp = guidList[i - 1].Timestamp;
                DateTime currentTimestamp = guidList[i].Timestamp;

                Assert.IsTrue(prevTimestamp <= currentTimestamp, $"生成されたGUIDの順番がタイムスタンプに基づいていません。前: {prevTimestamp}, 現在: {currentTimestamp}");
            }
        }

        private static void ValidateVersion1Guid(GuidV1 guidV1)
        {
            // バージョンの確認
            Assert.AreEqual(1, guidV1.Version, "GUIDはバージョン1である必要があります。");

            // バリアントの確認
            Assert.AreEqual(2, guidV1.Variant, "GUIDはRFC 4122バリアントである必要があります。");

            // タイムスタンプの確認
            Assert.IsTrue(guidV1.Timestamp <= DateTime.UtcNow, "GUIDのタイムスタンプが未来の時刻を指しています。");

            // クロックシーケンスの確認
            int clockSeq = guidV1.ClockSequence;
            Assert.IsTrue(clockSeq >= 0 && clockSeq <= 0x3FFF, "クロックシーケンスが有効な範囲内にありません。");

            // ノードの確認
            byte[] node = guidV1.Node;
            Assert.IsTrue(node.Length == 6, "ノードの長さが正しくありません。");
        }
    }
}
