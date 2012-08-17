using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.AutoCAD.DatabaseServices;

namespace fi.upm.es.dwgDecoder.dwgElementos
{
    /** 
     * @brief   Clase base que contiene los atributos comunes de cualquier entidad de AutoCAD.
     * 
     **/
    public abstract class dwgEntidadBase
    {
        /** 
        * @brief   Variabe tipo ObjectId que identifica de forma únivoca la entidad.
        * 
        **/
        public ObjectId objId;
        
        /** 
        * @brief   Variabe tipo ObjectId que identifica de forma únivoca la capa a la que esta asociada la entidad.
        * 
        **/
        public ObjectId capaId;
        
        /** 
        * @brief   Variabe tipo ObjectId que identifica si esta informada el objeto del que depende la entidad. 
        *         Util en los procesos de descomposición para asociar las nuevas entidades con la entidad origen.
        * 
        **/
        public ObjectId parentId;

        /** 
        * @brief   Variabe tipo Double que identifica que el ancho de línea a utilizar cuando se pinte la entidad.
        * 
        **/
        public Double LineWeight;
    }
}
