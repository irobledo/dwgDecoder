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
        public Handle handleId;

        public String nombreCapa;
        public String descripcionCapa;

        public Color colorCapa;
        public int color_R;
        public int color_G;
        public int color_B;

        public bool enUso;
        public bool bloqueada;
        public bool apagada;
        public bool oculta;

        public LineWeight default_gruesoLinea;

        public override String ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("CAPA " + this.objectId);
            sb.AppendLine("\tnombreCapa: " + this.nombreCapa);
            
            return sb.ToString();
        }
    }
}
