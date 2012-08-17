using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

using fi.upm.es.dwgDecoder.dwgElementos;

namespace fi.upm.es.dwgDecoder.tools
{
    /** 
     * @brief   Clase que implementa los metodos para el manipulado y conversión de curvas a segmentos de linea.
     * 
     **/
    public static class herramientasCurvas
    {
        /**
         * @brief   Metodo que descompone una curva en un conjunto de segmentos de línea que aproximan o recubren la curva original.
         *          
         * @param   cur         Entidad curva que debe ser linealizada.
         * @param   numSeg      Número de líneas en las que tiene que ser partida la curva.
         * @param   acBlkTbl    Tabla de bloques de AutoCAD para buscar nuevos objetos y añadir nuevos objetos generados.
         * @param   acBlkTblRec Tabla de registros de los bloques de AutoCAD para buscar nuevos objetos y añadir nuevos objetos generados.
         * @param   t           Transaccion abierta para manipular la tabla de bloques de AutoCAD.
         * @param   LayerId     Parámetro del tipo ObjectId que identifica la capa a la que tendrán que asociarse las nuevas líneas generadas por el proceso
         *                      de descomposición de la curva.
         * @param   dwfg        Parámetro del tipo dwgFile donde se almacenaran las nuevas líneas creadas a partir del proceso de descomposición de la curva.
         * 
         * @return              Devuelve una colección de entidades tipo linea bajo la clase DBObjectCollection.
         **/

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
