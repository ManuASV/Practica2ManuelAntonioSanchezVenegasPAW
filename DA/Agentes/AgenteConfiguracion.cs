using Abstracciones.Agentes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DA.Agentes;

public class AgenteConfiguracion : IEntityTypeConfiguration<Agente>
{
    public void Configure(EntityTypeBuilder<Agente> b)
    {
        b.ToTable("Agentes", t => t.HasCheckConstraint(
            "CK_Agentes_CapacidadMaxima",
            $"[CapacidadMaxima] BETWEEN 1 AND {ReglasAgente.CapacidadMaximaPermitida}"));

        b.HasKey(a => a.Id);

        b.Property(a => a.Nombre).IsRequired().HasMaxLength(100);
        b.Property(a => a.Correo).IsRequired().HasMaxLength(150);
        b.Property(a => a.Telefono).IsRequired().HasMaxLength(8);
        b.Property(a => a.CapacidadMaxima).IsRequired();
        b.Property(a => a.Activo).IsRequired();
        b.Property(a => a.FechaIngreso).HasDefaultValueSql("SYSUTCDATETIME()");

        b.HasIndex(a => a.Correo).IsUnique();
    }
}