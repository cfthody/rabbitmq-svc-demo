using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SVC.Core.Entities;

namespace SVC.Infrastructure.DataAccess.Configurations;

public class RoleEntityConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(r => r.Id)
            .HasName("PK_Role");
        
        builder.Property(r => r.Id)
            .HasColumnName("ID")
            .ValueGeneratedOnAdd();
        
        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(100);

    }
}