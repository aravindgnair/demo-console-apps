namespace iText7Pdf;

public class SupplierProduct
{
    public string Description { get; set; } = string.Empty;

    public decimal Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal Amount => Quantity * UnitPrice;
}