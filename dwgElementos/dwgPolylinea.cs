using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.AutoCAD.DatabaseServices;

namespace fi.upm.es.dwgDecoder.dwgElementos
{
    public class dwgPolylinea : dwgEntidadBase
    {
        public List<ObjectId> lineas = new List<ObjectId>();
        public List<ObjectId> arcos = new List<ObjectId>();
    }
}
