using Entropy.Proxy.Pool;
using Newtonsoft.Json;
using RestSharp;

namespace Entropy.Proxy.Pool
{
    public class GimmeProxyPool : IProxyPool
    {
        private readonly RestClient _client = new RestClient();

        public ProxyInfo NextProxy()
        {
            // No SOCKS support atm
            var req = new RestRequest("http://gimmeproxy.com/api/getProxy").AddQueryParameter("protocol", "http");
            var res = _client.Execute<GimmeProxyResponse>(req);
            
            return new ProxyInfo(res.Data.Curl);
        }
    }
    
    public class GimmeProxyWebsites2
    {

        [JsonProperty("example")]
        public bool Example { get; set; }

        [JsonProperty("google")]
        public bool Google { get; set; }

        [JsonProperty("amazon")]
        public bool Amazon { get; set; }
    }

    public partial class GimmeProxyResponse
    {
        public class OtherProtocols2
        {
        }
    }

    public partial class GimmeProxyResponse
    {

        [JsonProperty("get")]
        public bool Get { get; set; }

        [JsonProperty("post")]
        public bool Post { get; set; }

        [JsonProperty("cookies")]
        public bool Cookies { get; set; }

        [JsonProperty("referer")]
        public bool Referer { get; set; }

        [JsonProperty("user-agent")]
        public bool UserAgent { get; set; }

        [JsonProperty("anonymityLevel")]
        public int AnonymityLevel { get; set; }

        [JsonProperty("supportsHttps")]
        public bool SupportsHttps { get; set; }

        [JsonProperty("protocol")]
        public string Protocol { get; set; }

        [JsonProperty("ip")]
        public string Ip { get; set; }

        [JsonProperty("port")]
        public string Port { get; set; }

        [JsonProperty("websites")]
        public GimmeProxyWebsites2 Websites { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("tsChecked")]
        public int TsChecked { get; set; }

        [JsonProperty("curl")]
        public string Curl { get; set; }

        [JsonProperty("ipPort")]
        public string IpPort { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("speed")]
        public double Speed { get; set; }

        [JsonProperty("otherProtocols")]
        public OtherProtocols2 OtherProtocols { get; set; }
    }
}