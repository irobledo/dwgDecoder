using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Colors;

namespace fi.upm.es.dwgDecoder.dwgElementos
{
    /** 
     * @brief   Clase que contiene los atributos de una entidad Punto de AutoCAD.
     * 
     **/
    public class dwgPunto : dwgEntidadBase
    {
       /** 
        * @brief   Variabe tipo Point3d que contiene las coordenadas X,Y,Z del punto.
        * 
        **/
        public Point3d coordenadas;

        /** 
         * @brief   Variabe tipo Color que identifica el color con el que debe ser pintado el punto.
         * 
         **/
        public Color colorPunto;

        /** 
         * @brief   Variabe que identifica el valor R del codigo RGB con el que tiene que ser pintado el punto.
         * 
         **/
        public int color_R;

        /** 
         * @brief   Variabe que identifica el valor G del codigo RGB con el que tiene que ser pintado el punto.
         * 
         **/
        public int color_G;

        /** 
         * @brief   Variabe que identifica el valor B del codigo RGB con el que tiene que ser pintado el punto.
         * 
         **/
        public int color_B;

        /** 
         * @brief   Variabe que serializa el contenido de la entidad Punto.
         **/
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
