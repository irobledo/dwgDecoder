using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.AutoCAD.DatabaseServices;

namespace fi.upm.es.dwgDecoder.dwgElementos
{
   /** 
    * @brief   Clase que contiene los atributos de una entidad Polilinea de AutoCAD.
    * 
    **/
    public class dwgPolylinea : dwgEntidadBase
    {
        /** 
        * @brief   Variabe de tipo lista que contiene la lista de los identificadores de las lineas en las que se descompone la polilinea.
        * 
        **/
        public List<ObjectId> lineas = new List<ObjectId>();

        /** 
        * @brief   Variabe de tipo lista que contiene la lista de los identificadores de los arcos en las que se descompone la polilinea.
        * 
        **/
        public List<ObjectId> arcos = new List<ObjectId>();

        /** 
         * @brief   Variabe que serializa el contenido de la entidad Polilinea.
         * @deprecated  No esta actualizado con los últimos atributos de la entidad.
        **/
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
