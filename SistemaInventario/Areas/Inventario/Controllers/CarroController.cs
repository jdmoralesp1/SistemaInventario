using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using SistemaInventario.AccesoDatos.Repositorio.IRepositorio;
using SistemaInventario.Modelos;
using SistemaInventario.Modelos.ViewModels;
using SistemaInventario.Utilidades;
using Stripe;
using System;
using System.Linq;
using System.Security.Claims;

namespace SistemaInventario.Areas.Inventario.Controllers
{
    [Area("Inventario")]
    public class CarroController : Controller
    {
        private readonly IUnidadTrabajo _unidadTrabajo;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<IdentityUser> _userManager;

        [BindProperty]
        public CarroComprasViewModel CarroComprasVM { get; set; }

        public CarroController(IUnidadTrabajo unidadTrabajo, IEmailSender emailSender, UserManager<IdentityUser> userManager)
        {
            _unidadTrabajo = unidadTrabajo;
            _emailSender = emailSender;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var claimIdentidad = (ClaimsIdentity)User.Identity;
            var claim = claimIdentidad.FindFirst(ClaimTypes.NameIdentifier);

            CarroComprasVM = new CarroComprasViewModel()
            {
                Orden = new Modelos.Orden(),
                CarroComprasLista = _unidadTrabajo.CarroCompras.ObtenerTodos(u => u.UsuarioAplicacionId == claim.Value, incluirPropiedades: "Producto")
            };

            CarroComprasVM.Orden.TotalOrden = 0;
            CarroComprasVM.Orden.UsuarioAplicacion = _unidadTrabajo.UsuarioAplicacion.ObtenerPrimero(u => u.Id == claim.Value);

            foreach (var lista in CarroComprasVM.CarroComprasLista)
            {
                lista.Precio = lista.Producto.Precio;
                CarroComprasVM.Orden.TotalOrden += (lista.Precio * lista.Cantidad);
            }

            return View(CarroComprasVM);
        }

        public IActionResult mas(int carroId)
        {
            var carroCompras = _unidadTrabajo.CarroCompras.ObtenerPrimero(c => c.Id == carroId, incluirPropiedades: "Producto");
            carroCompras.Cantidad += 1;
            _unidadTrabajo.Guardar();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult menos(int carroId)
        {
            var carroCompras = _unidadTrabajo.CarroCompras.ObtenerPrimero(c => c.Id == carroId, incluirPropiedades: "Producto");
            if (carroCompras.Cantidad==1)
            {
                var numeroProductos = _unidadTrabajo.CarroCompras.ObtenerTodos(u => u.UsuarioAplicacionId == carroCompras.UsuarioAplicacionId).ToList().Count();
                _unidadTrabajo.CarroCompras.Remover(carroCompras);
                _unidadTrabajo.Guardar();
                HttpContext.Session.SetInt32(DS.ssCarroCompras, numeroProductos - 1);
            }
            else
            {
                carroCompras.Cantidad -= 1;
                _unidadTrabajo.Guardar();
            }
            
            return RedirectToAction(nameof(Index));
        }

        public IActionResult remover(int carroId)
        {
            var carroCompras = _unidadTrabajo.CarroCompras.ObtenerPrimero(c => c.Id == carroId, incluirPropiedades: "Producto");
            
            var numeroProductos = _unidadTrabajo.CarroCompras.ObtenerTodos(u => u.UsuarioAplicacionId == carroCompras.UsuarioAplicacionId).ToList().Count();
            _unidadTrabajo.CarroCompras.Remover(carroCompras);
            _unidadTrabajo.Guardar();
            HttpContext.Session.SetInt32(DS.ssCarroCompras, numeroProductos - 1);
            

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Proceder()
        {
            var claimIdentidad = (ClaimsIdentity)User.Identity;
            var claim = claimIdentidad.FindFirst(ClaimTypes.NameIdentifier);

            CarroComprasVM = new CarroComprasViewModel()
            {
                Orden = new Modelos.Orden(),
                CarroComprasLista = _unidadTrabajo.CarroCompras.ObtenerTodos(u => u.UsuarioAplicacionId == claim.Value, incluirPropiedades: "Producto")
            };

            CarroComprasVM.Orden.TotalOrden = 0;
            CarroComprasVM.Orden.UsuarioAplicacion = _unidadTrabajo.UsuarioAplicacion.ObtenerPrimero(u => u.Id == claim.Value);

            foreach (var lista in CarroComprasVM.CarroComprasLista)
            {
                lista.Precio = lista.Producto.Precio;
                CarroComprasVM.Orden.TotalOrden += (lista.Precio * lista.Cantidad);
            }

            CarroComprasVM.Orden.NombresCliente = CarroComprasVM.Orden.UsuarioAplicacion.Nombres + " " + CarroComprasVM.Orden.UsuarioAplicacion.Apellidos;
            CarroComprasVM.Orden.Telefono = CarroComprasVM.Orden.UsuarioAplicacion.PhoneNumber;
            CarroComprasVM.Orden.Direccion = CarroComprasVM.Orden.UsuarioAplicacion.Direccion;
            CarroComprasVM.Orden.Pais = CarroComprasVM.Orden.UsuarioAplicacion.Pais;
            CarroComprasVM.Orden.Ciudad = CarroComprasVM.Orden.UsuarioAplicacion.Ciudad;

            return View(CarroComprasVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Proceder")]
        public IActionResult ProcederPost(string stripeToken)
        {
            var claimIdentidad = (ClaimsIdentity)User.Identity;
            var claim = claimIdentidad.FindFirst(ClaimTypes.NameIdentifier);
            CarroComprasVM.Orden.UsuarioAplicacion = _unidadTrabajo.UsuarioAplicacion.ObtenerPrimero(c => c.Id == claim.Value);
            CarroComprasVM.CarroComprasLista = _unidadTrabajo.CarroCompras.ObtenerTodos(c => c.UsuarioAplicacionId == claim.Value, incluirPropiedades: "Producto");
            CarroComprasVM.Orden.EstadoOrden = DS.EstadoPendiente;
            CarroComprasVM.Orden.EstadoPago = DS.PagoEstadoPendiente;
            CarroComprasVM.Orden.UsuarioAplicacionId = claim.Value;
            CarroComprasVM.Orden.FechaOrden = DateTime.Now;

            _unidadTrabajo.Orden.Agregar(CarroComprasVM.Orden);
            _unidadTrabajo.Guardar();

            foreach (var item in CarroComprasVM.CarroComprasLista)
            {
                OrdenDetalle ordenDetalle = new OrdenDetalle()
                {
                    ProductoId=item.ProductoId,
                    OrdenId = CarroComprasVM.Orden.Id,
                    Precio = item.Producto.Precio,
                    Cantidad = item.Cantidad
                };
                CarroComprasVM.Orden.TotalOrden += ordenDetalle.Cantidad * ordenDetalle.Precio;
                _unidadTrabajo.OrdenDetalle.Agregar(ordenDetalle);
            }
            // Remover los productos del carro de Compras
            _unidadTrabajo.CarroCompras.RemoverRango(CarroComprasVM.CarroComprasLista);
            _unidadTrabajo.Guardar();
            HttpContext.Session.SetInt32(DS.ssCarroCompras, 0);

            if (stripeToken == null)
            {

            }
            else
            {
                //Procesar el pago
                var options = new ChargeCreateOptions()
                {
                    Amount = Convert.ToInt32(CarroComprasVM.Orden.TotalOrden * 100),
                    Currency = "usd",
                    Description = "Numero de Orden: "+CarroComprasVM.Orden.Id,
                    Source = stripeToken
                };
                var service = new ChargeService();
                Charge charge = service.Create(options);
                if (charge.BalanceTransactionId == null)
                {
                    CarroComprasVM.Orden.EstadoPago = DS.EstadoRechazado;
                }
                else
                {
                    CarroComprasVM.Orden.TransaccionId = charge.Id;
                }

                if (charge.Status.ToLower() == "succeeded")
                {
                    CarroComprasVM.Orden.EstadoPago = DS.PagoEstadoAprobado;
                    CarroComprasVM.Orden.EstadoOrden = DS.EstadoAprobado;
                    CarroComprasVM.Orden.FechaPago = DateTime.Now;
                }
            }

            _unidadTrabajo.Guardar();

            return RedirectToAction("OrdenConfirmacion", "Carro", new { id = CarroComprasVM.Orden.Id });
        }

        public IActionResult OrdenConfirmacion(int id)
        {
            return View(id);
        }
    }
}
