using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.AutoCAD.Colors;

namespace fi.upm.es.dwgDecoder.dwgElementos
{
    /** 
     * @brief   Clase que contiene los atributos de una entidad Linea de AutoCAD.
     * 
     **/
    public class dwgLinea : dwgEntidadBase
    {
        /** 
        * @brief   Variabe de tipo dwgPunto que identifica el punto origen de la línea.
        * 
        **/
        public dwgPunto p_origen;

        /** 
        * @brief   Variabe de tipo dwgPunto que identifica el punto final de la línea.
        * 
        **/
        public dwgPunto p_final;

        /** 
        * @brief   Variabe que identifica color con el que tiene que ser pintada la linea.
        * 
        **/
        public Color colorLinea;

        /** 
        * @brief   Variabe que identifica el valor R del codigo RGB con el que tiene que ser pintada la linea.
        * 
        **/
        public int color_R;

        /** 
        * @brief   Variabe que identifica el valor G del codigo RGB con el que tiene que ser pintada la linea.
        * 
        **/
        public int color_G;

       /** 
        * @brief   Variabe que identifica el valor B del codigo RGB con el que tiene que ser pintada la linea.
        * 
        **/
        public int color_B;
    }
}
