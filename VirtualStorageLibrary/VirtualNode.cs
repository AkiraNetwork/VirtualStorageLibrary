namespace AkiraNet.VirtualStorageLibrary
{
    public abstract class VirtualNode : IVirtualDeepCloneable<VirtualNode>
    {
        public VirtualNodeName Name { get; set; }
        public DateTime CreatedDate { get; private set; }
        public DateTime UpdatedDate { get; private set; }

        public abstract VirtualNodeType NodeType { get; }

        public abstract VirtualNode DeepClone();

        protected VirtualNode(VirtualNodeName name)
        {
            Name = name;
            CreatedDate = DateTime.Now;
            UpdatedDate = DateTime.Now;
        }

        protected VirtualNode(VirtualNodeName name, DateTime createdDate, DateTime updatedDate)
        {
            Name = name;
            CreatedDate = createdDate;
            UpdatedDate = updatedDate;
        }

        public override string ToString() => $"{Name}";
    }
}
