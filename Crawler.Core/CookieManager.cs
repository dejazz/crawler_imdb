using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMDB_Crawler.Crawler.Core
{
    using System;
    using System.Collections.Generic;
    using System.Net;

    public class CookieManager
    {
        private readonly CookieContainer _cookieContainer;

        public CookieManager()
        {
            _cookieContainer = new CookieContainer();
        }

        public void AddCookie(string url, string name, string value, string domain, string path = "/")
        {
            Uri uri = new Uri(url);
            string cookieDomain = domain ?? uri.Host;

            var cookie = new Cookie(name, value, path, cookieDomain);
            _cookieContainer.Add(cookie);
        }

        public List<Cookie> GetCookies(string url)
        {
            Uri uri = new Uri(url);
            List<Cookie> cookies = new List<Cookie>();

            foreach (Cookie cookie in _cookieContainer.GetCookies(uri))
            {
                cookies.Add(cookie);
            }

            return cookies;
        }

        public CookieContainer GetCookieContainer() => _cookieContainer;
    }
}
