using System;

namespace Entropy.Proxy.Schema
{
    public interface IProxySchema
    {
        TimeSpan Refresh { get; }
        int MaxUses { get; }
    }
}