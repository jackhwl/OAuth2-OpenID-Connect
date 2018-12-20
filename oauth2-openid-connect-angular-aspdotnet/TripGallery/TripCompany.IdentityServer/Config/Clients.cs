using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IdentityServer3.Core.Models;
//using TripGallery;

namespace TripCompany.IdentityServer.Config
{
    public static class Clients
    {
        public static IEnumerable<Client> Get()
        { 
            return new List<Client>
            {
                new Client
                {
                    ClientId = "tripgalleryclientcredentials",
                    ClientName = "Trip Gallery (Client Credentials)",
                    Flow = Flows.ClientCredentials,
                    ClientSecrets = new List<Secret>
                    {
                        new Secret(TripGallery.Constants.TripGalleryClientSecret.Sha256())
                    },
                    AllowAccessToAllScopes = true,
                }
            }; 
        }
    }
}
