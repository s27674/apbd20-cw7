namespace Tutorial7.Models;

public class ProductWarehouseDTO
{
    public ProductDTO Product { get; set; }
    public WarehouseDTO Warehouse { get; set; }
    public int amount { get; set; }
    public DateTime createdAt { get; set; }
}
public record ProductDTO
{
    public int idProduct { get; set; }
}
public record WarehouseDTO
{
    public int idWarehouse { get; set; }
}