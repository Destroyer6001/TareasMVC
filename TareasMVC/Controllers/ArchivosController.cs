using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TareasMVC.Entidades;
using TareasMVC.Servicios;

namespace TareasMVC.Controllers
{
    [Route("api/archivos")]
    public class ArchivosController : ControllerBase
    {
        public readonly ApplicationDbContext context;
        public readonly IAlmacenadorArchivos Archivos;
        private readonly IServicioUsuarios usuarios;
        private readonly string Contenedor = "ArchivosAdjuntos";
        public ArchivosController(ApplicationDbContext _context, IAlmacenadorArchivos almacenadorArchivos, IServicioUsuarios servicioUsuarios)
        {
            context = _context;
            Archivos = almacenadorArchivos;
            usuarios = servicioUsuarios;
        }

        [HttpPost("{TareaId: int}")]
        public async Task<ActionResult<IEnumerable<ArchivoAdjunto>>> Post(int tareaId, [FromForm] IEnumerable<IFormFile> archivos)
        {
            var usuarioId = usuarios.ObtenerUsuarioId();

            var ExistenArchivosAdjuntos = await context.ArchivoAdjuntos.Where(a => a.TareaId == tareaId).AnyAsync();

            var tarea = await context.Tareas.Where(t => t.Id == tareaId).FirstOrDefaultAsync();

            if (tarea.UsuarioCreacionId != usuarioId)
            {
                return Forbid();
            }

            if (tarea is null)
            {
                return NotFound();
            }

            var ordenMayor = 0;

            if (ExistenArchivosAdjuntos)
            {
                ordenMayor = await context.ArchivoAdjuntos.Where(a => a.TareaId == tareaId).Select(a => a.Orden).MaxAsync();
            }

            var resultados = await Archivos.Almacenar(Contenedor, archivos);
            var archivosAdjuntos = resultados.Select((resultado, indice) => new ArchivoAdjunto
            {
                TareaId = tareaId,
                FechaCreacion = DateTime.UtcNow,
                Url = resultado.URL,
                Titulo = resultado.Titulo,
                Orden = ordenMayor + indice + 1
            }).ToList();

            context.AddRange(archivosAdjuntos);
            await context.SaveChangesAsync();
            return archivosAdjuntos.ToList();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid Id, [FromBody] string titulo)
        {
            var UsuarioId = usuarios.ObtenerUsuarioId();

            var archivoAdjunto = await context.ArchivoAdjuntos.Include(a => a.Tarea).Where(a => a.Id == Id).FirstOrDefaultAsync();

            if (archivoAdjunto is null)
            {
                return NotFound();
            }

            if (archivoAdjunto.Tarea.UsuarioCreacionId != UsuarioId)
            {
                return Forbid();
            }

            archivoAdjunto.Titulo = titulo;
            await context.SaveChangesAsync();
            return Ok();


        }

        [HttpDelete("{Id}")]
        public async Task<IActionResult> Delete (Guid Id)
        {
            var UsuarioId = usuarios.ObtenerUsuarioId();

            var archivos = await context.ArchivoAdjuntos.Include(a => a.Tarea).Where(a => a.Id == Id).FirstOrDefaultAsync();

            if(archivos is null)
            {
                return NotFound();
            }

            if(archivos.Tarea.UsuarioCreacionId != UsuarioId)
            {
                return Forbid();
            }

            context.Remove(archivos);
            await context.SaveChangesAsync();
            await Archivos.Borrar(archivos.Url, contenedor);
            return Ok();

        }

    }
}
