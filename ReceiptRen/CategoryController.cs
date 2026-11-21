using MySqlConnector;

namespace ReceiptRen;

/// <summary>
/// Reads and changes categories in the database using ADO.NET.
/// User-facing problems (empty name, duplicate, category in use) are raised as
/// InvalidOperationException with a readable message.
/// </summary>
public class CategoryController
{
    /// <summary>Returns all categories sorted by name.</summary>
    public List<Category> GetAll()
    {
        List<Category> categories = new List<Category>();

        using MySqlConnection connection = new MySqlConnection(Database.ConnectionString);
        connection.Open();

        using MySqlCommand command = new MySqlCommand(
            "SELECT category_id, category_name FROM categories ORDER BY category_name", connection);

        using MySqlDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            categories.Add(new Category
            {
                CategoryId = reader.GetInt32("category_id"),
                CategoryName = reader.GetString("category_name")
            });
        }

        return categories;
    }

    /// <summary>Adds a new category and returns it with its generated id.</summary>
    public Category Add(string name)
    {
        name = CleanName(name);

        try
        {
            using MySqlConnection connection = new MySqlConnection(Database.ConnectionString);
            connection.Open();

            using MySqlCommand command = new MySqlCommand(
                "INSERT INTO categories (category_name) VALUES (@name)", connection);
            command.Parameters.AddWithValue("@name", name);
            command.ExecuteNonQuery();

            return new Category { CategoryId = (int)command.LastInsertedId, CategoryName = name };
        }
        catch (MySqlException ex) when (ex.ErrorCode == MySqlErrorCode.DuplicateKeyEntry)
        {
            throw new InvalidOperationException("A category named \"" + name + "\" already exists.");
        }
    }

    /// <summary>Renames an existing category.</summary>
    public void Rename(int categoryId, string newName)
    {
        newName = CleanName(newName);

        try
        {
            using MySqlConnection connection = new MySqlConnection(Database.ConnectionString);
            connection.Open();

            using MySqlCommand command = new MySqlCommand(
                "UPDATE categories SET category_name = @name WHERE category_id = @id", connection);
            command.Parameters.AddWithValue("@name", newName);
            command.Parameters.AddWithValue("@id", categoryId);
            command.ExecuteNonQuery();
        }
        catch (MySqlException ex) when (ex.ErrorCode == MySqlErrorCode.DuplicateKeyEntry)
        {
            throw new InvalidOperationException("A category named \"" + newName + "\" already exists.");
        }
    }

    /// <summary>Deletes a category. A category that still has receipts cannot be deleted.</summary>
    public void Delete(int categoryId)
    {
        if (CountReceipts(categoryId) > 0)
        {
            throw new InvalidOperationException(
                "This category is used by existing receipts and cannot be deleted. " +
                "Change or delete those receipts first.");
        }

        try
        {
            using MySqlConnection connection = new MySqlConnection(Database.ConnectionString);
            connection.Open();

            using MySqlCommand command = new MySqlCommand(
                "DELETE FROM categories WHERE category_id = @id", connection);
            command.Parameters.AddWithValue("@id", categoryId);
            command.ExecuteNonQuery();
        }
        catch (MySqlException ex) when (ex.ErrorCode == MySqlErrorCode.RowIsReferenced
                                        || ex.ErrorCode == MySqlErrorCode.RowIsReferenced2)
        {
            // Safety net: the foreign key refuses the delete even if the pre-check was bypassed.
            throw new InvalidOperationException("This category is used by existing receipts and cannot be deleted.");
        }
    }

    /// <summary>Counts how many receipts use the given category.</summary>
    public int CountReceipts(int categoryId)
    {
        using MySqlConnection connection = new MySqlConnection(Database.ConnectionString);
        connection.Open();

        using MySqlCommand command = new MySqlCommand(
            "SELECT COUNT(*) FROM receipts WHERE category_id = @id", connection);
        command.Parameters.AddWithValue("@id", categoryId);

        return Convert.ToInt32(command.ExecuteScalar());
    }

    /// <summary>Trims the name and rejects empty or too long names.</summary>
    private static string CleanName(string name)
    {
        name = (name ?? "").Trim();

        if (name.Length == 0)
            throw new InvalidOperationException("Category name is required.");
        if (name.Length > 80)
            throw new InvalidOperationException("Category name must be 80 characters or fewer.");

        return name;
    }
}
