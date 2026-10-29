using Abstracciones.Agentes;
using Abstracciones.Comunes;
using Abstracciones.Excepciones;
using Abstracciones.Tickets;

namespace Flujo.Agentes;

public class AgenteFlujo : IAgenteFlujo
{
    private readonly IAgenteDA _da;
    private readonly ITicketDA _ticketDA;
    private readonly TimeProvider _reloj;

    public AgenteFlujo(IAgenteDA da, ITicketDA ticketDA, TimeProvider reloj)
    {
        _da = da;
        _ticketDA = ticketDA;
        _reloj = reloj;
    }

    public async Task<ResultadoPaginado<AgenteDto>> ListarAsync(AgenteFiltro filtro, CancellationToken ct)
    {
        var pagina = await _da.BuscarAsync(filtro, ct);
        var activos = await _ticketDA.ContarActivosPorAgenteAsync(pagina.Items.Select(a => a.Id).ToList(), ct);

        return new ResultadoPaginado<AgenteDto>
        {
            Items = pagina.Items.Select(a => AgenteMapeador.ADto(a, activos.GetValueOrDefault(a.Id))).ToList(),
            Total = pagina.Total,
            Pagina = pagina.Pagina,
            Tamano = pagina.Tamano
        };
    }

    public async Task<AgenteDto> ObtenerAsync(int id, CancellationToken ct)
    {
        var agente = await _da.ObtenerPorIdAsync(id, ct) ?? throw new NoEncontradoException("Agente", id);
        var activos = await _ticketDA.ContarActivosDeAgenteAsync(id, ct);
        return AgenteMapeador.ADto(agente, activos);
    }

    public async Task<AgenteDto> CrearAsync(AgenteCrearDto dto, CancellationToken ct)
    {
        var correo = NormalizarCorreo(dto.Correo);

        if (await _da.ObtenerPorCorreoAsync(correo, ct) is not null)
            throw new ConflictoException($"Ya existe un agente con el correo '{correo}'.");

        var agente = new Agente
        {
            Nombre = dto.Nombre.Trim(),
            Correo = correo,
            Telefono = dto.Telefono.Trim(),
            CapacidadMaxima = dto.CapacidadMaxima,
            Activo = dto.Activo,
            FechaIngreso = _reloj.GetUtcNow().UtcDateTime
        };

        await _da.AgregarAsync(agente, ct);
        return AgenteMapeador.ADto(agente, ticketsActivos: 0);
    }

    public async Task ActualizarAsync(int id, AgenteActualizarDto dto, CancellationToken ct)
    {
        var correo = NormalizarCorreo(dto.Correo);

        _ = await _da.ObtenerPorIdAsync(id, ct) ?? throw new NoEncontradoException("Agente", id);

        var otro = await _da.ObtenerPorCorreoAsync(correo, ct);
        if (otro is not null && otro.Id != id)
            throw new ConflictoException($"Ya existe otro agente con el correo '{correo}'.");

        var activos = await _ticketDA.ContarActivosDeAgenteAsync(id, ct);
        if (dto.CapacidadMaxima < activos)
            throw new ConflictoException(
                $"El agente tiene {activos} tickets activos; la capacidad máxima no puede ser menor a esa cantidad.");

        var actualizado = await _da.ActualizarAsync(new Agente
        {
            Id = id,
            Nombre = dto.Nombre.Trim(),
            Correo = correo,
            Telefono = dto.Telefono.Trim(),
            CapacidadMaxima = dto.CapacidadMaxima,
            Activo = dto.Activo
        }, ct);

        if (!actualizado) throw new NoEncontradoException("Agente", id);
    }

    public async Task EliminarAsync(int id, CancellationToken ct)
    {
        _ = await _da.ObtenerPorIdAsync(id, ct) ?? throw new NoEncontradoException("Agente", id);

        if (await _ticketDA.AgenteTieneTicketsAsync(id, ct))
            throw new ConflictoException("El agente tiene tickets asociados y no se puede eliminar. Puede desactivarlo en su lugar.");

        if (!await _da.EliminarAsync(id, ct))
            throw new NoEncontradoException("Agente", id);
    }

    private static string NormalizarCorreo(string correo) => correo.Trim().ToLowerInvariant();
}