using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mermer.Authorization.Models;
using Mermer.Core.Authorization.Services;
using Mermer.Http;
using Mermer.Ui.Pc.DTOs;


namespace Mermer.Ui.Pc.Services
{

    public class ApiLoginService : LoginService
    {
        private readonly RestClient _restClient;

        public ApiLoginService(RestClient restClient)
        {
            _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
        }

        protected override async Task<User> GetUser(string username, string password)
        {
            try
            {
                // Запрос к API
                var apiResponse = await _restClient.PostAsync<ApiLoginResponse>("/api/auth/login", new
                {
                    Username = username,
                    Password = password
                });

                if (apiResponse == null)
                {
                    throw new InvalidOperationException("Неверный логин или пароль!");
                }

                // Определяем статус администратора по полю Role из DTO
                bool isAdmin = string.Equals(apiResponse.Role, "Admin", StringComparison.OrdinalIgnoreCase);

                // Маппим ответ в доменную модель User WPF-клиента
                var user = new User
                {
                    Id = apiResponse.Id,
                    Username = apiResponse.Username,
                    IsAdmin = isAdmin
                };

                return user;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Ошибка входа через API: {ex.Message}", ex);
            }
        }

        protected override async Task<IEnumerable<Role>> GetRoles(IEnumerable<string> roleIds)
        {
            try
            {
                var roles = await _restClient.PostAsync<List<Role>>("/api/auth/roles", roleIds);
                if (roles != null && roles.Count > 0)
                {
                    return roles;
                }
            }
            catch
            {
                // Игнорируем ошибки запроса к нереализованному эндпоинту
            }

            // Локальный массив/список ролей по умолчанию (заглушка)
            return new List<Role>
            {
                new Role
                {
                    Id = "admin",
                    Name = "Administrator"
                }
            };
        }

        public override async Task UpdatePassword(string currentPassword, string newPassword)
        {
            await _restClient.PostAsync<object>("/api/auth/update-password", new
            {
                userId = this.Session?.UserId,
                currentPassword = currentPassword,
                newPassword = newPassword
            });
        }
    }
}