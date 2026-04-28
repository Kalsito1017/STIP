using System.Reflection;
using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SofiaTransport.Application.Common.Behaviors;
using SofiaTransport.Application.Common.Interfaces;

namespace SofiaTransport.Api.DependencyInjection;

public static class ApiServiceRegistration
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetAssembly(
                typeof(SofiaTransport.Application.Routes.GetRoutesQuery))!);
            cfg.RegisterServicesFromAssembly(Assembly.GetAssembly(
                typeof(SofiaTransport.Application.Users.RegisterUserCommand))!);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssemblyContaining<SofiaTransport.Application.Routes.GetRoutesQuery>();
        services.AddValidatorsFromAssemblyContaining<SofiaTransport.Application.Users.RegisterUserCommand>();
        services.AddFluentValidationAutoValidation();

        services.AddControllers();
        services.AddSignalR();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]
                            ?? throw new InvalidOperationException("Jwt:Secret configuration is required"))),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization();

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
                policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials());
        });

        services.AddResponseCompression(options =>
            options.EnableForHttps = false);

        return services;
    }
}
