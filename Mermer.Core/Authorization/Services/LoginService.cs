using Mermer.Authorization.Enums;
using Mermer.Authorization.Models;
using Mermer.Authorization.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mermer.Core.Authorization.Services;

public abstract class LoginService : ILoginService
{
    public UserSession? Session { get; set; }

    public virtual bool IsLoggedIn => this.Session != null;

    public async Task LoginAsync(string username, string password)
    {
        if (string.IsNullOrEmpty(username))
            throw new ArgumentNullException(nameof(username));
        if (string.IsNullOrEmpty(password))
            throw new ArgumentNullException(nameof(password));

        try
        {
            User user = await this.GetUser(username, password);

            if (user.IsDisabled)
                throw new InvalidOperationException("User is disabled.");

            IEnumerable<Role> roles;
            if (user.Roles == null || !user.Roles.Any())
                roles = Array.Empty<Role>();
            else
                roles = await this.GetRoles(user.Roles);

            // Замінили __nonvirtual на нормальне привласнення
            this.Session = new UserSession()
            {
                UserId = user.Id,
                Username = user.Username,
                IsAdmin = user.IsAdmin,
                Accounts = user.AccountPrivileges ?? new Dictionary<string, AccountAccessLevel>(),
                Roles = roles.SelectMany(x => x.Authorizations)
                           .GroupBy(x => x.Key)
                           .ToDictionary(
                               g => g.Key,
                               // Замінили проблемний AddBit на стандартне бітове об'єднання прав
                               g => g.Select(x => x.Value).Aggregate((current, next) => current | next)
                           )
            };
        }
        catch (Exception)
        {
            // Замінили __nonvirtual
            this.Session = null;
            throw;
        }
    }

    public Task LogoutAsync() => Task.Run(() => this.Session = null);

    public abstract Task UpdatePassword(string currentPassword, string newPassword);

    protected abstract Task<User> GetUser(string username, string password);

    protected abstract Task<IEnumerable<Role>> GetRoles(IEnumerable<string> roles);
}