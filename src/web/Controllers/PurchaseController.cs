using Application.Interfaces;
using Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using web.Extensions;

namespace web.Controllers;

[ApiController]
[Authorize(Roles = "Owner")]
[Route("api/branch/{branchId}/purchases")]
public class PurchaseController : ControllerBase
{
    private readonly IPurchaseService _purchaseService;

    public PurchaseController(IPurchaseService purchaseService)
    {
        _purchaseService = purchaseService;
    }

    // Registra la compra de mercadería y suma el stock
    [HttpPost]
    public ActionResult<PurchaseResponse> Create(Guid branchId, PurchaseRequest request)
    {
        return StatusCode(StatusCodes.Status201Created, _purchaseService.Create(User.GetCuit(), branchId, request));
    }
}