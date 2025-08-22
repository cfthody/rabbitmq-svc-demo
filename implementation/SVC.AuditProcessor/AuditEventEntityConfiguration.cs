using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SVC.AuditProcessor;

public class AuditEventEntityConfiguration: IEntityTypeConfiguration<AuditEvent>
{
    public void Configure(EntityTypeBuilder<AuditEvent> builder)
    {
        builder.ToTable("AuditEvents");
        
        builder.HasKey(a => a.Id)
            .HasName("PK_AuditEvent");
        
        builder.Property(e => e.Id)
            .HasColumnName("ID")
            .UseSerialColumn();
        
        builder.Property(a => a.Data)
            .HasColumnType("jsonb")
            .IsRequired();

    }
}