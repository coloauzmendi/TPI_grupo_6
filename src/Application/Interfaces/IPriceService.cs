using Application.Models;

namespace Application.Interfaces
{
    public interface IPriceService
    {
        int Update(string cuit, PriceUpdateRequest request);
    }
}
