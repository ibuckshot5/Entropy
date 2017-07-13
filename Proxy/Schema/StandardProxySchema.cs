using System;

namespace Entropy.Proxy.Schema
{
    public class StandardProxySchema : IProxySchema
    {
        public TimeSpan Refresh { get; } = TimeSpan.FromMinutes(15);
        public int MaxUses { get; } = 5;
    }
}