using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using MultitenantAuthentication.Config;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace MultitenantAuthentication.Middlewares
{
    public class TokenIssuerValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly AuthenticationSettings _authSettings;

        public TokenIssuerValidationMiddleware(RequestDelegate next, AuthenticationSettings authSettings)
        {
            _next = next;
            _authSettings = authSettings;
        }

        public async Task Invoke(HttpContext context)
        {
            string? token = context.Request.Headers.Authorization.ToString().Replace(JwtBearerDefaults.AuthenticationScheme, "").Trim();
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtSecurityToken = handler.ReadJwtToken(token);

            // attach current user to the context
            context.User = new ClaimsPrincipal(new ClaimsIdentity(jwtSecurityToken.Claims));

            Regex issuerRegex = new Regex(_authSettings.AllowedIssuersRegex);
            bool validIssuer = issuerRegex.IsMatch(jwtSecurityToken.Issuer);

            if (!validIssuer)
            {
                context.Response.StatusCode = 404;
                return;
            }

            await _next(context);
        }
    }
}
