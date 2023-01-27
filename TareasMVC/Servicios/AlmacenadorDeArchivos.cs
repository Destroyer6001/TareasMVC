using TareasMVC.Models;

namespace TareasMVC.Servicios
{
    public class AlmacenadorDeArchivos : IAlmacenadorArchivos
    {
        public async Task<AlmacenarArchivoResultado[]> Almacenar(string contenedor, IEnumerable<IFormFile> archivos)
        {
            throw new NotImplementedException();
        }

        public Task Borrar(string ruta, string contenedor)
        {
            throw new NotImplementedException();
        }
    }
}
