using System;
using System.Collections.Generic;
using System.Text;

namespace ReceiptReader.Models;

public class Receipt
{
    public string MerchantName { get; set; } = string.Empty;
    public DateTime? TransactionDate { get; set; }
    public decimal TotalAmount { get; set; }
    public List<LineItem> LineItems { get; set; } = [];
}

public class LineItem
{
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public Category Category { get; set; }
}

public enum Category
{
    Unknown,
    Groceries,
    Clothing,
    AutoRepair,
    Utilities,
    PreparedFood,
    Entertainment,
    Phone,
    Household
}