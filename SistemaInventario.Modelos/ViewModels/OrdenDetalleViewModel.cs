﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaInventario.Modelos.ViewModels
{
    public class OrdenDetalleViewModel
    {
        public Compañia Compañia { get; set; }
        public Orden Orden { get; set; }
        public IEnumerable<OrdenDetalle> OrdenDetalleLista { get; set; }
    }
}
