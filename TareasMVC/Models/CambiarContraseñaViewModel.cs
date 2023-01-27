using System.ComponentModel.DataAnnotations;

namespace TareasMVC.Models
{
    public class CambiarContraseñaViewModel
    {
        [DataType(DataType.Password)]
        [Required(ErrorMessage ="Campo requerido")]
        [Display(Name = "Contraseña Antigua")]
        public string PasswordVieja { get; set; }
        [DataType(DataType.Password)]
        [Required(ErrorMessage ="Campo requerido")]
        [Display(Name = "Contraseña Nueva")]
        public string PasswordNueva { get; set; }
        [DataType(DataType.Password)]
        [Required(ErrorMessage ="Campo requerido")]
        [Display(Name ="Confirmar Contraseña Nueva")]
        public string PasswordNuevaConfirmar { get; set; }
    }
}
