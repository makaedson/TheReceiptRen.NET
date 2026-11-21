namespace ReceiptRen;

/// <summary>
/// Represents a spending category such as Food or Transport.
/// One category has many receipts.
/// </summary>
public class Category
{
    /// <summary>Primary key of the category (categories.category_id).</summary>
    public int CategoryId { get; set; }

    /// <summary>Display name of the category (categories.category_name).</summary>
    public string CategoryName { get; set; } = "";

    /// <summary>
    /// Text shown when a Category object is placed directly in a ComboBox or ListBox.
    /// </summary>
    public override string ToString()
    {
        return CategoryName;
    }
}
