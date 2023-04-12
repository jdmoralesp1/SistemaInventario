using SistemaInventario.AccesoDatos.Data;
using SistemaInventario.AccesoDatos.Repositorio.IRepositorio;
using SistemaInventario.Modelos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SistemaInventario.AccesoDatos.Repositorio
{
    public class CompañiaRepositorio : Repositorio<Compañia>, ICompañiaRepositorio
    {
        private readonly ApplicationDbContext _db;

        public CompañiaRepositorio(ApplicationDbContext db): base(db)
        {
            _db = db;
        }

        public void Actualizar(Compañia compañia)
        {
            var companiaDb = _db.Compañia.FirstOrDefault(c => c.Id == compañia.Id);
            if (companiaDb != null)
            {
                if (compañia.LogoUrl != null)
                {
                    companiaDb.LogoUrl = compañia.LogoUrl;
                }
                companiaDb.Nombre = compañia.Nombre;
                companiaDb.Descripcion = compañia.Descripcion;
                companiaDb.Direccion = compañia.Direccion;
                companiaDb.BodegaVentaId = compañia.BodegaVentaId;
                companiaDb.Pais = compañia.Pais;
                companiaDb.Ciudad = compañia.Ciudad;
                companiaDb.Telefono = compañia.Telefono;
            }
        }
    }
}
