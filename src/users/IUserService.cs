using System.Collections.Specialized;

namespace SimpleMDB;

public interface IUSerService
{
    public Task<Result<PagedResult<User>>> ReadAll(int page, int size);
    public Task<Result<User>> Create(User user);
    public Task<Result<User>> Read(int id);
    public Task<Result<User>> Update(int id, User newUser);
    public Task<Result<User>> Delete(int id);
    public Task<Result<string>> GetToken(string username, string password);
    public Task<Result<NameValueCollection>> ValidateToken(string token);
}
