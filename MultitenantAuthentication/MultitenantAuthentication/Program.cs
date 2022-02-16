using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using MultitenantAuthentication.Config;
using MultitenantAuthentication.Middlewares;
using MultitenantAuthentication.Security;

var builder = WebApplication.CreateBuilder(args);

AuthenticationSettings authenticationSettings = new AuthenticationSettings();
new ConfigureFromConfigurationOptions<AuthenticationSettings>(builder.Configuration.GetSection("AuthenticationSettings")).Configure(authenticationSettings);
builder.Services.AddSingleton(authenticationSettings);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpContextAccessor();

builder.Services.AddSingleton<IOptionsMonitor<JwtBearerOptions>, JwtBearerOptionsProvider>();
builder.Services.AddSingleton<IConfigureOptions<JwtBearerOptions>, JwtBearerOptionsInitializer>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();

// POI: Authorization
builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


// POI: Blocking requests that have JWT token with non-valid issuer
app.UseMiddleware<TokenIssuerValidationMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
