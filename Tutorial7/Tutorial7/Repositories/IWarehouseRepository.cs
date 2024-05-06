using Tutorial7.Models;

namespace Tutorial7.Repositories;

public interface IWarehouseRepository
{
    Task<bool> DoesProductExist(int id);
    Task<bool> DoesWarehouseExist(int id);
    Task<bool> DoesOrderExist(int id, DateTime createdAt);
    Task<bool> IsOrderComplete(int orderId);
    Task UpdateOrderFulfilledAt(int orderId);
    Task AddProductToWarehouse(ProductWarehouseDTO productWarehouse);

}