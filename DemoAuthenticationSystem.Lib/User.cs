namespace DemoAuthenticationSystem.Lib;

/// <summary>
/// Represents a user in the authentication system.
/// </summary>
public struct User()
{
    /// <summary>
    /// Gets or sets the Id of the User.
    /// </summary>
    /// <remarks>
    /// The Id property represents the unique identifier of a User in the system. It is an integer value that is automatically generated when a User is created.
    /// </remarks>
    public int Id { get; internal set; }

    /// <summary>
    /// Gets or sets the username of the user.
    /// </summary>
    /// <value>The username.</value>
    public string Username { get; set; }

    /// <summary>
    /// Represents the email address of a user.
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// Represents the password of a user.
    /// </summary>
    public string Password { get; set; }
}