using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marvin.IDP.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Marvin.IDP
{
    public static class IdentityServerBuilderExtensions
    {
        public static IIdentityServerBuilder AddMarvinUserStore(this IIdentityServerBuilder builder)
        {
            builder.Services.AddSingleton<IMarvinUserRepository, MarvinUserRepository>();
            builder.AddProfileService<MarvinUserProfileService>();
            return builder;
        }
    }
}
