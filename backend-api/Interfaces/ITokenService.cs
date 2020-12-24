using backend_api.Entities;

namespace backend_api.Interfaces
{
    public interface ITokenService
    {
        string CreateToken(AppUser user);
    }
}
