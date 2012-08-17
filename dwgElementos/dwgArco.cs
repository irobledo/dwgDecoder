using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.AutoCAD.DatabaseServices;

namespace fi.upm.es.dwgDecoder.dwgElementos
{
    /** 
     * @brief   Clase que contiene los atributos de una entidad Arco de AutoCAD.
     * 
     **/
    public class dwgArco : dwgEntidadBase
    {

        /** 
        * @brief   Variabe de tipo lista que contiene la lista de los identificadores de las lineas en las que se descompone el arco.
        * 
        **/
        public List<ObjectId> lineas = new List<ObjectId>();

        /** 
        * @brief   Variabe de tipo Double que contiene el radio del arco.
        * 
        **/
        public Double radio;

        /** 
        * @brief   Variabe de tipo Double que contiene el ángulo inicial del arco.
        * 
        **/
        public Double angulo_inicio;

        /** 
        * @brief   Variabe de tipo Double que contiene el ángulo final del arco.
        * 
        **/
        public Double angulo_final;

        /** 
        * @brief   Variabe de tipo ObjectId que identifica el punto situado en el centro del arco.
        * 
        **/
        public ObjectId punto_centro;

        /** 
         * @brief   Variabe que serializa el contenido de la entidad Arco.
         * @deprecated  No esta actualizado con los últimos atributos de la entidad.
        **/
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
