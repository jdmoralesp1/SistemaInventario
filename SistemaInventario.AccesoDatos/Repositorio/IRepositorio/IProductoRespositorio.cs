using SistemaInventario.Modelos;
using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaInventario.AccesoDatos.Repositorio.IRepositorio
{
    public interface IProductoRespositorio: IRepositorio<Producto>
    {
        void Actualizar(Producto producto);
    }
}
