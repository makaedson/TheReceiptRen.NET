namespace ReceiptRen;

/// <summary>
/// Represents one purchase recorded by the user (one row of the receipts table).
/// </summary>
public class Receipt
{
    /// <summary>Primary key of the receipt (receipts.receipt_id).</summary>
    public int ReceiptId { get; set; }

    /// <summary>Foreign key to the category this receipt belongs to.</summary>
    public int CategoryId { get; set; }

    /// <summary>Date of the purchase.</summary>
    public DateTime ReceiptDate { get; set; } = DateTime.Today;

    /// <summary>Store name or short description of the purchase.</summary>
    public string StoreName { get; set; } = "";

    /// <summary>Amount spent; must be greater than zero.</summary>
    public decimal Amount { get; set; }

    /// <summary>Optional note written by the user.</summary>
    public string Note { get; set; } = "";

    /// <summary>Category name, filled in by the controller (JOIN) for display and reports.</summary>
    public string CategoryName { get; set; } = "";
}
