﻿using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaInventario.Modelos.ViewModels
{
    public class CompañiaVM
    {
        public Compañia Compañia { get; set; }
        public IEnumerable<SelectListItem> BodegaLista { get; set; }
    }
}
