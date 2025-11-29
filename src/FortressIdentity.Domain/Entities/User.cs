using FortressIdentity.Domain.Exceptions;

namespace FortressIdentity.Domain.Entities;

/// <summary>
/// Represents a User in the domain.
/// This is a Rich Domain Model with encapsulation, validation, and behavior.
/// </summary>
public class User : BaseEntity
{
    /// <summary>
    /// User's first name.
    /// </summary>
    public string FirstName { get; private set; }

    /// <summary>
    /// User's last name.
    /// </summary>
    public string LastName { get; private set; }

    /// <summary>
    /// User's email address (unique identifier for authentication).
    /// </summary>
    public string Email { get; private set; }

    /// <summary>
    /// Hashed password using Argon2.
    /// </summary>
    public string PasswordHash { get; private set; }

    /// <summary>
    /// List of roles assigned to the user.
    /// </summary>
    public List<string> Roles { get; private set; }

    /// <summary>
    /// Indicates whether the user account is active.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Full name of the user (computed property).
    /// </summary>
    public string FullName => $"{FirstName} {LastName}";

    /// <summary>
    /// Private constructor for ORM/reconstitution.
    /// </summary>
    private User() : base()
    {
        Roles = new List<string>();
    }

    /// <summary>
    /// Creates a new User with validation.
    /// </summary>
    /// <param name="firstName">User's first name</param>
    /// <param name="lastName">User's last name</param>
    /// <param name="email">User's email address</param>
    /// <param name="passwordHash">Hashed password</param>
    /// <param name="roles">Optional list of roles</param>
    public User(string firstName, string lastName, string email, string passwordHash, List<string>? roles = null)
        : base()
    {
        ValidateAndSetFirstName(firstName);
        ValidateAndSetLastName(lastName);
        ValidateAndSetEmail(email);
        ValidateAndSetPasswordHash(passwordHash);

        Roles = roles ?? new List<string>();
        IsActive = true;
    }

    /// <summary>
    /// Updates the user's profile information.
    /// </summary>
    /// <param name="firstName">New first name</param>
    /// <param name="lastName">New last name</param>
    /// <param name="email">New email address</param>
    public void UpdateProfile(string firstName, string lastName, string email)
    {
        ValidateAndSetFirstName(firstName);
        ValidateAndSetLastName(lastName);
        ValidateAndSetEmail(email);

        UpdateTimestamp();
    }

    /// <summary>
    /// Changes the user's password.
    /// </summary>
    /// <param name="newPasswordHash">New hashed password</param>
    public void ChangePassword(string newPasswordHash)
    {
        ValidateAndSetPasswordHash(newPasswordHash);
        UpdateTimestamp();
    }

    /// <summary>
    /// Deactivates the user account.
    /// </summary>
    public void Deactivate()
    {
        if (!IsActive)
        {
            throw new DomainException("User is already deactivated.");
        }

        IsActive = false;
        UpdateTimestamp();
    }

    /// <summary>
    /// Activates the user account.
    /// </summary>
    public void Activate()
    {
        if (IsActive)
        {
            throw new DomainException("User is already active.");
        }

        IsActive = true;
        UpdateTimestamp();
    }

    /// <summary>
    /// Adds a role to the user.
    /// </summary>
    /// <param name="role">Role to add</param>
    public void AddRole(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            throw new InvalidEntityException(nameof(User), "Role cannot be null or empty.");
        }

        if (Roles.Contains(role))
        {
            throw new DomainException($"User already has the role '{role}'.");
        }

        Roles.Add(role);
        UpdateTimestamp();
    }

    /// <summary>
    /// Removes a role from the user.
    /// </summary>
    /// <param name="role">Role to remove</param>
    public void RemoveRole(string role)
    {
        if (!Roles.Contains(role))
        {
            throw new DomainException($"User does not have the role '{role}'.");
        }

        Roles.Remove(role);
        UpdateTimestamp();
    }

    #region Private Validation Methods

    private void ValidateAndSetFirstName(string firstName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            throw new InvalidEntityException(nameof(User), "FirstName cannot be null or empty.");
        }

        if (firstName.Length > 100)
        {
            throw new InvalidEntityException(nameof(User), "FirstName cannot exceed 100 characters.");
        }

        FirstName = firstName.Trim();
    }

    private void ValidateAndSetLastName(string lastName)
    {
        if (string.IsNullOrWhiteSpace(lastName))
        {
            throw new InvalidEntityException(nameof(User), "LastName cannot be null or empty.");
        }

        if (lastName.Length > 100)
        {
            throw new InvalidEntityException(nameof(User), "LastName cannot exceed 100 characters.");
        }

        LastName = lastName.Trim();
    }

    private void ValidateAndSetEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new InvalidEntityException(nameof(User), "Email cannot be null or empty.");
        }

        // Basic email format validation
        if (!email.Contains('@') || email.Length < 5)
        {
            throw new InvalidEntityException(nameof(User), "Email format is invalid.");
        }

        if (email.Length > 255)
        {
            throw new InvalidEntityException(nameof(User), "Email cannot exceed 255 characters.");
        }

        Email = email.Trim().ToLowerInvariant();
    }

    private void ValidateAndSetPasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new InvalidEntityException(nameof(User), "PasswordHash cannot be null or empty.");
        }

        PasswordHash = passwordHash;
    }

    #endregion
}
