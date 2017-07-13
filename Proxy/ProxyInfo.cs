using System;
using System.Net;
using Entropy.Proxy.Pool;
using Entropy.Proxy.Schema;

namespace Entropy.Proxy
{
    public class ProxyInfo
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public ProxyProtocol Protocol { get; set; }
        
        public IProxySchema Schema { get; set; }
        
        public string Address { get; set; }

        public ProxyInfo(string address)
        {
            Address = address;
            var px = address.Replace("/", string.Empty).Split(':');
            ProxyProtocol proxyProtocol;
            Enum.TryParse(px[0], true, out proxyProtocol);
            Protocol = proxyProtocol;
        }

        public WebProxy ToWebProxy()
        {
            return new WebProxy(Address, true);
        }
    }
}