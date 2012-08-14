using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.AutoCAD.DatabaseServices;

namespace fi.upm.es.dwgDecoder.dwgElementos
{
    [Serializable]
    public class dwgFile
    {
        public Dictionary<ObjectId,dwgCapa> dwgCapas = new Dictionary<ObjectId,dwgCapa>();

        public Dictionary<ObjectId, dwgPunto> dwgPuntos = new Dictionary<ObjectId, dwgPunto>();

        public Dictionary<ObjectId, dwgLinea> dwgLineas = new Dictionary<ObjectId, dwgLinea>();

        public Dictionary<ObjectId,dwgPolylinea> dwgPolylineas = new Dictionary<ObjectId,dwgPolylinea>();

        public Dictionary<ObjectId, dwgArco> dwgArcos = new Dictionary<ObjectId, dwgArco>();

        public List<ObjectId> objetosArtificiales = new List<ObjectId>();

        public String nombre_fichero_original;

        public String fecha_fichero_original;

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
