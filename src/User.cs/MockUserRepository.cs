using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleMDB
{
    public class MockUserRepository : IUserRepository
    {
        private List<User> users;
        private int idCount;

        public MockUserRepository()
        {
            users = new List<User>();
            idCount = 0;

            var usernames = new string[]
            {
                "raul", "san paolo", "alejandro", "Calloway",
                "Mr.sensei", "Rauw", "gtr", "Rosalia",
                "cosa nuestra", "japon", "taro", "ip.Capo"
            };

            Random r = new Random();
            foreach (var username in usernames)
            {
                var pass = Path.GetRandomFileName().Replace(".", "");
                var salt = Path.GetRandomFileName().Replace(".", "");
                var role = Roles.ROLES[r.Next(Roles.ROLES.Length)];
                User user = new User(idCount++, username, pass, salt, role);
                users.Add(user);
            }
        }

        public async Task<PagedResult<User>> ReadAll(int page, int size)
        {
            int totalCount = users.Count;
            int start = Math.Clamp((page - 1) * size, 0, totalCount);
            int end = Math.Clamp(start + size, 0, totalCount);
            int length = end - start;
            
            List<User> values = users.Skip(start).Take(length).ToList();
            var pagedResult = new PagedResult<User>(values, totalCount);

            return await Task.FromResult(pagedResult);
        }

        public async Task<User?> Create(User user)
        {
            user.Id = idCount++;
            users.Add(user);
            return await Task.FromResult(user);
        }

        public async Task<User?> Read(int id)
        {
            User? user = users.FirstOrDefault((u) => u.Id == id);
            return await Task.FromResult(user);
        }

        public async Task<User?> Update(int id, User newUser)
        {
            User? user = users.FirstOrDefault((u) => u.Id == id);

            if (user != null)
            {
                user.Username = newUser.Username;
                user.Password = newUser.Password;
                user.Salt = newUser.Salt;
                user.Role = newUser.Role;
            }

            return await Task.FromResult(user);
        }

        public async Task<User?> Delete(int id)
        {
            User? user = users.FirstOrDefault((u) => u.Id == id);

            if (user != null)
            {
                users.Remove(user);
            }
           
            return await Task.FromResult(user);
        }
    }
} 