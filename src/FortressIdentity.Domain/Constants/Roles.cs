namespace FortressIdentity.Domain.Constants;

/// <summary>
/// Defines the available roles in the system for Role-Based Access Control (RBAC).
/// </summary>
public static class Roles
{
    /// <summary>
    /// Administrator role with full system access.
    /// </summary>
    public const string Admin = "Admin";

    /// <summary>
    /// Standard user role with limited access.
    /// </summary>
    public const string User = "User";
}
