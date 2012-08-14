using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.AutoCAD.Colors;

namespace fi.upm.es.dwgDecoder.dwgElementos
{
    public class dwgLinea : dwgEntidadBase
    {
        public dwgPunto p_origen;
        public dwgPunto p_final;

        public Color colorLinea;
        public int color_R;
        public int color_G;
        public int color_B;
    }
}
