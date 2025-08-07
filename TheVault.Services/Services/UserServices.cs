using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheVault.Entities;

namespace TheVault.Services.Services
{
    public class UserServices
    {
        public void CreateUser(User user)
        {
            // Logic to create a user
            user.UserId = Guid.NewGuid();
            Console.WriteLine($"User has id: {user.UserId}, Password is {user.Password}");
        }
    }
}
