using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.AutoCAD.DatabaseServices;

namespace fi.upm.es.dwgDecoder.dwgElementos
{
    public class dwgFile
    {
        public Dictionary<ObjectId,dwgCapa> dwgCapas = new Dictionary<ObjectId,dwgCapa>();

        public Dictionary<ObjectId, dwgPunto> dwgPuntos = new Dictionary<ObjectId, dwgPunto>();

        public Dictionary<ObjectId, dwgLinea> dwgLineas = new Dictionary<ObjectId, dwgLinea>();

        public Dictionary<ObjectId,dwgPolylinea> dwgPolylineas = new Dictionary<ObjectId,dwgPolylinea>();

    }
}
