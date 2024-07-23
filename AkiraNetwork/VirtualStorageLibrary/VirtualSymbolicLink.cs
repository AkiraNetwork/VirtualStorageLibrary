using AkiraNetwork.VirtualStorageLibrary.Localization;

namespace AkiraNetwork.VirtualStorageLibrary
{
    public class VirtualSymbolicLink : VirtualNode
    {
        private VirtualPath? _targetPath = null;

        public VirtualPath? TargetPath
        {
            get => _targetPath;
            set => _targetPath = value;
        }

        public override VirtualNodeType NodeType => VirtualNodeType.SymbolicLink;

        public VirtualNodeType TargetNodeType { get; set; } = VirtualNodeType.None;

        public VirtualSymbolicLink()
             : base(VirtualNodeName.GenerateNodeName(VirtualStorageState.State.PrefixSymbolicLink))
        {
        }

        public VirtualSymbolicLink(VirtualNodeName name) : base(name)
        {
        }

        public VirtualSymbolicLink(VirtualNodeName name, VirtualPath? targetPath) : base(name)
        {
            _targetPath = targetPath;
        }

        public VirtualSymbolicLink(VirtualNodeName name, VirtualPath? targetPath, DateTime createdDate, DateTime updatedDate) : base(name, createdDate, updatedDate)
        {
            _targetPath = targetPath;
        }

        // タプルからVirtualSymbolicLinkへの暗黙的な変換
        public static implicit operator VirtualSymbolicLink((VirtualNodeName nodeName, VirtualPath? targetPath) tuple)
        {
            return new VirtualSymbolicLink(new VirtualNodeName(tuple.nodeName), tuple.targetPath);
        }

        // VirtualPathからVirtualSymbolicLinkへの暗黙的な変換
        public static implicit operator VirtualSymbolicLink(VirtualPath? targetPath)
        {
            string prefix = VirtualStorageState.State.PrefixSymbolicLink;
            VirtualNodeName nodeName = VirtualNodeName.GenerateNodeName(prefix);
            return new VirtualSymbolicLink(nodeName, targetPath);
        }

        public override string ToString() => $"{Name} -> {TargetPath ?? "(null)"}";

        public override VirtualNode DeepClone(bool recursive = false)
        {
            return new VirtualSymbolicLink(Name, TargetPath);
        }

        public override void Update(VirtualNode node)
        {
            if (node is not VirtualSymbolicLink newLink)
            {
                throw new ArgumentException(string.Format(Resources.NodeIsNotVirtualSymbolicLink, node.Name), nameof(node));
            }

            if (newLink.IsReferencedInStorage)
            {
                newLink = (VirtualSymbolicLink)newLink.DeepClone();
            }

            CreatedDate = newLink.CreatedDate;
            UpdatedDate = DateTime.Now;
            TargetPath = newLink.TargetPath;
        }
    }
}
