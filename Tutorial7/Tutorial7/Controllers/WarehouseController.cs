using Microsoft.AspNetCore.Mvc;
using Tutorial7.Models;
using Tutorial7.Repositories;

namespace Tutorial7.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WarehouseController : ControllerBase
{
    private readonly IWarehouseRepository _warehouseRepository;

    public WarehouseController(IWarehouseRepository warehouseRepository)
    {
        _warehouseRepository = warehouseRepository;
    }

    [HttpGet("CheckProductExistence/{productId}")]
    public async Task<IActionResult> CheckProductExistence(int productId)
    {
        if (!await _warehouseRepository.DoesProductExist(productId))
            return NotFound();
        
        return Ok("Product exist");
    }

    [HttpGet("CheckWarehouseExistence/{warehouseId}")]
    public async Task<IActionResult> CheckWarehouseExistence(int warehouseId)
    {
        if (!await _warehouseRepository.DoesWarehouseExist(warehouseId))
            return NotFound();
        
        return Ok("Warehouse exist");
    }

    [HttpGet("CheckOrder/{productId}/{createdAt}")]
    public async Task<IActionResult> CheckOrder(int productId, DateTime createdAt)
    {
        if (!await _warehouseRepository.DoesOrderExist(productId, createdAt))
            return NotFound();
        
        return Ok("Order exist");
    }
    
    [HttpGet("IsOrderComplete/{orderId}")]
    public async Task<IActionResult> IsOrderComplete(int orderId)
    {
        if (await _warehouseRepository.IsOrderComplete(orderId))
            return Ok("The order is complete.");
        
        return Ok("The order is not complete.");
    }
    
    [HttpPut("UpdateOrderFulfilledAt/{orderId}")]
    public async Task<IActionResult> UpdateOrderFulfilledAt(int orderId)
    {
        await _warehouseRepository.UpdateOrderFulfilledAt(orderId);
        return Ok("Order updated.");
    }
    
    [HttpPost("AddProductToWarehouse")]
    public async Task<IActionResult> AddProductToWarehouse([FromBody] ProductWarehouseDTO productWarehouse)
    {
        if (productWarehouse.amount <= 0)
        {
            return BadRequest("The value of the quantity communicated in the request should be greater than 0.");
        }

        try
        {
            await _warehouseRepository.AddProductToWarehouse(productWarehouse);
            return Ok("Product added.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }

}
