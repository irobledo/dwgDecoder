using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.ApplicationServices;

using System;
using System.Xml.Serialization;

using fi.upm.es.dwgDecoder.dwgElementos;
using fi.upm.es.dwgDecoder.tools;
 
namespace fi.upm.es.dwgDecoder
{
    // Alcance para SSII:
    //
    // 1.CAPAS
    //
    // 2.PUNTOS
    // 3.LINEAS
    // 4.POLILINEAS - CONVERTIDA EN MULTIPLES LINEAS
    // 5.ARCO - CONVERTIDO EN MULTIPLES LINEAS
    //
    // 6.ATRIBUTOS POR DEFECTO PARA TODOS LOS OBJETOS MENOS PARA LA CAPA:
    // - ID: IDENTIFICADOR UNIVOCO
    // - COLOR
    // - TIPO DE LINEA
    // - GROSOR DE LA LINEA
    // - CAPA A LA QUE PERTENECEN

    // 7.ATRIBUTOS PARA LA CAPA
    // - ID: IDENTIFICADOR UNIVOCO
    // - NOMBRE DE LA CAPA     
    public class dwgDecoder
    {
        [CommandMethod("serializarDWG")]
        public static void serializarDWG()
        {
            Database db = Application.DocumentManager.MdiActiveDocument.Database;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            dwgFile dwgf = new dwgFile();

            using (Transaction t = Application.DocumentManager.MdiActiveDocument.Database.TransactionManager.StartTransaction())
            {
                // Leemos las capas
                LayerTable acLyrTbl = (LayerTable)t.GetObject(db.LayerTableId, OpenMode.ForRead);
                foreach (ObjectId acObjId in acLyrTbl)
                {
                    LayerTableRecord acLyrTblRec = (LayerTableRecord)t.GetObject(acObjId, OpenMode.ForRead);

                    dwgCapa capa = new dwgCapa();
                    capa.objectId = acObjId;
                    capa.handleId = acLyrTblRec.Handle;

                    capa.nombreCapa = acLyrTblRec.Name;
                    capa.descripcionCapa = acLyrTblRec.Description;
                    
                    capa.colorCapa = acLyrTblRec.Color;
                    capa.color_R = capa.colorCapa.Red;
                    capa.color_G = capa.colorCapa.Green;
                    capa.color_B = capa.colorCapa.Blue;

                    capa.oculta = acLyrTblRec.IsHidden;
                    capa.bloqueada = acLyrTblRec.IsLocked;
                    capa.apagada = acLyrTblRec.IsOff;
                    capa.enUso = acLyrTblRec.IsUsed;
                    
                    capa.default_gruesoLinea = acLyrTblRec.LineWeight;

                    dwgf.dwgCapas.Add(capa.objectId, capa);

                    ed.WriteMessage("\nProcesada capa:" + acLyrTblRec.Name);                     
                    
                    // acLyrTblRec.LinetypeObjectId;
                    // acLyrTblRec.IsPersistent;
                    // acLyrTblRec.Transparency;             
                }

                // Open the Block table for read
                BlockTable acBlkTbl = (BlockTable) t.GetObject(db.BlockTableId,OpenMode.ForRead);
 
                // Open the Block table record Model space for read
                BlockTableRecord acBlkTblRec = (BlockTableRecord) t.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],OpenMode.ForRead);
 
                // Step through the Block table record
                foreach (ObjectId acObjId in acBlkTblRec)
                {
                    Entity ent = (Entity)t.GetObject(acObjId, OpenMode.ForRead);
                    switch (acObjId.ObjectClass.DxfName)
                    {
                        case "POINT":
                        case "LINE":
                            dwgDecoder.ProcesarObjetos(acObjId, acBlkTbl, acBlkTblRec, t, dwgf);
                            break;
                        case "LWPOLYLINE":
                            DBObjectCollection entitySet = new DBObjectCollection();
                            // ent.Explode(entitySet);
                            ed.WriteMessage("\nProcesada polylinea. Número de entidades a procesar: " + entitySet.Count.ToString());
                            entitySet = dwgDecoder.ObtenerPuntosyLineas(ent,acBlkTbl);
                            /*
                            foreach (Entity ent2 in entitySet)
                            {
                                ed.WriteMessage("\nNueva entidad - " + ent2.ObjectId + ":" + ent2.ObjectId.ObjectClass.DxfName);
                            } 
                            */
                            break;
                        case "ARC":
                            DBObjectCollection entitySet2 = new DBObjectCollection();
                            ent.Explode(entitySet2);
                            ed.WriteMessage("\nProcesado arco. Número de entidades a procesar: " + entitySet2.Count.ToString());
                            entitySet2 = dwgDecoder.ObtenerPuntosyLineas(ent, acBlkTbl);
                            /*
                            foreach (Entity ent2 in entitySet2)
                            {
                                ed.WriteMessage("\nNueva entidad - " + ent2.ObjectId + ":" + ent2.ObjectId.ObjectClass.DxfName);
                            }
                            */
                            break;                        
                        default:
                            ed.WriteMessage("\nTipo de objeto no reconocido por dwgDecoder.");
                            break;
                    }
                }
            }

            exportXml.export2Xml(dwgf);
        }


        private static void ProcesarObjetos(ObjectId acObjId, BlockTable acBlkTbl, BlockTableRecord acBlkTblRec, Transaction t, dwgFile dwgf)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            Entity ent = (Entity)t.GetObject(acObjId, OpenMode.ForRead);
            switch (acObjId.ObjectClass.DxfName)
            {
                case "POINT":
                    DBPoint porigen = (DBPoint)ent;
                    dwgPunto punto = new dwgPunto();
                    punto.objId = acObjId;
                    punto.capaId = ent.LayerId;
                    punto.coordenadas = porigen.Position;
                    if (dwgf.dwgPuntos.ContainsKey(punto.objId) == false)
                    {
                        dwgf.dwgPuntos.Add(punto.objId, punto);
                    }
                    ed.WriteMessage("\nProcesado punto: " + punto.objId.ToString());
                    break;
                case "LINE":
                    Line lorigen = (Line)ent;
                    dwgLinea linea = new dwgLinea();
                    linea.objId = acObjId;
                    linea.capaId = ent.LayerId;
                    DBPoint p_origen_0 = new DBPoint(lorigen.StartPoint);
                    DBPoint p_final_0 = new DBPoint(lorigen.EndPoint);

                    using (Transaction t2 = Application.DocumentManager.MdiActiveDocument.Database.TransactionManager.StartTransaction())
                    {
                        BlockTableRecord acBlkTblRec2 = (BlockTableRecord)t2.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                        acBlkTblRec2.AppendEntity(p_origen_0);
                        acBlkTblRec2.AppendEntity(p_final_0);
                        t2.AddNewlyCreatedDBObject(p_origen_0, true);
                        t2.AddNewlyCreatedDBObject(p_final_0, true);
                    }

                    dwgPunto p_origen_1 = new dwgPunto();
                    p_origen_1.objId = p_origen_0.ObjectId;
                    p_origen_1.coordenadas = p_origen_0.Position;
                    p_origen_1.capaId = linea.capaId;
                    linea.p_origen = p_origen_1;

                    dwgPunto p_final_1 = new dwgPunto();
                    p_final_1.objId = p_final_0.ObjectId;
                    p_final_1.coordenadas = p_final_0.Position;
                    p_final_1.capaId = linea.capaId;
                    linea.p_final = p_final_1;

                    if (dwgf.dwgPuntos.ContainsKey(p_origen_1.objId) == false)
                    {
                        dwgf.dwgPuntos.Add(p_origen_1.objId, p_origen_1);
                    }
                    if (dwgf.dwgPuntos.ContainsKey(p_final_1.objId) == false)
                    {
                        dwgf.dwgPuntos.Add(p_final_1.objId, p_final_1);
                    }

                    dwgf.dwgLineas.Add(linea.objId, linea);

                    ed.WriteMessage("\nProcesada linea: " + linea.objId.ToString());
                    break;
                case "ARC":
                    break;
                case "LWPOLYLINE":
                    /*
                    dwgPolylinea poli = new dwgPolylinea();
                    poli.objId = acObjId;
                    poli.capaId = ent.LayerId;
                    dwgf.dwgPolylineas.Add(poli.objId, poli);                            
                    */
                    break;
                default:
                    ed.WriteMessage(acObjId.ObjectClass.ClassVersion.ToString());
                    break;
            }
            return;
        }

        private static DBObjectCollection ObtenerPuntosyLineas(Entity ent, BlockTable acBlkTbl)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            DBObjectCollection retorno = new DBObjectCollection();
            DBObjectCollection procesar = new DBObjectCollection();

            ent.Explode(procesar);

            using (Transaction t = Application.DocumentManager.MdiActiveDocument.Database.TransactionManager.StartTransaction())
            {
                BlockTableRecord acBlkTblRec2 = (BlockTableRecord)t.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                while (procesar.Count != 0)
                {
                    Entity obj = (Entity) procesar[0];
                    acBlkTblRec2.AppendEntity(obj);
                    t.AddNewlyCreatedDBObject(obj, true);
                    if (obj.ObjectId.ObjectClass.DxfName == "POINT" || obj.ObjectId.ObjectClass.DxfName == "LINE")
                    {
                        retorno.Add(obj);
                    }
                    else
                    {
                        DBObjectCollection aux = new DBObjectCollection();
                        obj.Explode(aux);
                        foreach (DBObject aux2 in aux)
                        {
                            procesar.Add(aux2);
                        }
                    }
                    procesar.Remove(obj);
                }
            }
            return retorno;
        }
    }
}