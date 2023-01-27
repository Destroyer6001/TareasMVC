using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using TareasMVC.Entidades;
using TareasMVC.Models;
using TareasMVC.Servicios;

namespace TareasMVC.Controllers
{
    [Route("api/pasos")]
    public class PasosController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IServicioUsuarios usuarios;
        public PasosController(ApplicationDbContext dbContext, IServicioUsuarios servicio)
        {
            context= dbContext;
            usuarios = servicio;
        }

        [HttpPost("{tareaId: int}")]
        public async Task<ActionResult<Paso>> Post(int Tareaid, [FromBody] PasoCrearDTO paso)
        {
            var usuarioId = usuarios.ObtenerUsuarioId();
            var tarea = await context.Tareas.Where(t => t.Id == Tareaid).FirstOrDefaultAsync();

            if(tarea is null)
            {
                return NotFound();
            }

            if(tarea.UsuarioCreacionId != usuarioId)
            {
                return Forbid();
            }

            var  existenPasos = await context.Pasos.AnyAsync(p => p.TareaId == Tareaid);

            var ordenMayor = 0;
            if(existenPasos)
            {
                ordenMayor = await context.Pasos.Where(p => p.TareaId == Tareaid).Select(p => p.Orden).MaxAsync();
            }

            var Paso = new Paso()
            {
                TareaId = Tareaid,
                Orden = ordenMayor + 1,
                Descripcion = paso.Descripcion,
                Realizado = paso.Realizado
            };

            context.Add(Paso);
            await context.SaveChangesAsync();

            return Paso;
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> put(Guid Id, [FromBody] PasoCrearDTO paso)
        {
            var usuarioId = usuarios.ObtenerUsuarioId();

            var Paso = await context.Pasos.Include(p => p.Tarea).FirstOrDefaultAsync(p => p.Id == Id);

            if(Paso is null)
            {
                return NotFound();
            }

            if(Paso.Tarea.UsuarioCreacionId != usuarioId)
            {
                return Forbid();
            }

            Paso.Descripcion = paso.Descripcion;
            Paso.Realizado = paso.Realizado;

            await context.SaveChangesAsync();
            return Ok();
        }


        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete (Guid id)
        {
            var UsuarioId = usuarios.ObtenerUsuarioId();

            var Paso = await context.Pasos.Include(p => p.Tarea).Where(p => p.Id == id).FirstOrDefaultAsync();

            if(Paso is null) 
            {
                return NotFound();
            }

            if(Paso.Tarea.UsuarioCreacionId != UsuarioId)
            {
                return Forbid();
            }

            context.Remove(Paso);
            await context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Ordenar(int tareaId, [FromBody] Guid[] ids)
        {
            var usuarioId = usuarios.ObtenerUsuarioId();

            var tarea = await context.Tareas.Where(t => t.UsuarioCreacionId == usuarioId && t.Id == tareaId).FirstOrDefaultAsync();

            if(tarea is null) {
                return NotFound();
            }

            var pasos = await context.Pasos.Where(p => p.TareaId == tareaId).ToListAsync();

            var pasosIds = pasos.Select(p => p.Id);

            var idsPasosNoPertenceALaTarea = ids.Except(pasosIds).ToList();

            if(idsPasosNoPertenceALaTarea.Any())
            {
                return BadRequest("No todos los pasos estan presentes");
            }

            var pasosDiccionario = pasos.ToDictionary(p => p.Id);

            for(int i = 0; i< ids.Length; i++) 
            {
                var pasoId = ids[i];
                var paso = pasosDiccionario[pasoId];
                paso.Orden = i + 1;
            }

            await context.SaveChangesAsync();
            return Ok();
        }
    }
}
