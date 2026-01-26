using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentCollection.Domain.Entities;

namespace RentCollection.Infrastructure.Data.Configurations;

public class UserPropertyAssignmentConfiguration : IEntityTypeConfiguration<UserPropertyAssignment>
{
    public void Configure(EntityTypeBuilder<UserPropertyAssignment> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.AssignmentRole)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.HasOne(x => x.User)
            .WithMany(u => u.PropertyAssignments)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Property)
            .WithMany()
            .HasForeignKey(x => x.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.UserId, x.PropertyId, x.AssignmentRole })
            .IsUnique();

        builder.HasIndex(x => x.IsActive);
    }
}
