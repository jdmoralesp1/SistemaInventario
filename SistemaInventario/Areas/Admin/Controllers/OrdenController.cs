using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaInventario.AccesoDatos.Repositorio.IRepositorio;
using SistemaInventario.Modelos;
using SistemaInventario.Modelos.ViewModels;
using SistemaInventario.Utilidades;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace SistemaInventario.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrdenController : Controller
    {
        private readonly IUnidadTrabajo _unidadTrabajo;
        [BindProperty]
        public OrdenDetalleViewModel OrdenDetalleVM { get; set; }

        public OrdenController(IUnidadTrabajo unidadTrabajo)
        {
            _unidadTrabajo = unidadTrabajo;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Detalle(int id)
        {
            OrdenDetalleVM = new OrdenDetalleViewModel()
            {
                Orden = _unidadTrabajo.Orden.ObtenerPrimero(o => o.Id == id, incluirPropiedades: "UsuarioAplicacion"),
                OrdenDetalleLista = _unidadTrabajo.OrdenDetalle.ObtenerTodos(o => o.OrdenId == id, incluirPropiedades: "Producto")
            };
            return View(OrdenDetalleVM);
        }

        #region

        [HttpGet]
        public IActionResult ObtenerOrdenLista(string estado)
        {
            var claimIdentidad = (ClaimsIdentity)User.Identity;
            var claim = claimIdentidad.FindFirst(ClaimTypes.NameIdentifier);
            IEnumerable<Orden> ordenlista;
            if (User.IsInRole(DS.Role_Admin) || User.IsInRole(DS.Role_Ventas))
            {
                ordenlista = _unidadTrabajo.Orden.ObtenerTodos(incluirPropiedades: "UsuarioAplicacion");
            }
            else
            {
                ordenlista = _unidadTrabajo.Orden.ObtenerTodos(o => o.UsuarioAplicacionId == claim.Value, incluirPropiedades: "UsuarioAplicacion");
            }

            switch (estado)
            {
                case "pendiente":
                    ordenlista = ordenlista.Where(o => o.EstadoPago == DS.PagoEstadoPendiente || o.EstadoPago == DS.PagoEstadoRestrasado);
                    break;

                case "aprobado":
                    ordenlista = ordenlista.Where(o => o.EstadoPago == DS.PagoEstadoAprobado);
                    break;

                case "completado":
                    ordenlista = ordenlista.Where(o => o.EstadoOrden == DS.EstadoEnviado);
                    break;

                case "rechazado":
                    ordenlista = ordenlista.Where(o => o.EstadoOrden == DS.EstadoRechazado || o.EstadoOrden == DS.EstadoCancelado);
                    break;

                default:
                    break;
            }

            return Json(new { data = ordenlista });
        }

        #endregion
    }
}
