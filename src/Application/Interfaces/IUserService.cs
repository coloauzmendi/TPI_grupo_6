using Application.Models;

namespace Application.Interfaces
{
    public interface IUserService
    {
        List<UserResponse> GetEmployees(string ownerCuit);
        UserResponse GetByCuit(string requesterCuit, string cuit);
        UserResponse Create(string? requesterCuit, UserCreateRequest request);
        void Update(string ownerCuit, string cuit, UserUpdateRequest request);
        void Delete(string ownerCuit, string cuit);
    }
}
