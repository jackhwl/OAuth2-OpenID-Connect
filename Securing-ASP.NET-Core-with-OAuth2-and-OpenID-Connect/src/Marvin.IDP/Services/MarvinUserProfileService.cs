using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;

namespace Marvin.IDP.Services
{
    public class MarvinUserProfileService : IProfileService
    {
        private readonly IMarvinUserRepository _marvinUserRepository;
        public MarvinUserProfileService(IMarvinUserRepository marvinUserRepository)
        {
            _marvinUserRepository = marvinUserRepository;
        }
        public Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var subjectId = context.Subject.GetSubjectId();
            var claimsForUser = _marvinUserRepository.GetUserClaimsBySubjectId(subjectId);

            context.IssuedClaims = claimsForUser.Select(c => new Claim(c.ClaimType, c.ClaimValue)).ToList();

            return Task.CompletedTask;
        }

        public Task IsActiveAsync(IsActiveContext context)
        {
            var subjectId = context.Subject.GetSubjectId();
            context.IsActive = _marvinUserRepository.IsUserActive(subjectId);

            return Task.CompletedTask;
        }
    }
}
