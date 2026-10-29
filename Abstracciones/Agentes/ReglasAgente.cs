using System.Linq.Expressions;
using Abstracciones.Tickets;

namespace Abstracciones.Agentes;

public static class ReglasAgente
{
    public const int CapacidadMaximaPermitida = 50;

    public static bool EstadoOcupaCapacidad(EstadoTicket estado) =>
        estado is EstadoTicket.Abierto or EstadoTicket.EnProceso;

    public static readonly Expression<Func<Ticket, bool>> TicketOcupaCapacidad = t =>
        t.Estado == EstadoTicket.Abierto || t.Estado == EstadoTicket.EnProceso;
}