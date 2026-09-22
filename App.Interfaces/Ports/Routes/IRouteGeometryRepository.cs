using App.Domain.Routes.Entities;

namespace App.Interfaces.Ports.Routes;

public interface IRouteGeometryRepository
{
    /// <summary>
    /// Obtiene todos los caminos activos (red vial) del módulo Routes, con los
    /// que el router calcula el trayecto sobre la red. Pertenece al mismo módulo
    /// de dominio, por lo que se resuelve internamente sin cruzar otro módulo.
    /// </summary>
    Task<List<TMineRoad>> GetAllActiveRoadsAsync(CancellationToken cancellationToken = default);
}