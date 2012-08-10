using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.AutoCAD.Geometry;

namespace fi.upm.es.dwgDecoder.dwgElementos
{
    public class dwgPunto : dwgEntidadBase
    {
        public Point3d coordenadas;

        public override String ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("PUNTO " + this.objId);
            sb.AppendLine("\tCapaId " + this.capaId);
            sb.AppendLine("\tParentId " + this.parentId);

            sb.AppendLine("\t\tX: " + this.coordenadas.X);
            sb.AppendLine("\t\tY: " + this.coordenadas.Y);
            sb.AppendLine("\t\tZ: " + this.coordenadas.Z);
            
            return sb.ToString();
        }
    }
}
