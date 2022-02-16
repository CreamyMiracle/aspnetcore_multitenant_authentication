using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MultitenantAuthentication.Config;
using System.Diagnostics;
using System.Security.Claims;

namespace MultitenantAuthentication.Security
{
    public class JwtBearerOptionsInitializer : IConfigureNamedOptions<JwtBearerOptions>
    {
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly AuthenticationSettings _authenticationSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public JwtBearerOptionsInitializer(IDataProtectionProvider dataProtectionProvider, IHttpContextAccessor httpContextAccessor, AuthenticationSettings authenticationSettings)
        {
            _dataProtectionProvider = dataProtectionProvider;
            _authenticationSettings = authenticationSettings;
            _httpContextAccessor = httpContextAccessor;
        }

        public void Configure(string name, JwtBearerOptions options)
        {
            if (!string.Equals(name, JwtBearerDefaults.AuthenticationScheme, StringComparison.Ordinal))
            {
                return;
            }

            ClaimsPrincipal? user = _httpContextAccessor.HttpContext?.User;
            string? issuer = user?.Claims.FirstOrDefault()?.OriginalIssuer;
            string? currentTenant = issuer?.Replace(_authenticationSettings.AuthorityPrefix, "");

            if (currentTenant == null)
            {
                return;
            }

            options.Authority = _authenticationSettings.AuthorityPrefix + currentTenant;
            options.MetadataAddress = string.Format(_authenticationSettings.Metadata, currentTenant);
            options.RequireHttpsMetadata = false; // Needs to be false to provide MetadataAddress with HTTP scheme
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false, // Set this true to enable validation
                ValidAudience = _authenticationSettings.Audience // POI: Audience to check whether given tokes is meant to be used for this service
            };
        }

        public void Configure(JwtBearerOptions options) => Debug.Fail("This infrastructure method shouldn't be called.");
    }
}
