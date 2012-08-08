using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.ApplicationServices;

using System;
using System.Xml.Serialization;

using fi.upm.es.dwgDecoder.dwgElementos;
 
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
    // - GROSOS DE LA LINEA
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
                    capa.nombreCapa = acLyrTblRec.Name;
                    capa.colorCapa = acLyrTblRec.Color;
                    capa.color_R = capa.colorCapa.Red;
                    capa.color_G = capa.colorCapa.Green;
                    capa.color_B = capa.colorCapa.Blue;

                    dwgf.dwgCapas.Add(capa.objectId, capa);

                    ed.WriteMessage("Capa:" + acLyrTblRec.Name);
                    /*
                    ;
                    acLyrTblRec.Description;
                    acLyrTblRec.Handle;
                    acLyrTblRec.IsHidden;
                    acLyrTblRec.IsLocked;
                    acLyrTblRec.IsOff;
                    acLyrTblRec.IsPersistent;
                    acLyrTblRec.IsUsed;
                    acLyrTblRec.Transparency;
                    acLyrTblRec.LinetypeObjectId;
                    acLyrTblRec.LineWeight;
                    */
                }

                // Open the Block table for read
                BlockTable acBlkTbl = (BlockTable) t.GetObject(db.BlockTableId,OpenMode.ForRead);
 
                // Open the Block table record Model space for read
                BlockTableRecord acBlkTblRec = (BlockTableRecord) t.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],OpenMode.ForRead);
 
                // Step through the Block table record
                foreach (ObjectId acObjId in acBlkTblRec)
                {
                    switch (acObjId.GetType().ToString())
                    {
                        case "": 
                            break;
                        default:
                            ed.WriteMessage(acObjId.ObjectClass.ClassVersion.ToString());                                
                            break;
                    }

                    ed.WriteMessage("\nDXF name: " + acObjId.ObjectClass.DxfName);
                    ed.WriteMessage("\nObjectID: " + acObjId.ToString());
                    ed.WriteMessage("\nHandle: " + acObjId.Handle.ToString());
                    ed.WriteMessage("\nLayerTableId:" + acObjId.Database.LayerTableId.ToString());
                    ed.WriteMessage("\nLayerZero:" + acObjId.Database.LayerZero.ToString());
                    ed.WriteMessage("\n");
                    
                }
            }
            // XmlSerializer x = new XmlSerializer(dwgf.GetType());
            // x.Serialize(Console.Out, dwgf);
        }        
    }
}