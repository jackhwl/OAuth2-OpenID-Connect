using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using IdentityModel.Client;

namespace TripGallery.MVCClient.Helpers
{
    public static class TripGalleryHttpClient
    {

        public static HttpClient GetClient()
        { 
            HttpClient client = new HttpClient(); 
           
            // client credentials flow
            var accessToken = RequestAccessTokenClientCredentials();

            client.SetBearerToken(accessToken);

            client.BaseAddress = new Uri(Constants.TripGalleryAPI);

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            return client;
        }

        private static string RequestAccessTokenClientCredentials()
        {
            // did we store the token before?
            var cookie = HttpContext.Current.Request.Cookies.Get("TripGalleryCookie");
            if (cookie != null && cookie["access_token"] != null)
            {
                return cookie["access_token"];
            }

            // create a Token Client
            var tokenClient = new TokenClient(TripGallery.Constants.TripGallerySTSTokenEndpoint,
                "tripgalleryclientcredentials", TripGallery.Constants.TripGalleryClientSecret);

            // ask for a token, containing the gallerymanagement scope
            var tokenResponse = tokenClient.RequestClientCredentialsAsync("gallerymanagement").Result;

            // decode & write out the token, so we can see what's in it
            TokenHelper.DecodeAndWrite(tokenResponse.AccessToken);

            // we save the token in a cookie for use later on
            HttpContext.Current.Response.Cookies["TripGalleryCookie"]["access_token"] = tokenResponse.AccessToken;
            return tokenResponse.AccessToken;
        }
 
    }
}