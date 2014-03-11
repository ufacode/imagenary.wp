using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Imagenary.Api
{
    public class ImagenaryApi
    {
        private readonly string _domain;

        public ImagenaryApi(string domain)
        {
            _domain = domain;
        }

        public async Task<UsersAuthResponse> Login(string login, string password)
        {
            var form = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("email", login),
                    new KeyValuePair<string, string>("password", password)
                };

            var content = new FormUrlEncodedContent(form);
            var c = new HttpClient();
            HttpResponseMessage resp = await c.PostAsync(string.Format("http://{0}/users/auth.json", _domain), content);
            resp.EnsureSuccessStatusCode();

            return JsonConvert.DeserializeObject<UsersAuthResponse>(await resp.Content.ReadAsStringAsync());
        }

        public async Task<PhotoResponse> Photos(long limit = 8, long from = -1, string direction = null)
        {
            var query = "";

            query += "limit=" + limit + "&";

            if (from != -1)
            {
                query += "from=" + from + "&";
            }

            if (!string.IsNullOrWhiteSpace(direction))
            {
                query += "direction=" + direction;
            }

            var cl = new HttpClient();
            HttpResponseMessage resp = await cl.GetAsync(string.Format("http://{0}/photos.json?{1}", _domain, query.TrimEnd('&')));

            var content = await resp.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<PhotoResponse>(content);
        }
    }
}