using System.Threading.Tasks;
using AmsApi.Models;

namespace AmsApi.Interfaces
{
    public interface IAdminService
    {
        Task<AuthResponse> LoginAsync(LoginDto dto);
    }
}
