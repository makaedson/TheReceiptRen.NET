namespace ReceiptRen;

/// <summary>
/// Holds the MySQL connection string used by every controller.
/// Keeping it in one place means the database settings are changed in a single spot.
/// </summary>
public static class Database
{
    /// <summary>
    /// Connection string for the XAMPP MySQL server and the receiptren database.
    /// Change User ID / Password here if your MySQL root user has a password.
    /// </summary>
    public const string ConnectionString = "Server=localhost;Database=receiptren;User ID=root;Password=;";
}
