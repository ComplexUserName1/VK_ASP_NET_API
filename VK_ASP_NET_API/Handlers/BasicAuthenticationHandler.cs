using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text;
using System.Threading.Tasks;
using System;
using VK_ASP_NET_API.Models;
using VK_ASP_NET_API.Data;
using Microsoft.EntityFrameworkCore;

namespace VK_ASP_NET_API.Handlers
{
    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly VK_ASP_NET_APIDbContext _context;

        public BasicAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder,
            ISystemClock clock, VK_ASP_NET_APIDbContext context)
            : base(options, logger, encoder, clock)
        {
            _context = context;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
                return AuthenticateResult.Fail("Missing authorization header");

            User user = null;
            try
            {
                var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
                var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':');
                var login = credentials[0];
                var password = credentials[1];

                user = await _context.Users.FirstOrDefaultAsync(u => u.Login == login && u.Password == password);
            }
            catch
            {
                return AuthenticateResult.Fail("Invalid authorization header");
            }

            if (user == null)
                return AuthenticateResult.Fail("Invalid username or password");

            var claims = new[] {
            new Claim(ClaimTypes.Name, user.Login)
        };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
    }
}