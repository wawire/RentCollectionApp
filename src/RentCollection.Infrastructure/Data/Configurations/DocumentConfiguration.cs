using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentCollection.Domain.Entities;

namespace RentCollection.Infrastructure.Data.Configurations;

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.FileName)
            .HasMaxLength(255);

        builder.Property(d => d.FileUrl)
            .HasMaxLength(1000);

        builder.Property(d => d.ContentType)
            .HasMaxLength(100);

        builder.Property(d => d.Description)
            .HasMaxLength(1000);

        builder.HasOne(d => d.UploadedBy)
            .WithMany()
            .HasForeignKey(d => d.UploadedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.VerifiedBy)
            .WithMany()
            .HasForeignKey(d => d.VerifiedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
