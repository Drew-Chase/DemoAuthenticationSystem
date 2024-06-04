using System.Data.SQLite;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using Chase.CommonLib.Math;
using HashidsNet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

    /// <summary>
    /// Represents the Hashids instance used for encoding and decoding user IDs.
    /// </summary>
    private readonly Hashids _hashids = new("DemoAuthenticationSystem", 8);

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
    public User GetUserById(string id)
    {
        // Decode the hash ID to get the actual user ID.
        int[]? decodedUserIds = _hashids.Decode(id);
        if (decodedUserIds is null || decodedUserIds.Length == 0) return User.Empty;
        int userId = decodedUserIds[0];

        using var command = new SQLiteCommand("SELECT * FROM Users WHERE Id = @Id LIMIT 1", _connection);
        command.Parameters.AddWithValue("@Id", userId);
        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return new User()
            {
                Id = _hashids.Encode(reader.GetInt32(0)),
                Username = reader.GetString(1),
                Email = reader.GetString(2),
                Password = reader.GetString(3)
            };
        }

        return User.Empty;
    }

    /// <summary>
    /// Tries to get a user by their ID from the database.
    /// </summary>
    /// <param name="id">The ID of the user to retrieve.</param>
    /// <param name="user">The retrieved user. Null if the user doesn't exist.</param>
    /// <returns>True if the user exists, false otherwise.</returns>
    public bool TryGetUserById(string id, out User user) => !(user = GetUserById(id)).IsEmpty;

    /// <summary>
    /// Authenticates a user by verifying the provided username or email and password.
    /// </summary>
    /// <param name="username">The username or email of the user.</param>
    /// <param name="password">The password of the user.</param>
    /// <param name="user">The user that was found, or null if authentication failed.</param>
    /// <returns>
    /// True if the user is authenticated, false otherwise.
    /// </returns>
    public bool AuthenticateUser(string username, string password, out User user)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            user = User.Empty;
            return false;
        }

        using var command = new SQLiteCommand("SELECT * FROM Users WHERE Username = @Username OR Email = @Username LIMIT 1", _connection);
        command.Parameters.AddWithValue("@Username", username);
        using var reader = command.ExecuteReader();
        user = User.Empty;
        if (!reader.Read()) return false;

        var encryption = new Crypt();
        if (encryption.Decrypt(reader.GetString(3)) != password) return false;
        user = new User()
        {
            Id = _hashids.Encode(reader.GetInt32(0)),
            Username = reader.GetString(1),
            Email = reader.GetString(2),
            Password = reader.GetString(3)
        };
        return true;
    }


    /// <summary>
    /// Authenticates a user by verifying the provided username or email and password.
    /// </summary>
    /// <param name="username">The username or email</param>
    /// <param name="password">The plain-text password</param>
    /// <param name="uniqueKey">A unique key to generate the token with</param>
    /// <param name="token">The outputted token or null if authentication failed.</param>
    /// <param name="user">The outputted user or empty user object</param>
    /// <returns></returns>
    public bool AuthenticateUser(string username, string password, string uniqueKey, [NotNullWhen(true)] out string? token, out User user)
    {
        if (!AuthenticateUser(username, password, out user) || user.IsEmpty)
        {
            token = null;
            return false;
        }

        token = GenerateToken(user, uniqueKey);
        return true;
    }

    /// <summary>
    /// Authenticates the user with the provided token and unique key.
    /// </summary>
    /// <param name="token">The token to be used for authentication.</param>
    /// <param name="uniqueKey">The unique key used for token generation.</param>
    /// <param name="user">The authenticated user object if the authentication is successful; otherwise an empty user object.</param>
    /// <returns>True if the user is successfully authenticated; otherwise false.</returns>
    public bool AuthenticateUserWithToken(string token, string uniqueKey, out User user)
    {
        user = GetUserFromToken(token);
        user = GetUserById(user.Id);
        return !user.IsEmpty && GenerateToken(user, uniqueKey) == token;
    }

    /// <summary>
    /// Gets all users from the database.
    /// </summary>
    /// <returns>An array of User objects representing the users.</returns>
    public User[] GetUsers()
    {
        var command = _connection.CreateCommand();
        command.CommandText = "SELECT id,username,email FROM Users";
        var reader = command.ExecuteReader();
        List<User> users = [];
        while (reader.Read())
        {
            users.Add(new User()
            {
                Id = _hashids.Encode(reader.GetInt32(0)),
                Username = reader.GetString(1),
                Email = reader.GetString(2)
            });
        }

        return users.ToArray();
    }

    public User[] SearchUsers(string query, int limit = 10, int offset = 0, string sort = "username", bool ascending = false)
    {
        var command = _connection.CreateCommand();
        command.CommandText = $"SELECT id,username,email FROM Users WHERE username LIKE @Query OR email LIKE @Query ORDER BY @Sort {(ascending ? "ASC" : "DESC")} LIMIT @Limit OFFSET @Offset";
        command.Parameters.AddWithValue("@Query", $"%{query}%");
        command.Parameters.AddWithValue("@Limit", limit);
        command.Parameters.AddWithValue("@Offset", offset);
        command.Parameters.AddWithValue("@Sort", sort);
        var reader = command.ExecuteReader();
        List<User> users = [];
        while (reader.Read())
        {
            users.Add(new User()
            {
                Id = _hashids.Encode(reader.GetInt32(0)),
                Username = reader.GetString(1),
                Email = reader.GetString(2)
            });
        }

        return users.ToArray();
    }

    public void DeleteUser(string id)
    {
        int[]? decodedUserIds = _hashids.Decode(id);
        if (decodedUserIds is null || decodedUserIds.Length == 0) return;
        int userId = decodedUserIds[0];

        using var command = new SQLiteCommand("DELETE FROM Users WHERE Id = @Id", _connection);
        command.Parameters.AddWithValue("@Id", userId);
        command.ExecuteNonQuery();
    }

    /// <summary>
    /// Generates a token for the specified user and unique key.
    /// </summary>
    /// <param name="user">The user for whom the token is generated.</param>
    /// <param name="uniqueKey">The unique key used for generating the token.</param>
    /// <returns>The generated token as a string.</returns>
    public string GenerateToken(User user, string uniqueKey)
    {
        if (user.IsEmpty) throw new ArgumentException("User cannot be empty.", nameof(user));
        var encryption = new Crypt();
        return encryption.Encrypt(JsonConvert.SerializeObject(new { user.Id, user.Username, user.Password, uniqueKey }));
    }

    /// <summary>
    /// Generates a token for the specified user and unique key.
    /// </summary>
    /// <param name="user">The user for whom the token is generated.</param>
    /// <param name="address">The connecting IP Address</param>
    /// <returns>The generated token as a string.</returns>
    public string GenerateToken(User user, IPAddress address) => GenerateToken(user, address.ToString());

    /// <summary>
    /// Retrieves a user object from the given token.
    /// </summary>
    /// <param name="token">The encrypted token containing user information.</param>
    /// <returns>The user object extracted from the token.</returns>
    private User GetUserFromToken(string token)
    {
        JObject json = JObject.Parse(new Crypt().Decrypt(token));
        if (json.TryGetValue("Id", out var id) && json.TryGetValue("Username", out var username) && json.TryGetValue("Password", out var password))
            return new User()
            {
                Id = id.ToString(),
                Username = username.ToString(),
                Password = password.ToString()
            };
        return User.Empty;
    }

    public void Dispose()
    {
        _connection.Close();
        _connection.Dispose();
    }
}