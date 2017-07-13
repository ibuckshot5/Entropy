using System.Net;

namespace Entropy.Proxy.Pool
{
    public interface IProxyPool
    {
        ProxyInfo NextProxy();
    }
}