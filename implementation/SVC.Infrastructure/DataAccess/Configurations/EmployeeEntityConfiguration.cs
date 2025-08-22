using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SVC.Core.Entities;

namespace SVC.Infrastructure.DataAccess.Configurations;

public class EmployeeEntityConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");

        builder.HasKey(e => e.Id)
            .HasName("PK_Employee");

        builder.Property(e => e.Id)
            .HasColumnName("ID")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(e => e.Name);

        builder.Property(e => e.EntryDate)
            .HasConversion(
                v => v.ToUniversalTime(),
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
            );

        builder.Property(e => e.EntryDate)
            .HasConversion(
                v => v.ToUniversalTime(),
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
            );
        
        builder.HasOne(e => e.Platoon)
            .WithMany()
            .HasForeignKey(e => e.PlatoonId)
            .HasConstraintName("FK_Employee_Platoon")
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.Role)
            .WithMany()
            .HasForeignKey(e => e.RoleId)
            .HasConstraintName("FK_Employee_Role")
            .OnDelete(DeleteBehavior.SetNull);
    }
}