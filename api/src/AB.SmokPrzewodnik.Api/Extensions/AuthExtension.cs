using AB.SmokPrzewodnik.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace AB.SmokPrzewodnik.Api.Extensions;

public static class AuthExtension
{
    public static IServiceCollection AddSupabaseAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var supabaseConfig = configuration.GetSection("Supabase");
        ArgumentNullException.ThrowIfNull(supabaseConfig);

        var authority = $"{supabaseConfig.GetValue("Url", string.Empty)}/auth/v1";
        var audience = supabaseConfig.GetValue("Audience", string.Empty);
        var claimRole = supabaseConfig.GetValue("ClaimRole", string.Empty);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(builder =>
            {
                builder.Authority = authority;
                builder.Audience = audience;

                builder.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    NameClaimType = "sub",
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(AppConsts.AuthPolicyName, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("role", claimRole);
            });
        });

        return services;
    }
}
