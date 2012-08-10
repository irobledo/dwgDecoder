using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.AutoCAD.DatabaseServices;

namespace fi.upm.es.dwgDecoder.dwgElementos
{
    public class dwgPolylinea : dwgEntidadBase
    {
        public List<ObjectId> lineas = new List<ObjectId>();
        public List<ObjectId> arcos = new List<ObjectId>();

        public override String ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("POLYLINEA " + this.objId);
            sb.AppendLine("\tCapaId " + this.capaId);
            sb.AppendLine("\tParentId " + this.parentId);

            foreach (ObjectId obj in this.lineas)
            {
                sb.AppendLine("\t\tLinea " + obj.ToString());
            }

            foreach (ObjectId obj in this.arcos)
            {
                sb.AppendLine("\t\tArco " + obj.ToString());
            }

            return sb.ToString();
        }
    }
}
