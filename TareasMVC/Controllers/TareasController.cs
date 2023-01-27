using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TareasMVC.Entidades;
using TareasMVC.Models;
using TareasMVC.Servicios;

namespace TareasMVC.Controllers
{
    [Route("api/tareas")]
    public class TareasController:ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IServicioUsuarios usuarios;
        private readonly IMapper mapper1;
        public TareasController(ApplicationDbContext dbcontext, IServicioUsuarios servicioUsuarios, IMapper mapper)
        {
            _dbContext = dbcontext;
            usuarios = servicioUsuarios;
            mapper1 = mapper;
        }

        [HttpGet]
        public async Task<List<TareaDTO>> get()
        {
            var UsuarioId = usuarios.ObtenerUsuarioId();
            var tareas = await _dbContext.Tareas.Where(c => c.UsuarioCreacionId == UsuarioId)
                .Include(t => t.Pasos.OrderBy(p => p.Orden))
                .Include(t => t.ArchivoAdjuntos.OrderBy(a => a.Orden))
                .OrderBy(t => t.Orden)
                .ProjectTo<TareaDTO>(mapper1.ConfigurationProvider)
                .ToListAsync();
            return tareas;
        }

        [HttpPost]
        public async Task<ActionResult<Tarea>> Post([FromBody] string titulo)
        {
            var usuarioid = usuarios.ObtenerUsuarioId();
            var existenTareas = await _dbContext.Tareas.AnyAsync(t => t.UsuarioCreacionId == usuarioid);

            var ordenMayor = 0;
            if(existenTareas)
            {
                ordenMayor = await _dbContext.Tareas.Where(t => t.UsuarioCreacionId == usuarioid).Select(t => t.Orden).MaxAsync();
            }

            var Tarea = new Tarea
            {
                Titulo = titulo,
                UsuarioCreacionId = usuarioid,
                FechaCreacion = DateTime.UtcNow,
                Orden = ordenMayor + 1
            };

            _dbContext.Add(Tarea);
            await _dbContext.SaveChangesAsync();

            return Tarea; 
        }

        [HttpPost]
        public async Task<IActionResult> Ordenar([FromBody] int[] Ids)
        {
            var usuarioId = usuarios.ObtenerUsuarioId();
            var tareas = await _dbContext.Tareas.Where(t => t.UsuarioCreacionId == usuarioId).ToListAsync();

            var tareasId = tareas.Select(t => t.Id);

            var idsTareasNoPertenecenAlUsuario = Ids.Except(tareasId).ToList();

            if(idsTareasNoPertenecenAlUsuario.Any())
            {
                return Forbid();
            }

            var tareasDiccionario = tareas.ToDictionary(x => x.Id);

            for(int i = 0; i < Ids.Length; ++i)
            {
                var id = Ids[i];
                var tarea = tareasDiccionario[id];
                tarea.Orden = i + 1;
            }

            await _dbContext.SaveChangesAsync();
            return Ok();

        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Tarea>> Get(int id)
        {
            var obtenerUsuarioId = usuarios.ObtenerUsuarioId();

            var tarea = await _dbContext.Tareas.Where(t => t.UsuarioCreacionId == obtenerUsuarioId && t.Id == id).FirstOrDefaultAsync();

            if(tarea is null)
            {
                return NotFound();
            }

            return tarea;
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> EditarTarea(int id ,[FromBody] TareaEditarDTO tareaDTO)
        {
            var usuarioid = usuarios.ObtenerUsuarioId();
            var tarea = await _dbContext.Tareas.FirstOrDefaultAsync(t => t.Id == id && t.UsuarioCreacionId == usuarioid);

            if(tarea is null)
            {
                return NotFound();
            }

            tarea.Titulo = tareaDTO.Titulo;
            tarea.Descripcion = tareaDTO.Descripcion;

            await _dbContext.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete]
        public async Task<ActionResult> Delete(int id)
        {
            var usuarioId = usuarios.ObtenerUsuarioId();
            var tarea = await _dbContext.Tareas.Where(t => t.Id == id && t.UsuarioCreacionId ==usuarioId).FirstOrDefaultAsync();
            if(tarea is null)
            {
                return NotFound();
            }

            _dbContext.Remove(tarea);
            await _dbContext.SaveChangesAsync();
            return Ok();
            
        }
        
    }
}
