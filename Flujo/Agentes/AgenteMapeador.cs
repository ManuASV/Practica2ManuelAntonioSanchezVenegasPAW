using Abstracciones.Agentes;

namespace Flujo.Agentes;

internal static class AgenteMapeador
{
    public static AgenteDto ADto(Agente a, int ticketsActivos) =>
        new(a.Id, a.Nombre, a.Correo, a.Telefono, a.CapacidadMaxima, a.Activo, a.FechaIngreso, ticketsActivos);
}