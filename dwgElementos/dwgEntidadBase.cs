using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.AutoCAD.DatabaseServices;

namespace fi.upm.es.dwgDecoder.dwgElementos
{
    public abstract class dwgEntidadBase
    {
        public ObjectId objId;
        public ObjectId layerId;        
    }
}
