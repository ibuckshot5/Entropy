using System;

namespace Entropy.Proxy.Schema
{
    public class GoManProxySchema : IProxySchema
    {
        public TimeSpan Refresh { get; } = TimeSpan.Zero;
        public int MaxUses { get; } = int.MaxValue;
    }
}