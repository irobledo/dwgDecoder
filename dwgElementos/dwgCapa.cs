using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Colors;

namespace fi.upm.es.dwgDecoder.dwgElementos
{
    public class dwgCapa
    {
        public ObjectId objectId;
        public String nombreCapa;

        public Color colorCapa;

        public int color_R;
        public int color_G;
        public int color_B;
    }
}
