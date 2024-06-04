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
    /// Gets a value indicating whether the User is empty.
    /// </summary>
    /// <remarks>
    /// The IsEmpty property returns true if all the properties of the User object (Id, Username, Email, and Password) have null or empty values. This indicates that the User object is empty and does not represent a valid user in the authentication system.
    /// </remarks>
    /// <value>
    /// <c>true</c> if the User is empty; otherwise, <c>false</c>.
    /// </value>
    public bool IsEmpty => string.IsNullOrWhiteSpace(Id) && string.IsNullOrWhiteSpace(Username) && string.IsNullOrWhiteSpace(Email) && string.IsNullOrWhiteSpace(Password);

    public override string ToString()
    {
        return string.IsNullOrWhiteSpace(Email) ? Username : $"{Username} ({Email})";
    }
}