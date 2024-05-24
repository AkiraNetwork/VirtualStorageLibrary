using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AkiraNet.VirtualStorageLibrary
{
    public delegate void NotifyNodeDelegate(VirtualPath path, VirtualNode? node, bool isEnd);

    public delegate bool ActionNodeDelegate(VirtualDirectory directory, VirtualNodeName nodeName);

    public delegate bool PatternMatch(string nodeName, string pattern);
}
