using System.Data.SQLite;
using System.Diagnostics.CodeAnalysis;
using Chase.CommonLib.Math;

namespace DemoAuthenticationSystem.Lib;

/// <summary>
/// The UserManager class handles user creation and management.
/// </summary>
public sealed class UserManager : IDisposable
{
    private readonly SQLiteConnection _connection;

    /// <summary>
    /// Represents the SQL statement for creating the Users table in the database.
    /// </summary>
    private const string CreateTableSql = """
                                                  CREATE TABLE IF NOT EXISTS Users (
                                                      Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                                      Username VARCHAR(256) NOT NULL,
                                                      Email VARCHAR(320) NOT NULL,
                                                      Password TEXT NOT NULL
                                                  );
                                          """;

    /// <summary>
    /// The file path to the SQLite database used for storing user information.
    /// </summary>
    private const string DatabaseFile = "users.db";

    public UserManager()
    {
        _connection = new SQLiteConnection($"Data Source={DatabaseFile};Version=3;");
        _connection.Open();

        // Create the Users table if it does not exist.
        using var command = new SQLiteCommand(CreateTableSql, _connection);
        command.ExecuteNonQuery();
    }

    /// <summary>
    /// Creates a new user and stores it in the database.
    /// </summary>
    /// <param name="user">The user object containing the user details.</param>
    public void CreateUser(User user)
    {
        // Encrypt the password before storing it in the database.
        var encryption = new Crypt();
        user.Password = encryption.Encrypt(user.Password);

        // Insert the user into the database.
        using var command = new SQLiteCommand("INSERT INTO Users (Username, Email, Password) VALUES (@Username, @Email, @Password);", _connection);
        command.Parameters.AddWithValue("@Username", user.Username);
        command.Parameters.AddWithValue("@Email", user.Email);
        command.Parameters.AddWithValue("@Password", user.Password);
        command.ExecuteNonQuery();
    }

    /// <summary>
    /// Creates a new user and stores their information in the database.
    /// The password is encrypted before storing it.
    /// </summary>
    /// <param name="username">The username of the user.</param>
    /// <param name="password">The password of the user.</param>
    /// <param name="email">The email of the user. Defaults to an empty string if not provided.</param>
    public void CreateUser(string username, string password, string email = "")
    {
        // Create a new user object and call the CreateUser method.
        CreateUser(new User() { Username = username, Email = email, Password = password });
    }

    /// <summary>
    /// Gets the user with the specified ID from the database.
    /// </summary>
    /// <param name="id">The ID of the user.</param>
    /// <returns>The user object with the specified ID, or null if the user is not found.</returns>
    public User? GetUserById(int id)
    {
        using var command = new SQLiteCommand("SELECT * FROM Users WHERE Id = @Id", _connection);
        command.Parameters.AddWithValue("@Id", id);
        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return new User()
            {
                Id = reader.GetInt32(0),
                Username = reader.GetString(1),
                Email = reader.GetString(2),
                Password = reader.GetString(3)
            };
        }

        return null;
    }

    /// <summary>
    /// Tries to get a user by their ID from the database.
    /// </summary>
    /// <param name="id">The ID of the user to retrieve.</param>
    /// <param name="user">The retrieved user. Null if the user doesn't exist.</param>
    /// <returns>True if the user exists, false otherwise.</returns>
    public bool TryGetUserById(int id, [NotNullWhen(true)] out User? user) => (user = GetUserById(id)) != null;

    public void Dispose()
    {
        _connection.Close();
        _connection.Dispose();
    }
}