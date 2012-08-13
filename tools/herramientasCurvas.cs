using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

using fi.upm.es.dwgDecoder.dwgElementos;

namespace fi.upm.es.dwgDecoder.tools
{
    public static class herramientasCurvas
    {
        public static DBObjectCollection curvaAlineas(Curve cur, int numSeg, BlockTable acBlkTbl, BlockTableRecord acBlkTblRec,Transaction t, ObjectId LayerId, dwgFile dwfg)
        {
            DBObjectCollection ret = new DBObjectCollection();

            // Collect points along our curve
            Point3dCollection pts = new Point3dCollection();

            // Split the curve's parameter space into
            // equal parts            
            double startParam = cur.StartParam;
            double segLen = (cur.EndParam - startParam) / numSeg;

            // Loop along it, getting points each time
            for (int i = 0; i < numSeg + 1; i++)
            {
                Point3d pt = cur.GetPointAtParameter(startParam + segLen * i);
                pts.Add(pt);
            }

            if (pts != null && pts.Count > 0)
            {
                if (pts.Count == 1)
                {
                    // Retornamos un punto.                  
                }
                else if (pts.Count >= 2)
                {
                    // Retornamos una secuencia de lineas
                    for (int i = 0; i < pts.Count - 1; i++)
                    {
                        Line ln = new Line();
                        ln.StartPoint = pts[i];
                        ln.EndPoint = pts[i + 1];
                        ln.LayerId = LayerId;
                        acBlkTblRec.AppendEntity(ln);
                        t.AddNewlyCreatedDBObject(ln, true);
                        dwfg.objetosArtificiales.Add(ln.ObjectId);
                        ret.Add(ln);
                    }
                }
            }

            return ret;
        }
    }
}
