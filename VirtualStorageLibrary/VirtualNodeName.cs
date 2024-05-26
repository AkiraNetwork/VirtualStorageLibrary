namespace AkiraNet.VirtualStorageLibrary
{
    [method: DebuggerStepThrough]
    public class VirtualNodeName(string name) : IEquatable<VirtualNodeName>, IComparable<VirtualNodeName>, IComparable
    {
        private readonly string _name = name;

        public string Name
        {
            [DebuggerStepThrough]
            get
            {
                return _name;
            }
        }

        [DebuggerStepThrough]
        public override string ToString() => _name;

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

        [DebuggerStepThrough]
        public static implicit operator VirtualNodeName(string name)
        {
            return new VirtualNodeName(name);
        }

        [DebuggerStepThrough]
        public static implicit operator string(VirtualNodeName nodeName)
        {
            return nodeName._name;
        }

        [DebuggerStepThrough]
        public bool Equals(VirtualNodeName? other)
        {
            return _name == other?._name;
        }

        [DebuggerStepThrough]
        public override bool Equals(object? obj)
        {
            if (obj is VirtualNodeName other)
            {
                return _name == other._name;
            }
            return false;
        }

        [DebuggerStepThrough]
        public override int GetHashCode() => _name.GetHashCode();

        [DebuggerStepThrough]
        public int CompareTo(VirtualNodeName? other)
        {
            if (other == null)
            {
                return 1;
            }

            return string.Compare(_name, other._name, StringComparison.Ordinal);
        }

        [DebuggerStepThrough]
        public int CompareTo(object? obj)
        {
            if (obj == null)
            {
                return 1;
            }

            if (obj is not VirtualNodeName)
            {
                throw new ArgumentException("Object is not a VirtualNodeName");
            }

            return CompareTo((VirtualNodeName)obj);
        }

        [DebuggerStepThrough]
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

        [DebuggerStepThrough]
        public static bool operator !=(VirtualNodeName? left, VirtualNodeName? right)
        {
            return !(left == right);
        }
    }
}
