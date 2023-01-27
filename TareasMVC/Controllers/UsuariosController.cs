using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TareasMVC.Models;
using TareasMVC.Servicios;

namespace TareasMVC.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ApplicationDbContext _dbContext;
        private readonly IServicioUsuarios usuarios;
        public UsuariosController(UserManager<IdentityUser> user, SignInManager<IdentityUser> signIn, ApplicationDbContext dbContext, IServicioUsuarios servicioUsuarios )
        {
            _signInManager = signIn;
            _userManager = user;
            _dbContext = dbContext;
            usuarios = servicioUsuarios;

        }

        [AllowAnonymous]
        public IActionResult Registro()
        {
            return View();
        }


        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Registro(RegistroViewModel registro)
        {
            if (!ModelState.IsValid)
            {
                return View(registro);
            }

            var Usuario = new IdentityUser() { Email = registro.Email, UserName = registro.Email };
            var Resultado = await _userManager.CreateAsync(Usuario, password: registro.Password);



            if (Resultado.Succeeded)
            {
                await _signInManager.SignInAsync(Usuario, isPersistent: true);
                return RedirectToAction("Index", "Home");
            }
            else
            {
                foreach (var Error in Resultado.Errors)
                {
                    ModelState.AddModelError(string.Empty, Error.Description);
                }

                return View(registro);
            }
        }

        [AllowAnonymous]
        public IActionResult Login(string mensaje = null)
        {
            if (mensaje is not null)
            {
                ViewData["mensaje"] = mensaje;
            }
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel login)
        {
            if (!ModelState.IsValid)
            {
                return View(login);
            }

            var resultado = await _signInManager.PasswordSignInAsync(login.Email, login.Password,
                login.Recuerdame, lockoutOnFailure: false);

            if (resultado.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Nombre de usuario o contraseña incorrecta");
                return View(login);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        [HttpGet]
        public ChallengeResult LoginExterno(string Proveedor, string urlRetorno = null)
        {
            var urlRedireccion = Url.Action("RegistrarUsuariosExterno", values: new { urlRetorno });
            var propiedad = _signInManager.ConfigureExternalAuthenticationProperties(Proveedor, urlRedireccion);
            return new ChallengeResult(Proveedor, propiedad);
        }

        [AllowAnonymous]
        public async Task<IActionResult> RegistrarUsuariosExterno(string urlRetorno = null, string remoteError = null)
        {
            urlRetorno = urlRetorno ?? Url.Content("~/");
            var mensaje = "";

            if (remoteError is not null)
            {
                mensaje = $"Error del proveedor externo: {remoteError}";
                return RedirectToAction("Login", routeValues: new { mensaje });
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info is null)
            {
                mensaje = "Error cargando la dato de login externi";
                return RedirectToAction("Login", routeValues: new { mensaje });
            }

            var resultadologinexterno = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: true,
                bypassTwoFactor: true);

            //Ya la cuenta existe
            if (resultadologinexterno.Succeeded)
            {
                return LocalRedirect(urlRetorno);
            }

            string email = "";

            if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
            {
                email = info.Principal.FindFirstValue(ClaimTypes.Email);
            }
            else
            {
                mensaje = "Error leyendo el email del usuario del proveedor";
                return RedirectToAction("Login", routeValues: new { mensaje });
            }

            var usuario = new IdentityUser { Email = email, UserName = email };
            var resultadoCrearUsuario = await _userManager.CreateAsync(usuario);

            if (!resultadoCrearUsuario.Succeeded)
            {
                mensaje = resultadoCrearUsuario.Errors.First().Description;
                return RedirectToAction("Login", routeValues: new { mensaje });
            }

            var resultadoAgregarLogin = await _userManager.AddLoginAsync(usuario, info);

            if (resultadoAgregarLogin.Succeeded)
            {
                await _signInManager.SignInAsync(usuario, isPersistent: true, info.LoginProvider);
                return LocalRedirect(urlRetorno);
            }
            else
            {
                mensaje = "Ha ocurrido un error agregrando el login";
                return RedirectToAction("Login", routeValues: new { mensaje });
            }
        }

        [HttpGet]
        [Authorize(Roles = Constantes.RolAdmin)]
        public async Task<IActionResult> Listado(string mensaje = null)
        {
            var usuarios = await _dbContext.Users.Select(u => new UsuarioViewModel { Email = u.Email }).ToListAsync();
            var modelo = new UsuariosListadoViewModel();
            modelo.Usuarios1 = usuarios;
            modelo.Mensaje = mensaje;

            return View(modelo);

        }

        [HttpPost]
        [Authorize(Roles = Constantes.RolAdmin)]
        public async Task<IActionResult> HacerAdmin(string email)
        {
            var usuario = await _dbContext.Users.Where(u => u.Email == email).FirstOrDefaultAsync();

            if (usuario is null)
            {
                return NotFound();
            }

            await _userManager.AddToRoleAsync(usuario, Constantes.RolAdmin);
            return RedirectToAction("Listado", routeValues: new { mensaje = "Rol Asignado correctamente a " + email });

        }

        [HttpPost]
        [Authorize(Roles = Constantes.RolAdmin)]
        public async Task<IActionResult> RemoverAdmin(string email)
        {
            var usuario = await _dbContext.Users.Where(u => u.Email == email).FirstOrDefaultAsync();

            if(usuario is null)
            {
                return NotFound();
            }

            await _userManager.RemoveFromRoleAsync(usuario, Constantes.RolAdmin);
            return RedirectToAction("Listado", routeValues: new { mensaje = "Rol removido correctamente a " + email });
        }

        [HttpGet]
        public IActionResult CambiarContraseña()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CambiarContraseña(CambiarContraseñaViewModel cambiarContraseña)
        {
            if(!ModelState.IsValid )
            {
                return View(cambiarContraseña);
            }

            if(cambiarContraseña.PasswordNueva != cambiarContraseña.PasswordNuevaConfirmar)
            {
                ModelState.AddModelError(nameof(cambiarContraseña.PasswordNueva), $"la contraseña nueva no coincide con su verificacion");
                return View(cambiarContraseña);
            }

            if(cambiarContraseña.PasswordNueva == cambiarContraseña.PasswordVieja)
            {
                ModelState.AddModelError(nameof(cambiarContraseña.PasswordNueva), $"la contraseña nueva no puede ser igual a la contraseña vieja");
                return View(cambiarContraseña);
            }

            var UsuarioId = usuarios.ObtenerUsuarioId();

            var Usuario = _dbContext.Users.Where(u => u.Id == UsuarioId).FirstOrDefault();

            var resultado = await _userManager.ChangePasswordAsync(Usuario, cambiarContraseña.PasswordVieja, cambiarContraseña.PasswordNueva);

            if(resultado.Succeeded)
            {
                await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
                return RedirectToAction("Index", "Home");
            }
            else
            {
                foreach (var Error in resultado.Errors)
                {
                    ModelState.AddModelError(string.Empty, Error.Description);
                }

                return View(cambiarContraseña);
            }
        }
    }
}