using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.AutoCAD.DatabaseServices;

namespace fi.upm.es.dwgDecoder.dwgElementos
{
    public class dwgArco : dwgEntidadBase
    {
        public List<ObjectId> lineas = new List<ObjectId>();

        public override String ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("ARCO " + this.objId );
            sb.AppendLine("\tCapaId " + this.capaId);
            sb.AppendLine("\tParentId " + this.parentId);

            foreach (ObjectId obj in this.lineas)
            {
                sb.AppendLine("\t\tLinea " + obj.ToString());
            }

            return sb.ToString();
        }
    }
}
