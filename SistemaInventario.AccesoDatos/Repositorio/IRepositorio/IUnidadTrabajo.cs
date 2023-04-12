using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaInventario.AccesoDatos.Repositorio.IRepositorio
{
    public interface IUnidadTrabajo :IDisposable
    {
        IBodegaRepositorio Bodega { get; }
        ICategoriaRepositorio Categoria { get; }
        IMarcaRepositorio Marca { get; }
        IProductoRespositorio Producto { get; }
        IUsuarioAplicacionRepositorio UsuarioAplicacion { get; }
        ICompañiaRepositorio Compañia { get; }
        ICarroComprasRepositorio CarroCompras { get; }
        IOrdenRepositorio Orden { get; }
        IOrdenDetalleRepositorio OrdenDetalle { get; }

        void Guardar();
    }
}
