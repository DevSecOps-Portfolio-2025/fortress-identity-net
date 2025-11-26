using FortressIdentity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace FortressIdentity.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the User entity.
/// Uses Fluent API to configure table structure, constraints, and conversions.
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Table name
        builder.ToTable("Users");

        // Primary Key
        builder.HasKey(u => u.Id);

        // Id configuration
        builder.Property(u => u.Id)
            .IsRequired()
            .ValueGeneratedNever(); // Guid is generated in the domain entity

        // FirstName configuration
        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        // LastName configuration
        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100);

        // Email configuration with unique index
        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("IX_Users_Email");

        // PasswordHash configuration
        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);

        // IsActive configuration
        builder.Property(u => u.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // CreatedAt configuration
        builder.Property(u => u.CreatedAt)
            .IsRequired();

        // UpdatedAt configuration
        builder.Property(u => u.UpdatedAt)
            .IsRequired();

        // Roles configuration - Store as JSON string
        builder.Property(u => u.Roles)
            .IsRequired()
            .HasMaxLength(1000)
            .HasConversion(
                roles => JsonSerializer.Serialize(roles, (JsonSerializerOptions?)null),
                json => JsonSerializer.Deserialize<List<string>>(json, (JsonSerializerOptions?)null) ?? new List<string>()
            );

        // Ignore computed properties (not stored in database)
        builder.Ignore(u => u.FullName);

        // Add index on IsActive for performance on queries filtering active users
        builder.HasIndex(u => u.IsActive)
            .HasDatabaseName("IX_Users_IsActive");

        // Add composite index for common query patterns
        builder.HasIndex(u => new { u.Email, u.IsActive })
            .HasDatabaseName("IX_Users_Email_IsActive");
    }
}
