﻿using SistemaInventario.Modelos;
using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaInventario.AccesoDatos.Repositorio.IRepositorio
{
    public interface ICompañiaRepositorio: IRepositorio<Compañia>
    {
        void Actualizar(Compañia compañia);
    }
}
