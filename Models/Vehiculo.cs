using alquiler_vehiculos_g11.Models.Enums;

namespace alquiler_vehiculos_g11.Models;

public class Vehiculo
{
    public int Id { get; set; }

    public int CategoriaVehiculoId { get; set; }

    public CategoriaVehiculo? CategoriaVehiculo { get; set; }

    public string Patente { get; set; } = string.Empty;

    public string Marca { get; set; } = string.Empty;

    public string Modelo { get; set; } = string.Empty;

    public int KilometrajeActual { get; set; }

    public EstadoOperativoVehiculo EstadoOperativo { get; set; } = EstadoOperativoVehiculo.Habilitado;


    //-----Tablas pendientes para la siguiente etapa-------

    //public ICollection<Alquiler> Alquileres { get; set; } = new List<Alquiler>();

    //public ICollection<Imagen> Imagenes { get; set; } = new List<Imagen>();
}
