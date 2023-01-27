using Microsoft.Extensions.Configuration.UserSecrets;
using TareasMVC.Models;

namespace TareasMVC.Servicios
{
    public class AlmacenadorArchivosLocal : IAlmacenadorArchivos
    {
        private readonly IWebHostEnvironment env;
        private readonly HttpContextAccessor httpContext;

        public AlmacenadorArchivosLocal(IWebHostEnvironment env, HttpContextAccessor httpContext)
        {
            this.env = env;
            this.httpContext = httpContext;
        }
        public async Task<AlmacenarArchivoResultado[]> Almacenar(string contenedor, IEnumerable<IFormFile> archivos)
        {
            var tareas = archivos.Select(async archivo =>
            {
                var NombreArchivoOriginal = Path.GetFileName(archivo.FileName);
                var extension = Path.GetExtension(archivo.FileName);
                var nombreArchivo = $"{Guid.NewGuid()}{extension}";
                string folder = Path.Combine(env.WebRootPath, contenedor);

                if(!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                string ruta = Path.Combine(folder, nombreArchivo);
                using (var ms = new MemoryStream())
                {
                    await archivo.CopyToAsync(ms);
                    var contenido = ms.ToArray();
                    await File.WriteAllBytesAsync(ruta, contenido);
                }

                var url = $"{httpContext.HttpContext.Request.Scheme}://{httpContext.HttpContext.Request.Host}";
                var urlArchivo = Path.Combine(url, contenedor, nombreArchivo).Replace("\\", "/");

                return new AlmacenarArchivoResultado
                {
                    URL = urlArchivo,
                    Titulo = NombreArchivoOriginal

                };
            });
            var resultados = await Task.WhenAll(tareas);
            return resultados;
        }

        public Task Borrar(string ruta, string contenedor)
        {
            if(string.IsNullOrWhiteSpace(ruta))
            {
                return Task.CompletedTask;
            }

            var NombreArchivo = Path.GetFileName(ruta);
            var directorio = Path.Combine(env.WebRootPath, contenedor, NombreArchivo) ;

            if(File.Exists(directorio))
            {
                File.Delete(directorio);
            }

            return Task.CompletedTask;
        }
    }
}
