namespace AkiraNet.VirtualStorageLibrary
{
    [DebuggerStepThrough]
    public abstract class VirtualNode : IVirtualDeepCloneable<VirtualNode>
    {
        public VirtualNodeName Name { get; internal set; }

        public DateTime CreatedDate { get; internal set; }

        public DateTime UpdatedDate { get; internal set; }

        public abstract VirtualNodeType NodeType { get; }

        public abstract VirtualNode DeepClone(bool recursive = false);

        public abstract void Update(VirtualNode node);

        public bool IsReferencedInStorage { get; internal set; } = false;

        protected VirtualNode(VirtualNodeName name)
        {
            Name = name;
            CreatedDate = UpdatedDate = DateTime.Now;
        }

        protected VirtualNode(VirtualNodeName name, DateTime createdDate)
        {
            Name = name;
            CreatedDate = UpdatedDate = createdDate;
        }

        protected VirtualNode(VirtualNodeName name, DateTime createdDate, DateTime updatedDate)
        {
            Name = name;
            CreatedDate = UpdatedDate = createdDate;
        }

        public override string ToString() => $"{Name}";
    }
}
