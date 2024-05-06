using Microsoft.Data.SqlClient;
using Tutorial7.Models;

namespace Tutorial7.Repositories;

public class WarehouseRepository : IWarehouseRepository
{
    private readonly IConfiguration _configuration;

    public WarehouseRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<bool> DoesProductExist(int id)
    {
        var query = "SELECT 1 FROM Product WHERE IdProduct = @IdProduct";

        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@IdProduct", id);

        await connection.OpenAsync();

        var res = await command.ExecuteScalarAsync();

        return res is not null;
    }

    public async Task<bool> DoesWarehouseExist(int id)
    {
        var query = "SELECT 1 FROM Warehouse WHERE IdWarehouse = @IdWarehouse";

        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@IdWarehouse", id);

        await connection.OpenAsync();

        var res = await command.ExecuteScalarAsync();

        return res is not null;
    }

    public async Task<bool> DoesOrderExist(int id, DateTime createdAt)
    {
        var query = "SELECT 1 FROM [Order] WHERE IdProduct = @IdProduct AND Amount > 0 AND CreatedAt = @createdAt";

        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@IdProduct", id);
        command.Parameters.AddWithValue("@CreatedAt", createdAt);

        await connection.OpenAsync();

        var res = await command.ExecuteScalarAsync();

        return res is not null;
    }
    public async Task<bool> IsOrderComplete(int orderId)
    {
        var query = "SELECT 1 FROM Product_Warehouse WHERE IdOrder = @IdOrder";

        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@IdOrder", orderId);

        await connection.OpenAsync();

        var res = await command.ExecuteScalarAsync();

        return res is not null;
    }
    
    private async Task<int> GetOrderId(int productId, DateTime createdAt)
    {
        var query = "SELECT IdOrder FROM [Order] WHERE IdProduct = @IdProduct AND CreatedAt = @CreatedAt";

        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@IdProduct", productId);
        command.Parameters.AddWithValue("@CreatedAt", createdAt);

        await connection.OpenAsync();

        // Получаем Id заказа
        var orderId = await command.ExecuteScalarAsync();

        if (orderId == null)
        {
            throw new Exception("Failed to find the Order Id for the specified product and creation date.");
        }

        return (int)orderId;
    }
    
    public async Task UpdateOrderFulfilledAt(int orderId)
    {
        var query = "UPDATE [Order] SET FulfilledAt = GETDATE() WHERE IdOrder = @IdOrder";

        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@IdOrder", orderId);

        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }
    
    public async Task AddProductToWarehouse(ProductWarehouseDTO productWarehouse)
{
    if (!await DoesOrderExist(productWarehouse.Product.idProduct, productWarehouse.createdAt))
    {
        throw new Exception("There is no order for this product or the order was placed after the product was added to stock.");
    }

    decimal productPrice = await GetProductPrice(productWarehouse.Product.idProduct);

    decimal warehousePrice = productPrice * productWarehouse.amount;

    var query =
        @"INSERT INTO Product_Warehouse (IdProduct, IdWarehouse, IdOrder, Amount, Price, CreatedAt) 
        VALUES (@IdProduct, @IdWarehouse, @IdOrder, @Amount, @Price, GETDATE())";
    
    // Получаем Id заказа для указанного продукта и даты создания
    int orderId = await GetOrderId(productWarehouse.Product.idProduct, productWarehouse.createdAt);

    using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
    using SqlCommand command = new SqlCommand();

    command.Connection = connection;
    command.CommandText = query;
    command.Parameters.AddWithValue("@IdProduct", productWarehouse.Product.idProduct);
    command.Parameters.AddWithValue("@IdWarehouse", productWarehouse.Warehouse.idWarehouse);
    command.Parameters.AddWithValue("@IdOrder", orderId);
    command.Parameters.AddWithValue("@Amount", productWarehouse.amount);
    command.Parameters.AddWithValue("@Price", warehousePrice);

    await connection.OpenAsync();
    await command.ExecuteNonQueryAsync();
}

    private async Task<decimal> GetProductPrice(int productId)
    {
        var query = "SELECT Price FROM Product WHERE IdProduct = @IdProduct";

        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@IdProduct", productId);

        await connection.OpenAsync();

        var price = await command.ExecuteScalarAsync();

        if (price == null || price == DBNull.Value)
        {
            throw new Exception("Unable to find a price for the specified product.");
        }

        return Convert.ToDecimal(price);
    }
}