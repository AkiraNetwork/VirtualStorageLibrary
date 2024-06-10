namespace AkiraNet.VirtualStorageLibrary
{
    public abstract class VirtualNode : IVirtualDeepCloneable<VirtualNode>
    {
        private VirtualNodeName _name;

        public VirtualNodeName Name
        {
            get => _name;
            
            internal set
            {
                _name = value;
                UpdatedDate = DateTime.Now;
            }
        }

        public DateTime CreatedDate { get; private set; }
        public DateTime UpdatedDate { get; private set; }

        public abstract VirtualNodeType NodeType { get; }

        public abstract VirtualNode DeepClone();

        protected VirtualNode(VirtualNodeName name)
        {
            _name = name;
            CreatedDate = DateTime.Now;
            UpdatedDate = DateTime.Now;
        }

        protected VirtualNode(VirtualNodeName name, DateTime createdDate)
        {
            _name = name;
            CreatedDate = createdDate;
            UpdatedDate = createdDate;
        }

        protected VirtualNode(VirtualNodeName name, DateTime createdDate, DateTime updatedDate)
        {
            _name = name;
            CreatedDate = createdDate;
            UpdatedDate = updatedDate;
        }

        public override string ToString() => $"{Name}";
    }
}
