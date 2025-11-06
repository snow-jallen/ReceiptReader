namespace ReceiptReader.Models;

public class ProcessedReceipt
{
    public byte[] ImageBytes { get; set; } = [];
    public Receipt Receipt { get; set; } = new();
}
