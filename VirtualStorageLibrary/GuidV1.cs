using System;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Diagnostics;

namespace AkiraNet.VirtualStorageLibrary
{
    public class GuidV1
    {
        public GuidV1()
        {
            Value = GenerateNewGuid();
        }

        public Guid Value { get; }

        public DateTime Timestamp => GetTimestamp();

        public DateTime TimestampAsLocalTime => Timestamp.ToLocalTime();

        public int Version => GetVersion();

        public int Variant => GetVariant();

        public int ClockSequence => GetClockSequence();

        public byte[] Node => GetNode();

        public string NodeAsString => BitConverter.ToString(Node).Replace("-", ":");

        private static Guid GenerateNewGuid()
        {
            byte[] guidBytes = new byte[16];

            // タイムスタンプの取得
            DateTime epoch = new(1582, 10, 15, 0, 0, 0, DateTimeKind.Utc);
            DateTime now = DateTime.UtcNow;
            TimeSpan ts = now - epoch;
            long timestamp = ts.Ticks * 100; // 100ナノ秒単位に変換

            byte[] timestampBytes = new byte[8];
            timestampBytes[0] = (byte)((timestamp >> 24) & 0xFF);
            timestampBytes[1] = (byte)((timestamp >> 16) & 0xFF);
            timestampBytes[2] = (byte)((timestamp >> 8) & 0xFF);
            timestampBytes[3] = (byte)(timestamp & 0xFF);
            timestampBytes[4] = (byte)((timestamp >> 56) & 0x0F);
            timestampBytes[5] = (byte)((timestamp >> 48) & 0xFF);
            timestampBytes[6] = (byte)((timestamp >> 40) & 0xFF);
            timestampBytes[7] = (byte)((timestamp >> 32) & 0xFF);

            Debug.WriteLine($"Current UTC time: {now}");
            Debug.WriteLine($"Calculated timestamp: {timestamp}");

            // タイムスタンプを GUID バイト配列に設定
            Array.Copy(timestampBytes, 4, guidBytes, 0, 4); // time_low
            Array.Copy(timestampBytes, 2, guidBytes, 4, 2); // time_mid
            Array.Copy(timestampBytes, 0, guidBytes, 6, 2); // time_hi_and_version

            Debug.WriteLine($"GUID Bytes: {BitConverter.ToString(guidBytes)}");

            // バージョン 1 を設定
            guidBytes[6] = (byte)((guidBytes[6] & 0x0F) | 0x10);

            // クロックシーケンスを設定
            byte[] clockSeq = new byte[2];
            RandomNumberGenerator.Fill(clockSeq);
            clockSeq[0] = (byte)((clockSeq[0] & 0x3F) | 0x80); // variant
            Array.Copy(clockSeq, 0, guidBytes, 8, 2);

            // ノード（MACアドレスの取得、失敗した場合はランダムなノード）
            byte[] node = GetMacAddress() ?? GetRandomNode();
            Array.Copy(node, 0, guidBytes, 10, 6);

            Debug.WriteLine($"Final GUID Bytes: {BitConverter.ToString(guidBytes)}");

            return new Guid(guidBytes);
        }

        private DateTime GetTimestamp()
        {
            byte[] guidBytes = Value.ToByteArray();

            long timestamp = (long)(guidBytes[3] & 0xFF) |
                             ((long)(guidBytes[2] & 0xFF) << 8) |
                             ((long)(guidBytes[1] & 0xFF) << 16) |
                             ((long)(guidBytes[0] & 0xFF) << 24) |
                             ((long)(guidBytes[5] & 0xFF) << 32) |
                             ((long)(guidBytes[4] & 0xFF) << 40) |
                             ((long)(guidBytes[7] & 0x0F) << 48) |
                             ((long)(guidBytes[6] & 0x0F) << 52);

            DateTime epoch = new(1582, 10, 15, 0, 0, 0, DateTimeKind.Utc);
            DateTime generatedTime = epoch.AddTicks(timestamp / 100); // 100ナノ秒単位をティックに変換

            return generatedTime;
        }

        private int GetVersion()
        {
            byte[] guidBytes = Value.ToByteArray();
            return (guidBytes[6] >> 4) & 0x0F;
        }

        private int GetVariant()
        {
            byte[] guidBytes = Value.ToByteArray();
            return (guidBytes[8] >> 6) & 0x03;
        }

        private int GetClockSequence()
        {
            byte[] guidBytes = Value.ToByteArray();
            return ((guidBytes[8] & 0x3F) << 8) | (guidBytes[9] & 0xFF);
        }

        private byte[] GetNode()
        {
            byte[] guidBytes = Value.ToByteArray();
            byte[] node = new byte[6];
            Array.Copy(guidBytes, 10, node, 0, 6);
            return node;
        }

        private static byte[]? GetMacAddress()
        {
            try
            {
                foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (nic.OperationalStatus == OperationalStatus.Up)
                    {
                        return nic.GetPhysicalAddress().GetAddressBytes();
                    }
                }
            }
            catch (Exception ex)
            {
                // 必要に応じて例外をログに記録
                Debug.WriteLine($"MACアドレスの取得に失敗しました: {ex.Message}");
            }
            return null;
        }

        private static byte[] GetRandomNode()
        {
            byte[] node = new byte[6];
            RandomNumberGenerator.Fill(node);
            node[0] = (byte)(node[0] | 0x01); // マルチキャストビットを設定
            return node;
        }
    }
}
