using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NandaFood_Auth.Models.Global;
using NandaFood_Auth.Services;
using NandaFood_Menu.Data;
using NandaFood_Order.Data;
using NandaFood_Order.Models;
using NandaFood_Order.Models.DTO;

namespace NandaFood_Order.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrderController(NandaFoodOrderContext orderContext, NandaFoodMenuContext menuContext) : ControllerBase
{
    [HttpGet("get-all")]
    [Authorize(Roles = "NFA")]
    public async Task<IActionResult?> GetAllOrder()
    {
        IEnumerable<FoodOrder> results = await orderContext.FoodOrders.ToListAsync();
        
        if (!results.Any())
        {
            return BadRequest(new ApiMessage<object>()
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Status = "Bad Request",
                Message = "Data empty"
            });
        }
            
        return Ok(new ApiMessage<object>()
        {
            StatusCode = (int)HttpStatusCode.OK,
            Status = "OK",
            Message = "Success get all data",
            Data = results
        });
    }
    
    [HttpGet("check-menu")]
    [Authorize(Roles = "NFA")]
    public async Task<IActionResult?> GetOrderByMenu([FromQuery] string menu)
    {
        IEnumerable<FoodOrder> results = await orderContext.FoodOrders.Where(x => x.Menu == menu).ToListAsync();
        
        if (!results.Any())
        {
            return BadRequest(new ApiMessage<object>()
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Status = "Bad Request",
                Message = $"There is no {menu} order"
            });
        }
            
        return Ok(new ApiMessage<object>()
        {
            StatusCode = (int)HttpStatusCode.OK,
            Status = "OK",
            Message = $"Success get all {menu} order",
            Data = results
        });
    }
    
    [HttpGet("check")]
    [Authorize(Roles = "NFM")]
    public async Task<IActionResult?> CheckOrder()
    {
        string token = JwtTokenService.ExtractTokenFromRequest(HttpContext.Request);
        var tokenDetails = JwtTokenService.GetTokenDetails(token);
        IEnumerable<FoodOrder> results = await orderContext.FoodOrders.Where(x => x.OrderBy == tokenDetails.Item1).ToListAsync();
        
        if (!results.Any())
        {
            return BadRequest(new ApiMessage<object>()
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Status = "Bad Request",
                Message = "You don't have an order"
            });
        }
            
        return Ok(new ApiMessage<object>()
        {
            StatusCode = (int)HttpStatusCode.OK,
            Status = "OK",
            Message = "Success get all your orders",
            Data = results
        });
    }
    
    [HttpPost("add")]
    [Authorize(Roles = "NFM")]
    public async Task<IActionResult> AddOrder([FromBody] AddOrderRequest addOrderRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiMessage<object>()
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Status = "Bad Request",
                Message = "Please provide all the required fields"
            });
        }
        
        bool menuExists = await menuContext.FoodMenu.AnyAsync(r => r.Menu == addOrderRequest.Menu);
        if (!menuExists)
        {
            return BadRequest(new ApiMessage<object>()
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Status = "Bad Request",
                Message = $"Menu {addOrderRequest.Menu} does not exist"
            });
        }

        string token = JwtTokenService.ExtractTokenFromRequest(HttpContext.Request);
        var tokenDetails = JwtTokenService.GetTokenDetails(token);
        var checkOrder = await orderContext.FoodOrders.FirstOrDefaultAsync(r => r.OrderBy == tokenDetails.Item1);
        if (checkOrder != null)
        {
            bool orderedMenuExists = addOrderRequest.Menu == checkOrder.Menu;
            
            if (orderedMenuExists && checkOrder.OrderStatus != "Done")
            {
                return BadRequest(new ApiMessage<object>()
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Status = "Bad Request",
                    Message = $"You already ordered {addOrderRequest.Menu}"
                });
            }
        }
        var getPrice = await menuContext.FoodMenu.FirstOrDefaultAsync(x => x.Menu == addOrderRequest.Menu);
    
        FoodOrder newOrder = new FoodOrder()
        {
            Id = Guid.NewGuid().ToString(),
            Name = tokenDetails.Item2,
            Menu = addOrderRequest.Menu,
            Quantity = addOrderRequest.Quantity,
            TotalPrice = getPrice.Price * addOrderRequest.Quantity,
            OrderStatus = "Ordered",
            OrderBy = tokenDetails.Item1,
            OrderDate = DateTime.Now
        };
                
        orderContext.FoodOrders.Add(newOrder);
        await orderContext.SaveChangesAsync();
                
        return Ok(new ApiMessage<object>()
        {
            StatusCode = (int)HttpStatusCode.OK,
            Status = "OK",
            Message = "Order created"
        });
    }
    
    [HttpPut("update")]
    [Authorize(Roles = "NFM")]
    public async Task<IActionResult> UpdateOrder([FromBody] UpdateOrderRequest updateOrderRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiMessage<object>()
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Status = "Bad Request",
                Message = "Please provide all the required fields"
            });
        }
        
        var getOrder = await orderContext.FoodOrders.FirstOrDefaultAsync(r => r.Id == updateOrderRequest.Id);
        if (getOrder == null)
        {
            return BadRequest(new ApiMessage<object>()
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Status = "Bad Request",
                Message = "Order does not exist"
            });
        }

        if (updateOrderRequest.Menu != null)
        {
            bool menuExists = await menuContext.FoodMenu.AnyAsync(r => r.Menu == updateOrderRequest.Menu);
            if (!menuExists)
            {
                return BadRequest(new ApiMessage<object>()
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Status = "Bad Request",
                    Message = $"Menu {updateOrderRequest.Menu} does not exist"
                });
            }
        }

        if (getOrder.OrderStatus is "Processed" or "Done")
        {
            return BadRequest(new ApiMessage<object>()
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Status = "Bad Request",
                Message = "Your order is being processed, can't update"
            });
        }
        
        string token = JwtTokenService.ExtractTokenFromRequest(HttpContext.Request);
        var tokenDetails = JwtTokenService.GetTokenDetails(token);
        var getPrice = await menuContext.FoodMenu.FirstOrDefaultAsync(x => x.Menu == (updateOrderRequest.Menu ?? getOrder.Menu));

        getOrder.Menu = updateOrderRequest.Menu ?? getOrder.Menu;
        getOrder.Quantity = updateOrderRequest.Quantity ?? getOrder.Quantity;
        getOrder.TotalPrice = getPrice.Price * updateOrderRequest.Quantity ?? getOrder.TotalPrice;
        getOrder.UpdatedBy = tokenDetails.Item1;
        getOrder.UpdatedDate = DateTime.Now;
        
        await orderContext.SaveChangesAsync();
                
        return Ok(new ApiMessage<object>()
        {
            StatusCode = (int)HttpStatusCode.OK,
            Status = "OK",
            Message = "Order updated"
        });
    }
    
    [HttpPut("update-status")]
    [Authorize(Roles = "NFA")]
    public async Task<IActionResult> UpdateStatus([FromBody] UpdateStatusRequest updateStatusRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiMessage<object>()
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Status = "Bad Request",
                Message = "Please provide all the required fields"
            });
        }
        
        var getOrder = await orderContext.FoodOrders.FirstOrDefaultAsync(r => r.Id == updateStatusRequest.Id);
        if (getOrder == null)
        {
            return BadRequest(new ApiMessage<object>()
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Status = "Bad Request",
                Message = "Order does not exist"
            });
        }
        
        string token = JwtTokenService.ExtractTokenFromRequest(HttpContext.Request);
        var tokenDetails = JwtTokenService.GetTokenDetails(token);

        getOrder.OrderStatus = updateStatusRequest.Status;
        getOrder.UpdatedBy = tokenDetails.Item1;
        getOrder.UpdatedDate = DateTime.Now;
        
        await orderContext.SaveChangesAsync();
                
        return Ok(new ApiMessage<object>()
        {
            StatusCode = (int)HttpStatusCode.OK,
            Status = "OK",
            Message = "Order updated"
        });
    }
    
    [HttpDelete("delete")]
    [Authorize(Roles = "NFM")]
    public async Task<IActionResult?> DeleteOrder([FromBody] DeleteOrderRequest deleteOrderRequest)
    {
        var getOrder = await orderContext.FoodOrders.FindAsync(deleteOrderRequest.Id);
        
        if (getOrder == null)
        {
            return BadRequest(new ApiMessage<object>()
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Status = "Bad Request",
                Message = "Order does not exist"
            });
        }
        
        string token = JwtTokenService.ExtractTokenFromRequest(HttpContext.Request);
        var tokenDetails = JwtTokenService.GetTokenDetails(token);

        if (getOrder.OrderBy != tokenDetails.Item1)
        {
            return BadRequest(new ApiMessage<object>()
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Status = "Bad Request",
                Message = "Can't delete other member order"
            });
        }

        orderContext.FoodOrders.Remove(getOrder);
        await orderContext.SaveChangesAsync();
            
        return Ok(new ApiMessage<object>()
        {
            StatusCode = (int)HttpStatusCode.OK,
            Status = "OK",
            Message = "Success delete order"
        });
    }
}