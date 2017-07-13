using System.Collections.Generic;
using System.IO;
using System.Linq;
using Entropy.Proxy;
using Entropy.Proxy.Schema;

namespace Entropy
{
    public class ProxyReader
    {
        public static List<ProxyInfo> ReadProxies(string proxyFile)
        {
            var lines = File.ReadAllLines($"{Directory.GetCurrentDirectory()}/{proxyFile}");

            return lines.Select(line => new ProxyInfo(line) {Schema = line.Contains("goman.io") ? (IProxySchema) new GoManProxySchema() : new StandardProxySchema()}).ToList();
        }
    }
}