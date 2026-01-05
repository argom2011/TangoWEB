//# Panorama Técnico – HomeController
//* **Tecnología:** ASP.NET Core MVC + Razor Views
//* **Tipo de controlador:** `Controller` para renderizar **vistas estáticas o con datos mínimos**.
//* **Responsabilidades:**
//  * Redirige a las vistas principales de la aplicación:
//    * `Index` → Página principal / Dashboard
//    * `Privacy` → Página de política de privacidad
//    * `Clientes` → Vista para administrar clientes
//    * `Productos` → Vista para administrar productos
//    * `Ventas` → Vista para administrar ventas
//  * Manejo de errores con la acción `Error()` que devuelve la vista `Error.cshtml` con información del request.
//* **Acceso a datos / lógica de negocio:** Ninguno directamente; todo se realiza mediante **otros controladores o servicios**.
//* **Frontend:** Integración con **Bootstrap / Razor Pages** para mostrar datos o formularios de manera responsiva.
//* **Otros detalles:**
//  * Uso de `ILogger` para logging de errores o eventos generales
//  * Decorador `[ResponseCache]` en la acción `Error` para evitar caching de páginas de error
//En resumen: **controlador de navegación y renderizado de vistas**, funciona como puerta de entrada al **frontend MVC** sin manipular datos directamente, delegando toda lógica al backend (otros controladores / servicios / repositorios).

using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TangoWEB.Models;

namespace TangoWEB.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult Clientes()
        {
            return View();
        }

        public IActionResult Productos()
        {
            return View();
        }
        public IActionResult Ventas()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
