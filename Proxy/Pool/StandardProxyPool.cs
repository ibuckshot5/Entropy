using System.Collections.Generic;
using System.Linq;

namespace Entropy.Proxy.Pool
{
    public class StandardProxyPool : IProxyPool
    {
        public List<ProxyInfo> Proxies;

        private int _currentProxyIteration = 0;

        public StandardProxyPool(IEnumerable<ProxyInfo> proxies)
        {
            Proxies = proxies.ToList();
        }

        public ProxyInfo NextProxy()
        {
            _currentProxyIteration++;
            return Proxies[_currentProxyIteration];
        }
    }
}