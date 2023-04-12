using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SistemaInventario.AccesoDatos.Repositorio.IRepositorio;
using SistemaInventario.Modelos;
using SistemaInventario.Modelos.ViewModels;
using SistemaInventario.Utilidades;

namespace SistemaInventario.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = DS.Role_Admin)]
    public class CompañiaController : Controller
    {
        private readonly IUnidadTrabajo _unidadTrabajo;
        private readonly IWebHostEnvironment _hostEnvironment;

        public CompañiaController(IUnidadTrabajo unidadTrabajo, IWebHostEnvironment hostEnvironment)
        {
            _hostEnvironment= hostEnvironment;
            _unidadTrabajo = unidadTrabajo;
        }


        public IActionResult Index()
        {
            var compañia = _unidadTrabajo.Compañia.ObtenerTodos();
            return View(compañia);
        }

        public IActionResult Upsert(int? id)
        {
            CompañiaVM compañiaVM = new CompañiaVM()
            {
                Compañia = new Compañia(),
                BodegaLista = _unidadTrabajo.Bodega.ObtenerTodos().Select(c => new SelectListItem
                {
                    Text = c.Nombre,
                    Value = c.Id.ToString()
                })
            };
            if (id == null)
            {
                // Esto es para Crear nuevo Registro
                return View(compañiaVM);
            }
            // Esto es para Actualizar
            compañiaVM.Compañia = _unidadTrabajo.Compañia.Obtener(id.GetValueOrDefault());
            if (compañiaVM.Compañia == null)
            {
                return NotFound();
            }

            return View(compañiaVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(CompañiaVM compañiaVM)
        {
            if (ModelState.IsValid)
            {
                // Cargar Imagenes
                string webRootPath = _hostEnvironment.WebRootPath;
                var files = HttpContext.Request.Form.Files;
                if (files.Count > 0)
                {
                    string filename = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(webRootPath, @"imagenes\compania");
                    var extension = Path.GetExtension(files[0].FileName);
                    if (compañiaVM.Compañia.LogoUrl != null)
                    {
                        //Esto es para editar, necesitamos borrar la imagen anterior
                        var imagenPath = Path.Combine(webRootPath, compañiaVM.Compañia.LogoUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(imagenPath))
                        {
                            System.IO.File.Delete(imagenPath);
                        }
                    }

                    using (var filesStreams = new FileStream(Path.Combine(uploads, filename + extension), FileMode.Create))
                    {
                        files[0].CopyTo(filesStreams);
                    }

                    compañiaVM.Compañia.LogoUrl = @"\imagenes\compania\" + filename + extension;
                }
                else
                {
                    //Si en el Update el usuario no cambia la imagen
                    if (compañiaVM.Compañia.Id != 0)
                    {
                        Compañia compañiaDB = _unidadTrabajo.Compañia.Obtener(compañiaVM.Compañia.Id);
                        compañiaVM.Compañia.LogoUrl = compañiaDB.LogoUrl;
                    }
                }


                if (compañiaVM.Compañia.Id == 0)
                {
                    _unidadTrabajo.Compañia.Agregar(compañiaVM.Compañia);
                }
                else
                {
                    _unidadTrabajo.Compañia.Actualizar(compañiaVM.Compañia);
                }
                _unidadTrabajo.Guardar();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                compañiaVM.BodegaLista = _unidadTrabajo.Bodega.ObtenerTodos().Select(c => new SelectListItem
                {
                    Text = c.Nombre,
                    Value = c.Id.ToString()
                });

                if (compañiaVM.Compañia.Id != 0)
                {
                    compañiaVM.Compañia = _unidadTrabajo.Compañia.Obtener(compañiaVM.Compañia.Id);
                }
            }
            return View(compañiaVM.Compañia);
        }




        #region API
        [HttpGet]
        public IActionResult ObtenerTodos()
        {
            var todos = _unidadTrabajo.Compañia.ObtenerTodos(incluirPropiedades: "Bodega");
            return Json(new { data = todos });
        }

        #endregion
    }
}