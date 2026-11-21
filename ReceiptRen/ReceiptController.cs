using System.Text;
using MySqlConnector;

namespace ReceiptRen;

/// <summary>
/// Reads and changes receipts in the database using ADO.NET with parameterised SQL.
/// </summary>
public class ReceiptController
{
    private const string SelectSql =
        "SELECT r.receipt_id, r.category_id, r.receipt_date, r.store_name, r.amount, r.note, " +
        "c.category_name " +
        "FROM receipts r " +
        "INNER JOIN categories c ON c.category_id = r.category_id";

    /// <summary>
    /// Returns receipts matching the filter (all receipts when the filter is null),
    /// newest first. Only the options that are set on the filter are applied.
    /// </summary>
    public List<Receipt> Search(ReceiptFilter? filter = null)
    {
        List<Receipt> receipts = new List<Receipt>();

        StringBuilder sql = new StringBuilder(SelectSql);
        List<string> conditions = new List<string>();

        if (filter != null)
        {
            if (filter.From.HasValue) conditions.Add("r.receipt_date >= @from");
            if (filter.To.HasValue) conditions.Add("r.receipt_date <= @to");
            if (filter.CategoryId.HasValue) conditions.Add("r.category_id = @categoryId");
            if (filter.MinAmount.HasValue) conditions.Add("r.amount >= @min");
            if (filter.MaxAmount.HasValue) conditions.Add("r.amount <= @max");
            if (!string.IsNullOrWhiteSpace(filter.Keyword)) conditions.Add("r.store_name LIKE @keyword");
        }

        if (conditions.Count > 0)
            sql.Append(" WHERE ").Append(string.Join(" AND ", conditions));

        sql.Append(" ORDER BY r.receipt_date DESC, r.receipt_id DESC");

        using MySqlConnection connection = new MySqlConnection(Database.ConnectionString);
        connection.Open();

        using MySqlCommand command = new MySqlCommand(sql.ToString(), connection);

        if (filter != null)
        {
            if (filter.From.HasValue) command.Parameters.AddWithValue("@from", filter.From.Value.Date);
            if (filter.To.HasValue) command.Parameters.AddWithValue("@to", filter.To.Value.Date);
            if (filter.CategoryId.HasValue) command.Parameters.AddWithValue("@categoryId", filter.CategoryId.Value);
            if (filter.MinAmount.HasValue) command.Parameters.AddWithValue("@min", filter.MinAmount.Value);
            if (filter.MaxAmount.HasValue) command.Parameters.AddWithValue("@max", filter.MaxAmount.Value);
            if (!string.IsNullOrWhiteSpace(filter.Keyword))
                command.Parameters.AddWithValue("@keyword", "%" + EscapeLike(filter.Keyword.Trim()) + "%");
        }

        using MySqlDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            receipts.Add(new Receipt
            {
                ReceiptId = reader.GetInt32("receipt_id"),
                CategoryId = reader.GetInt32("category_id"),
                ReceiptDate = reader.GetDateTime("receipt_date"),
                StoreName = reader.GetString("store_name"),
                Amount = reader.GetDecimal("amount"),
                Note = reader.GetString("note"),
                CategoryName = reader.GetString("category_name")
            });
        }

        return receipts;
    }

    /// <summary>Returns every receipt dated inside the given month.</summary>
    public List<Receipt> GetByMonth(int month, int year)
    {
        DateTime first = new DateTime(year, month, 1);

        return Search(new ReceiptFilter
        {
            From = first,
            To = first.AddMonths(1).AddDays(-1)
        });
    }

    /// <summary>
    /// Checks a receipt against the rules: amount greater than zero, category and date required.
    /// Returns an error message, or null when the receipt is valid.
    /// (Future dates are only warned about by the form, not rejected.)
    /// </summary>
    public static string? Validate(Receipt receipt)
    {
        if (receipt.Amount <= 0)
            return "Amount must be greater than zero.";
        if (receipt.CategoryId <= 0)
            return "Please choose a category.";
        if (receipt.ReceiptDate == default)
            return "Date is required.";
        if (string.IsNullOrWhiteSpace(receipt.StoreName))
            return "Store or description is required.";
        if (receipt.StoreName.Trim().Length > 120)
            return "Store or description must be 120 characters or fewer.";
        if ((receipt.Note ?? "").Length > 255)
            return "Note must be 255 characters or fewer.";

        return null;
    }

    /// <summary>Inserts a new receipt and stores its generated id on the object.</summary>
    public void Add(Receipt receipt)
    {
        using MySqlConnection connection = new MySqlConnection(Database.ConnectionString);
        connection.Open();

        using MySqlCommand command = new MySqlCommand(
            "INSERT INTO receipts (category_id, receipt_date, store_name, amount, note) " +
            "VALUES (@categoryId, @date, @store, @amount, @note)", connection);
        AddParameters(command, receipt);
        command.ExecuteNonQuery();

        receipt.ReceiptId = (int)command.LastInsertedId;
    }

    /// <summary>Saves changes to an existing receipt.</summary>
    public void Update(Receipt receipt)
    {
        using MySqlConnection connection = new MySqlConnection(Database.ConnectionString);
        connection.Open();

        using MySqlCommand command = new MySqlCommand(
            "UPDATE receipts SET category_id = @categoryId, receipt_date = @date, " +
            "store_name = @store, amount = @amount, note = @note " +
            "WHERE receipt_id = @id", connection);
        AddParameters(command, receipt);
        command.Parameters.AddWithValue("@id", receipt.ReceiptId);
        command.ExecuteNonQuery();
    }

    /// <summary>Deletes a receipt by id.</summary>
    public void Delete(int receiptId)
    {
        using MySqlConnection connection = new MySqlConnection(Database.ConnectionString);
        connection.Open();

        using MySqlCommand command = new MySqlCommand(
            "DELETE FROM receipts WHERE receipt_id = @id", connection);
        command.Parameters.AddWithValue("@id", receiptId);
        command.ExecuteNonQuery();
    }

    /// <summary>Adds the parameters shared by the INSERT and UPDATE commands.</summary>
    private static void AddParameters(MySqlCommand command, Receipt receipt)
    {
        command.Parameters.AddWithValue("@categoryId", receipt.CategoryId);
        command.Parameters.AddWithValue("@date", receipt.ReceiptDate.Date);
        command.Parameters.AddWithValue("@store", receipt.StoreName.Trim());
        command.Parameters.AddWithValue("@amount", receipt.Amount);
        command.Parameters.AddWithValue("@note", (receipt.Note ?? "").Trim());
    }

    /// <summary>Escapes the characters that have a special meaning inside a SQL LIKE pattern.</summary>
    private static string EscapeLike(string text)
    {
        return text.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
    }
}
