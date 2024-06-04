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
    public string Id { get; internal set; }

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

    /// <summary>
    /// Represents an empty User object.
    /// </summary>
    /// <remarks>
    /// The Empty property is a static property that represents an empty User object. It can be used to indicate the absence of a valid User instance.
    /// </remarks>
    /// <value>
    /// An empty User object.
    /// </value>
    public static User Empty => new User();

    /// <summary>
    /// Gets a value indicating whether the User is empty or not.
    /// </summary>
    /// <value>
    /// <c>true</c> if the User is empty; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// The IsEmpty property is a boolean value that indicates whether the User object is empty or not.
    /// A User is considered empty when the Id property is null or an empty string.
    /// </remarks>
    public bool IsEmpty => string.IsNullOrEmpty(Id);
}