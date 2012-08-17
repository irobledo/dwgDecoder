using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.AutoCAD.DatabaseServices;

namespace fi.upm.es.dwgDecoder.dwgElementos
{
    /** 
     * @brief   Clase que contiene todas las entidades reconocidas y analizadas de un fichero de AutoCAD.
     * 
     **/
    [Serializable]
    public class dwgFile
    {
        /** 
        * @brief   Variable que contiene todas las capas (tipo dwgCapa) analizadas.
        * 
        **/
        public Dictionary<ObjectId,dwgCapa> dwgCapas = new Dictionary<ObjectId,dwgCapa>();

        /** 
        * @brief   Variable que contiene todos los puntos (tipo dwgPunto) analizados.
        * 
        **/
        public Dictionary<ObjectId, dwgPunto> dwgPuntos = new Dictionary<ObjectId, dwgPunto>();

        /** 
        * @brief   Variable que contiene todas las líneas (tipo dwgLinea) analizadas.
        * 
        **/
        public Dictionary<ObjectId, dwgLinea> dwgLineas = new Dictionary<ObjectId, dwgLinea>();

        /** 
        * @brief   Variable que contiene todas las polilineas (tipo dwgPolylinea) analizadas.
        * 
        **/
        public Dictionary<ObjectId,dwgPolylinea> dwgPolylineas = new Dictionary<ObjectId,dwgPolylinea>();

        /** 
        * @brief   Variable que contiene todos las arcos (tipo dwgArco) analizados.
        * 
        **/
        public Dictionary<ObjectId, dwgArco> dwgArcos = new Dictionary<ObjectId, dwgArco>();

        /** 
        * @brief   Variable que contiene todas las entidades generadas artificialmente y no pertenecientes al fichero de AutoCAD original durante
         *         el proceso de análisis.
        * 
        **/
        public List<ObjectId> objetosArtificiales = new List<ObjectId>();

        /** 
        * @brief   Variable que contiene contiene el nombre del fichero original del cual se extrae la información.
        * 
        **/
        public String nombre_fichero_original;

        /** 
        * @brief   Variable que contiene la fecha de última modificación dle fichero original del cual se extrae la información.
        * 
        **/
        public String fecha_fichero_original;

        /** 
        * @brief   Metodo que inicializa y vacia todas las variables de la clase para poder ser utilizada la instancia con un fichero nuevo
         *         o lanzar un nuevo proceso de decodificación.
        * 
        **/
        public void resetDwgFile()
        {
            this.nombre_fichero_original = "";
            this.fecha_fichero_original = "";
            this.dwgCapas.Clear();
            this.dwgPuntos.Clear();
            this.dwgLineas.Clear();
            this.dwgPolylineas.Clear();
            this.dwgArcos.Clear();
            this.objetosArtificiales.Clear();

            return;
        }
    }
}
