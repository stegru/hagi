namespace Hagi.HostServer.Auth
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Security.Authentication;
    using System.Security.Cryptography;
    using System.Threading.Tasks;
    using Configuration;
    using GuestIntegration;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.Controllers;

    /// <summary>
    /// Middleware to provide authentication for the configuration web interface.
    /// (very simple single-user)
    /// </summary>
    public class ConfigAuthMiddleware
    {
        private readonly RequestDelegate _next;
        private static readonly TimeSpan TokenTimeout = TimeSpan.FromHours(4);

        public ConfigAuthMiddleware(RequestDelegate next)
        {
            this._next = next;
        }

        private static readonly Dictionary<string, DateTime> AuthTokens = new Dictionary<string, DateTime>();

        /// <summary>
        /// Check if a token is valid, and update the expiry.
        /// </summary>
        /// <returns>true if the token is good.</returns>
        private static bool CheckToken(string token)
        {
            if (ConfigAuthMiddleware.AuthTokens.TryGetValue(token, out DateTime expires) && expires > DateTime.UtcNow)
            {
                ConfigAuthMiddleware.NewToken(token);
                return true;
            }

            ConfigAuthMiddleware.AuthTokens.Remove(token);
            return false;
        }

        /// <summary>Create a new token, or update an existing one.</summary>
        /// <returns>The token.</returns>
        public static string NewToken(string? existingToken = null)
        {
            DateTime expires = DateTime.UtcNow + ConfigAuthMiddleware.TokenTimeout;
            string token;

            if (string.IsNullOrEmpty(existingToken))
            {
                byte[] bytes = new byte[15];
                RandomNumberGenerator.Fill(bytes);
                token = Convert.ToBase64String(bytes);
            }
            else
            {
                token = existingToken;
            }

            ConfigAuthMiddleware.AuthTokens[token] = expires;

            // Purge old ones
            foreach (string key in ConfigAuthMiddleware.AuthTokens.Where(kv => kv.Value < DateTime.UtcNow)
                .Select(kv => kv.Key))
            {
                ConfigAuthMiddleware.AuthTokens.Remove(key);
            }

            return token;
        }

        /// <summary>
        /// Check the username and password.
        /// </summary>
        public static void Authenticate(Config config, string username, string password)
        {
            if (!config.CheckPassword(username, password))
            {
                throw new InvalidCredentialException();
            }
        }

        public async Task Invoke(HttpContext context, Config config)
        {

            bool authRequired;

            // Check for the AuthConfig or NoAuth attribute on the action
            MethodInfo? action = context.GetEndpoint()?.Metadata.GetMetadata<ControllerActionDescriptor>()?.MethodInfo;
            if (action?.GetCustomAttribute<ConfigAuthAttribute>() != null)
            {
                authRequired = true;
            }
            else if (action?.GetCustomAttribute<NoAuthAttribute>() != null)
            {
                authRequired = false;
            }
            else
            {
                // Check for the attribute on the controller
                TypeInfo? controller = context.GetEndpoint()?.Metadata.GetMetadata<ControllerActionDescriptor>()
                    ?.ControllerTypeInfo;
                authRequired = controller?.GetCustomAttribute<ConfigAuthAttribute>() != null;
            }


            if (authRequired)
            {
                string token = context.Request.Headers["X-AuthToken"].ToString().Trim();

                if (string.IsNullOrEmpty(token))
                {
                    throw new InvalidCredentialException("X-AuthToken header is missing");
                }

                if (!ConfigAuthMiddleware.CheckToken(token))
                {
                    throw new InvalidCredentialException("X-AuthToken is invalid or expired");
                }
            }

            await this._next(context);
        }

    }

    public static class ConfigAuthMiddlewareExtensions
    {
        public static IApplicationBuilder UseConfigAuth(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ConfigAuthMiddleware>();
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    internal sealed class ConfigAuthAttribute : Attribute
    {
        public ConfigAuthAttribute()
        {
        }
    }
}