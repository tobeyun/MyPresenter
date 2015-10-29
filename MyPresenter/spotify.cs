using System;
using System.IO;
using System.Net;
using System.Text;

namespace MyPresenter
{
    public class Token
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
    }

    public static class Spotify
    {
        private static string live = "https://accounts.spotify.com/api/token";
        private static string playlist = "https://api.spotify.com/v1/users/spotify/playlists/6Higf6awk4pfVmjvlaCn7b";

        public static string getToken()
        {
            return JSonHelper.ConvertJSonToObject<Token>(getHttpWebResponse(live, "")).access_token;
        }

        public static string getPlaylist()
        {
            return getHttpWebResponse(playlist, getToken());
        }

        private static string getHttpWebResponse(string url, string token)
        {
            string response = String.Empty;

            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
               
                req.ContentType = "application/x-www-form-urlencoded";

                if (token == "")
                {
                    byte[] buf = Encoding.UTF8.GetBytes("grant_type=client_credentials");

                    //Set values for the request back
                    req.Method = "POST";
                    req.Headers.Add("Authorization", "Basic " + Base64Encode("4f2d6157f7454c6687d78b49db17185c:8bbdfa71f07849a38e148971f00a4245"));
                    req.ContentLength = buf.Length;
                    req.GetRequestStream().Write(buf, 0, buf.Length);
                }
                else
                {
                    req.Method = "GET";
                    req.Headers.Add("Authorization", "Bearer " + token);
                }

                using (HttpWebResponse res = (HttpWebResponse)req.GetResponse())
                {
                    // Pipes the stream to a higher level stream reader with the required encoding format. 
                    using (StreamReader readStream = new StreamReader(res.GetResponseStream(), Encoding.UTF8))
                    {
                        response = readStream.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                response = ex.Message;
            }
            finally { }

            return response;
        }

        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);

            return System.Convert.ToBase64String(plainTextBytes);
        }
    }
}
