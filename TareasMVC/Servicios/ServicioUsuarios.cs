using System.Security.Claims;

namespace TareasMVC.Servicios
{
    public class ServicioUsuarios : IServicioUsuarios
    {
        private HttpContext accessor;
        public ServicioUsuarios(IHttpContextAccessor httpContext)
        {
            accessor= httpContext.HttpContext;
        }
        public string ObtenerUsuarioId()
        {
            if(accessor.User.Identity.IsAuthenticated)
            {
                var idClaim = accessor.User.Claims.Where(x => x.Type == ClaimTypes.NameIdentifier).FirstOrDefault();

                return idClaim.Value;
            }
            else
            {
                throw new Exception("El usuario no esta autenticado");
            }

        }
    }
}
