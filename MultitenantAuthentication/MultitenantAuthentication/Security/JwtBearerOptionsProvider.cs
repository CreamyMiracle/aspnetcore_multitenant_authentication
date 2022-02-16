using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace MultitenantAuthentication.Security
{
    public class JwtBearerOptionsProvider : IOptionsMonitor<JwtBearerOptions>
    {
        private readonly ConcurrentDictionary<(string name, string tenant), Lazy<JwtBearerOptions>> _cache;
        private readonly IOptionsFactory<JwtBearerOptions> _optionsFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public JwtBearerOptionsProvider(IOptionsFactory<JwtBearerOptions> optionsFactory, IHttpContextAccessor httpContextAccessor)
        {
            _cache = new ConcurrentDictionary<(string, string), Lazy<JwtBearerOptions>>();
            _optionsFactory = optionsFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        public JwtBearerOptions CurrentValue => Get(Options.DefaultName);

        public JwtBearerOptions Get(string name)
        {
            ClaimsPrincipal? user = _httpContextAccessor.HttpContext?.User;
            string? issuer = user?.Claims.FirstOrDefault()?.OriginalIssuer;

            if (issuer == null)
            {
                throw new ArgumentNullException(nameof(issuer));
            }

            Lazy<JwtBearerOptions> Create() => new Lazy<JwtBearerOptions>(() => _optionsFactory.Create(name));
            return _cache.GetOrAdd((name, issuer), _ => Create()).Value;
        }

        public IDisposable OnChange(Action<JwtBearerOptions, string> listener) => null;
    }
}
