namespace ReceiptRen;

/// <summary>
/// Holds the search options for receipts. Any property left null is ignored.
/// </summary>
public class ReceiptFilter
{
    /// <summary>Earliest receipt date to include (inclusive).</summary>
    public DateTime? From { get; set; }

    /// <summary>Latest receipt date to include (inclusive).</summary>
    public DateTime? To { get; set; }

    /// <summary>Only include receipts of this category.</summary>
    public int? CategoryId { get; set; }

    /// <summary>Smallest amount to include (inclusive).</summary>
    public decimal? MinAmount { get; set; }

    /// <summary>Largest amount to include (inclusive).</summary>
    public decimal? MaxAmount { get; set; }

    /// <summary>Keyword that must appear in the store name.</summary>
    public string? Keyword { get; set; }
}
