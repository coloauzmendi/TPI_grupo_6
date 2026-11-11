using Application.Models;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using web.Extensions;

namespace web.Controllers;

[ApiController]
[Authorize(Roles = "Owner")]
[Route("api/branch/{branchId}/purchases")]
public class PurchaseController : ControllerBase
{
    private readonly PurchaseService _purchaseService;

    public PurchaseController(PurchaseService purchaseService)
    {
        _purchaseService = purchaseService;
    }

    // Registra la compra de mercadería y suma el stock
    [HttpPost]
    public ActionResult<PurchaseResponse> Create(Guid branchId, PurchaseRequest request)
    {
        return Ok(_purchaseService.Create(User.GetCuit(), branchId, request));
    }
}