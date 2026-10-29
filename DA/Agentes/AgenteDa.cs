using Abstracciones.Agentes;
using DA.Contexto;
using Microsoft.EntityFrameworkCore;

namespace DA.Agentes;

public class AgenteDa : IAgenteDa
{
    private readonly SistemaSoporteContexto _ctx;

    public AgenteDa(SistemaSoporteContexto ctx)
    {
        _ctx = ctx;
    }

    public async Task<AgenteDto> Crear(AgenteCrearDto agente)
    {
        var nuevoAgente = new Agente
        {
            Nombre = agente.Nombre,
            Correo = agente.Correo,
            Telefono = agente.Telefono,
            CantidadTickets = agente.CantidadTickets,
            Activo = true,
            FechaCreacion = DateTime.Now
        };

        _ctx.Agentes.Add(nuevoAgente);
        await _ctx.SaveChangesAsync();

        return ConvertirADto(nuevoAgente);
    }

    public async Task<bool> Actualizar(int id, AgenteActualizarDto agente)
    {
        var existente = await _ctx.Agentes
            .FirstOrDefaultAsync(a => a.AgenteId == id);

        if (existente is null)
            return false;

        existente.Nombre = agente.Nombre;
        existente.Correo = agente.Correo;
        existente.Telefono = agente.Telefono;
        existente.CantidadTickets = agente.CantidadTickets;

        await _ctx.SaveChangesAsync();

        return true;
    }

    public async Task<AgenteDto> ObtenerPorId(int id)
    {
        var agente = await _ctx.Agentes
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.AgenteId == id);

        if (agente is null)
            return null!;

        return ConvertirADto(agente);
    }

    public async Task<List<AgenteDto>> ObtenerTodos()
    {
        var agentes = await _ctx.Agentes
            .AsNoTracking()
            .OrderBy(a => a.Nombre)
            .ToListAsync();

        return agentes.Select(ConvertirADto).ToList();
    }

    public async Task<bool> Eliminar(int id)
    {
        var agente = await _ctx.Agentes
            .FirstOrDefaultAsync(a => a.AgenteId == id);

        if (agente is null)
            return false;

        _ctx.Agentes.Remove(agente);
        await _ctx.SaveChangesAsync();

        return true;
    }

    private static AgenteDto ConvertirADto(Agente agente)
    {
        return new AgenteDto(
            agente.Nombre,
            agente.Correo,
            agente.Telefono,
            agente.CantidadTickets
        );
    }
}