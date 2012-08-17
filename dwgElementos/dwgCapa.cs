using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Colors;

namespace fi.upm.es.dwgDecoder.dwgElementos
{
    /** 
     * @brief   Clase que contiene los atributos de una Capa de AutoCAD.
     * 
     **/
    public class dwgCapa
    {
        /** 
        * @brief   Variable de tipo ObjectId que identifica de forma unívoca la capa.
        * 
        **/
        public ObjectId objectId;
        
        /** 
         * @brief   Variable de tipo Handle que contiene el identificador fisico de la capa dentro del fichero de AutoCAD.
        * 
        **/
        public Handle handleId;

        /** 
         * @brief   Variable que contiene el nombre de la capa.
         * 
         **/
        public String nombreCapa;

        /** 
         * @brief   Variable que contiene una descripción del contenido de la capa.
         * 
         **/
        public String descripcionCapa;

        /** 
         * @brief   Variabe tipo Color que identifica el color con el que deben ser pintadas las entidades de la capa si no indican un color específico.
         * 
         **/
        public Color colorCapa;

        /** 
         * @brief   Variabe que identifica el valor R del código RGB con el que deben ser pintadas las entidades de la capa si no indican un color específico.
         * 
         **/
        public int color_R;

        /** 
         * @brief   Variabe que identifica el valor G del código RGB con el que deben ser pintadas las entidades de la capa si no indican un color específico.
         * 
         **/
        public int color_G;

        /** 
         * @brief   Variabe que identifica el valor B del código RGB con el que deben ser pintadas las entidades de la capa si no indican un color específico.
         * 
         **/
        public int color_B;

        /** 
         * @brief   Variabe que identifica si la capa esta siendo utilizada.
         * 
         **/
        public bool enUso;

        /** 
         * @brief   Variabe que identifica si la capa esta bloqueada.
         * 
         **/
        public bool bloqueada;

        /** 
         * @brief   Variabe que identifica si la capa esta apagada.
         * 
         **/
        public bool apagada;

        /** 
         * @brief   Variabe que identifica si la capa esta oculta.
         * 
         **/
        public bool oculta;

        /** 
         * @brief   Variabe que identifica el grueso de linea por defecto con el que deben pintarse las entidades de la capa si no indican un grueso 
         *          de línea alternativo.
         * 
         **/
        public Double default_gruesoLinea;

        /** 
         * @brief   Variabe que serializa el contenido de la entidad Punto.
         * @deprecated  No esta actualizada para serializar todos los atributos de la clase.
         **/
        public override String ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("CAPA " + this.objectId);
            sb.AppendLine("\tnombreCapa: " + this.nombreCapa);
            
            return sb.ToString();
        }
    }
}
