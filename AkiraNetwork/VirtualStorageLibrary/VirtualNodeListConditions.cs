namespace AkiraNetwork.VirtualStorageLibrary
{
    public class VirtualNodeListConditions
    {
        public VirtualNodeTypeFilter Filter { get; set; }

        public VirtualGroupCondition<VirtualNode, object>? GroupCondition { get; set; }

        public List<VirtualSortCondition<VirtualNode>>? SortConditions { get; set; }

        public VirtualNodeListConditions()
        {
            Filter = VirtualNodeTypeFilter.All;
            GroupCondition = null;
            SortConditions = null;
        }

        public VirtualNodeListConditions(VirtualNodeTypeFilter filter, VirtualGroupCondition<VirtualNode, object>? groupCondition, List<VirtualSortCondition<VirtualNode>>? sortConditions)
        {
            Filter = filter;
            GroupCondition = groupCondition;
            SortConditions = sortConditions;
        }
    }
}
