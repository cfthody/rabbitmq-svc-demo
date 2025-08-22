using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SVC.Core.Entities;

namespace SVC.Infrastructure.DataAccess.Configurations;

public class PlatoonEntityConfiguration : IEntityTypeConfiguration<Platoon>
{
    public void Configure(EntityTypeBuilder<Platoon> builder)
    {
        builder.ToTable("Platoons");

        builder.HasKey(p => p.Id)
            .HasName("Pk_Platoon");
        
        builder.Property(p => p.Id)
            .HasColumnName("ID")
            .ValueGeneratedOnAdd();
        
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.HasIndex(p => p.Name).IsUnique();
    }
}