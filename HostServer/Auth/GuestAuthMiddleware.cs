namespace HostServer.Auth
{
    using System;
    using System.Reflection;
    using System.Security.Authentication;
    using System.Threading.Tasks;
    using Configuration;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.Controllers;

    /// <summary>
    /// Middleware to provide authentication for guests, using http headers `X-Guest` and `X-Secret`.
    /// </summary>
    public class GuestAuthMiddleware
    {
        private readonly RequestDelegate _next;

        public GuestAuthMiddleware(RequestDelegate next)
        {
            this._next = next;
        }

        public async Task Invoke(HttpContext context, Config config)
        {

            MethodInfo? action = context.GetEndpoint()?.Metadata.GetMetadata<ControllerActionDescriptor>()?.MethodInfo;

            bool authRequired = action?.GetCustomAttribute<NoAuthAttribute>() == null;

            string guestId = context.Request.Headers["X-Guest"].ToString().Trim();
            string secret = context.Request.Headers["X-Secret"].ToString().Trim();

            bool authenticated;
            GuestConfig? guestConfig;
            try
            {
                if (string.IsNullOrEmpty(guestId))
                {
                    throw new InvalidCredentialException("X-Guest header is missing");
                }

                if (string.IsNullOrEmpty(secret))
                {
                    throw new InvalidCredentialException("X-Secret header is missing");
                }

                guestConfig = config.GetGuest(guestId, true);

                if (guestConfig == null)
                {
                    throw new InvalidCredentialException("Unknown guest");
                }

                if (guestConfig.Secret != secret)
                {
                    throw new InvalidCredentialException("Wrong secret");
                }

                authenticated = true;
            }
            catch when (!authRequired)
            {
                authenticated = false;
                guestConfig = null;
            }

            if (authenticated && guestConfig != null)
            {
                context.SetGuestConfig(guestConfig);
            }

            await this._next(context);
        }

    }

    public static class GuestAuthMiddlewareExtensions
    {
        public static IApplicationBuilder UseGuestAuth(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<GuestAuthMiddleware>();
        }

        public static GuestConfig GetGuestConfig(this HttpContext context)
        {
            return context.Features.Get<GuestConfig>();
        }
        public static void SetGuestConfig(this HttpContext context, GuestConfig guestConfig)
        {
            context.Features.Set(guestConfig);
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    internal sealed class NoAuthAttribute : Attribute
    {
        public NoAuthAttribute()
        {
        }
    }
}