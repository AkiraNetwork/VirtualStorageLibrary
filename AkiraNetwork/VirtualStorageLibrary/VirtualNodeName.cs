using AkiraNetwork.VirtualStorageLibrary.Localization;
using System.Reflection;

namespace AkiraNetwork.VirtualStorageLibrary
{
    public class VirtualNodeName(string name) : IEquatable<VirtualNodeName>, IComparable<VirtualNodeName>, IComparable
    {
        private readonly string _name = name;

        // 生成するノード名に付加するカウンター
        private static long _counter = 0;

        public string Name
        {
            get
            {
                return _name;
            }
        }

        public override string ToString() => _name;

        public bool IsRoot
        {
            get
            {
                return _name == VirtualStorageState.State.PathRoot;
            }
        }

        // ノード名を生成する
        public static VirtualNodeName GenerateNodeName(string prefix)
        {
            if (prefix == string.Empty)
            {
                throw new ArgumentException(Resources.PrefixIsEmpty, nameof(prefix));
            }

            long currentNumber = Interlocked.Increment(ref _counter);
            string generatedName = $"{prefix}{currentNumber}";

            return new VirtualNodeName(generatedName);
        }

        // カウンターをリセットする
        public static void ResetCounter()
        {
            Interlocked.Exchange(ref _counter, 0);
        }

        // ユーザーが使用してはいけないノード名の文字のチェック
        public static bool IsValidNodeName(VirtualNodeName nodeName)
        {
            if (nodeName.Name == string.Empty)
            {
                return false;
            }

            foreach (char c in VirtualStorageSettings.Settings.InvalidNodeNameCharacters)
            {
                if (nodeName.Name.Contains(c))
                {
                    return false;
                }
            }

            foreach (string invalidFullNodeName in VirtualStorageSettings.Settings.InvalidFullNodeNames)
            {
                if (nodeName.Name == invalidFullNodeName)
                {
                    return false;
                }
            }

            return true;
        }

        public static implicit operator VirtualNodeName(string name)
        {
            return new VirtualNodeName(name);
        }

        public static implicit operator string(VirtualNodeName nodeName)
        {
            return nodeName._name;
        }

        public bool Equals(VirtualNodeName? other)
        {
            return _name == other?._name;
        }

        public override bool Equals(object? obj)
        {
            if (obj is VirtualNodeName other)
            {
                return _name == other._name;
            }
            return false;
        }

        public override int GetHashCode() => _name.GetHashCode();

        public int CompareTo(VirtualNodeName? other)
        {
            if (other == null)
            {
                return 1;
            }

            return string.Compare(_name, other._name, StringComparison.Ordinal);
        }

        public int CompareTo(object? obj)
        {
            if (obj == null)
            {
                return 1;
            }

            if (obj is not VirtualNodeName)
            {
                throw new ArgumentException(Resources.ParameterIsNotVirtualNodeName, nameof(obj));
            }

            return CompareTo((VirtualNodeName)obj);
        }

        public static bool operator ==(VirtualNodeName? left, VirtualNodeName? right)
        {
            // 両方が null の場合は true
            if (left is null && right is null)
            {
                return true;
            }

            // 一方が null の場合は false
            if (left is null || right is null)
            {
                return false;
            }

            // 実際のノード名の比較
            return left._name == right._name;
        }

        public static bool operator !=(VirtualNodeName? left, VirtualNodeName? right)
        {
            return !(left == right);
        }
    }
}
