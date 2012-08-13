using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.ApplicationServices;

using System;
using System.IO;
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
        static Database db = Application.DocumentManager.MdiActiveDocument.Database;
        static Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
        static bool logActivo = false;
        static bool logDebug = false;

        static bool configuracionUsuario = false;

        static Double dlwdefault = 25;

        static dwgFile dwgf = new dwgFile();

        static String ruta;

        [CommandMethod("serializarDWG")]
        public static void serializarDWG()
        {
            // Solicitamos configuración al usuario.
            configuracionUsuario = ConfiguracionUsuario();
            if (configuracionUsuario == false)
            {
                log("No ha sido posible realizar la configuración del usuario.", false);
                log("Finalizando proceso de decodificación.", false);
                return;
            }

            // Reseteamos el dwgFile
            log("Reseteando estructuras en memoria.",false);
            dwgf.resetDwgFile();

            // Tratamos de cargar el valor por defecto del ancho de linea.
            log("Obteniendo el ancho de línea por defecto.",false);
            try
            {
                String lwdefault = Application.GetSystemVariable("LWDEFAULT").ToString();
                Double.TryParse(lwdefault, out dlwdefault);
            }
            catch (System.Exception e)
            {
                dlwdefault = 25;
            }

            // Abrimos una trasnaccion para poder empezar a operar con la bbdd de Autocad.
            log("Abrimos transacción para empezar a trabajar con la BBDD de AutoCad.",false);
            using (Transaction t = db.TransactionManager.StartTransaction())
            {
                log("Abrimos la tabla de capas.",false);
                // Leemos las capas
                LayerTable acLyrTbl = (LayerTable)t.GetObject(db.LayerTableId, OpenMode.ForRead);

                log("Procesamos las diferentes capas y las almacenamos en memoria.",false);
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
                    
                    capa.default_gruesoLinea = (Double) acLyrTblRec.LineWeight;
                    
                    //if (capa.nombreCapa == "1116-SENAL-RODADURA")
                    //{
                    dwgf.dwgCapas.Add(capa.objectId, capa);
                    //}

                    log("Procesada capa:" + acLyrTblRec.Name,false);                     
                    
                    // acLyrTblRec.LinetypeObjectId;
                    // acLyrTblRec.IsPersistent;
                    // acLyrTblRec.Transparency;             
                }

                log("Abrimos la tabla de bloques que contiene las entidades.", false);
                // Open the Block table for read
                BlockTable acBlkTbl = (BlockTable) t.GetObject(db.BlockTableId,OpenMode.ForWrite);

                log("Abrimos la tabla de bloques - Model Space, única soportada por este proceso.", false);
                // Open the Block table record Model space for read
                BlockTableRecord acBlkTblRec = (BlockTableRecord) t.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],OpenMode.ForWrite);
                
                // Step through the Block table record
                foreach (ObjectId acObjId in acBlkTblRec)
                {
                    Entity ent = (Entity)t.GetObject(acObjId, OpenMode.ForRead);
                    log("Objeto a procesar : " + acObjId.ToString(), true);

                    if (dwgf.dwgCapas.ContainsKey(ent.LayerId) == false) 
                    {
                        log("No se procesa el objeto. Esta en una capa seleccionada para no ser tratada.", true);
                        continue;
                    }

                    if (dwgf.objetosArtificiales.Contains(ent.ObjectId) == true)
                    {
                        log("No se procesa el objeto. Es un objeto no original del mapa creado por este proceso.", true);
                        continue;
                    }

                    switch (acObjId.ObjectClass.DxfName)
                    {
                        case "POINT":
                        case "LINE":
                        case "LWPOLYLINE":
                        case "ARC":
                            ObjectId parentId = new ObjectId();
                            dwgDecoder.ProcesarObjetos(acObjId, acBlkTbl, acBlkTblRec, t, parentId);
                            log("Procesado punto/linea/polylinea:/arco " + acObjId.ToString(),true);
                            break;                        
                        default:
                            log("Tipo de objeto no reconocido por dwgDecoder: " + acObjId.ObjectClass.DxfName.ToString(), true);
                            break;
                    }
                }
            }

            // exportXml.serializar(dwgf);
            log("Exportamos al formato XML el contenido de la base de datos de Autocad.", false);
            exportXml.export2Xml(dwgf,ruta);
        }


        private static void ProcesarObjetos(ObjectId acObjId, BlockTable acBlkTbl, BlockTableRecord acBlkTblRec, Transaction t, ObjectId parentId)
        {
            Entity ent = (Entity)t.GetObject(acObjId, OpenMode.ForRead);
            switch (acObjId.ObjectClass.DxfName)
            {
                case "POINT":
                    DBPoint porigen = (DBPoint)ent;
                    dwgPunto punto = new dwgPunto();
                    punto.objId = acObjId;
                    punto.capaId = ent.LayerId;
                    punto.parentId = parentId;
                    punto.coordenadas = porigen.Position;
                    
                    if (dwgf.dwgPuntos.ContainsKey(punto.objId) == false)
                    {
                        dwgf.dwgPuntos.Add(punto.objId, punto);
                    }
                    log("Procesado punto: " + punto.objId.ToString(),true);
                    break;
                case "LINE":
                    Line lorigen = (Line)ent;
                    dwgLinea linea = new dwgLinea();
                    linea.objId = acObjId;
                    linea.capaId = ent.LayerId;
                    linea.parentId = parentId;

                    linea.LineWeight = (Double)lorigen.LineWeight;

                    if (linea.LineWeight == -1)
                    {
                        log("Ancho de la linea igual al ancho de la capa: " + linea.objId.ToString(), true);
                        // Reemplazar por el ancho de la capa.
                        linea.LineWeight = dlwdefault;
                        dwgCapa c;
                        dwgf.dwgCapas.TryGetValue(linea.capaId,out c);
                        if (c != null)
                        {
                            linea.LineWeight = c.default_gruesoLinea;
                        }
                    }
                    else if ((linea.LineWeight == -2) || (linea.LineWeight == -3))
                    {
                        // -2: Reemplazar por el ancho del bloque. Esto habra que implementarlo cuando se de soporte a bloques.
                        // -3: ancho por defecto del autocad. Comando LWDEFAULT
                        log("Ancho de la linea igual al del bloque o al ancho por defecto: " + linea.objId.ToString(), true);
                        linea.LineWeight = dlwdefault;
                    }
                    
                    DBPoint p_origen_0 = new DBPoint(lorigen.StartPoint);
                    DBPoint p_final_0 = new DBPoint(lorigen.EndPoint);
                    acBlkTblRec.AppendEntity(p_origen_0);
                    acBlkTblRec.AppendEntity(p_final_0);
                    t.AddNewlyCreatedDBObject(p_origen_0, true);
                    t.AddNewlyCreatedDBObject(p_final_0, true);
                    
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
                    if (dwgf.dwgLineas.ContainsKey(linea.objId) == false)
                    {
                        dwgf.dwgLineas.Add(linea.objId, linea);
                    }
                    log("Procesada linea: " + linea.objId.ToString(),true);
                    break;
                case "LWPOLYLINE":
                    dwgPolylinea poli = new dwgPolylinea();
                    poli.objId = acObjId;
                    poli.capaId = ent.LayerId;
                    poli.parentId = parentId;
                    
                    // Descomponemos en subcomponentes.
                    DBObjectCollection entitySet = new DBObjectCollection();
                    log("Descomponemos polylinea en lineas y puntos: " + poli.objId.ToString(), true);
                    entitySet = dwgDecoder.ObtenerPuntosyLineas(ent, acBlkTbl, acBlkTblRec, t);
                    
                    // Procesamos cada uno de los subcomponentes.
                    // Solo pueden ser: lineas y arcos. Una polylinea no puede formarse con nada mas.
                    foreach (Entity ent2 in entitySet)
                    {
                        switch (ent2.ObjectId.ObjectClass.DxfName)
                        {
                            case "LINE":
                                log("Obtenida linea: " + poli.objId.ToString() + ":" + ent2.ObjectId.ToString(), true);
                                poli.lineas.Add(ent2.ObjectId);
                                break;
                            case "ARC":
                                log("Obtenido arco: " + poli.objId.ToString() + ":" + ent2.ObjectId.ToString(), true);
                                poli.arcos.Add(ent2.ObjectId);
                                break;
                            default:
                                log("Al descomponer polylinea, objeto no reconocido:" + ent2.ObjectId.ObjectClass.DxfName,true);
                                break;
                        }
                        log("Procesamos la nueva entidad obtenida - " + ent2.ObjectId.ObjectClass.DxfName + ":" + ent2.ObjectId, true);
                        dwgDecoder.ProcesarObjetos(ent2.ObjectId, acBlkTbl, acBlkTblRec, t, poli.objId);
                        
                    }

                    if ((entitySet.Count > 0) && (dwgf.dwgPolylineas.ContainsKey(poli.objId) == false))
                    {
                        dwgf.dwgPolylineas.Add(poli.objId, poli);
                    }
                    log("Procesada polilinea: " + poli.objId.ToString(),true);
                    break;
                case "ARC":
                    dwgArco arco = new dwgArco();
                    arco.objId = acObjId;
                    arco.capaId = ent.LayerId;
                    arco.parentId = parentId;
                    
                    // Descomponemos en subcomponentes.
                    log("Descomponemos arco en lineas: " + arco.objId.ToString(), true);
                    DBObjectCollection entitySet2 = new DBObjectCollection();
                    
                    entitySet2 = herramientasCurvas.curvaAlineas((Curve)ent, 5, acBlkTbl, acBlkTblRec, t,arco.capaId, dwgf);

                    // Procesamos cada uno de los subcomponentes.
                    // Solo pueden ser: lineas. Eso lo garantiza la funcion curvaAlineas
                    foreach (Entity ent2 in entitySet2)
                    {
                        log("Nueva entidad - " + ent2.ObjectId.ObjectClass.DxfName + ":" + ent2.ObjectId, true);
                        arco.lineas.Add(ent2.ObjectId);
                        dwgDecoder.ProcesarObjetos(ent2.ObjectId, acBlkTbl, acBlkTblRec, t, arco.objId);                        
                    }

                    if (dwgf.dwgArcos.ContainsKey(arco.objId) == false)
                    {
                        dwgf.dwgArcos.Add(arco.objId, arco);
                    }
                    log("Procesado arco: " + arco.objId.ToString(),true);
                    break;                
                default:
                    log("Elemento no reconocido para procesar. No procesado. " + acObjId.ObjectClass.ClassVersion.ToString(),true);
                    break;
            }
            return;
        }

        private static DBObjectCollection ObtenerPuntosyLineas(Entity ent, BlockTable acBlkTbl, BlockTableRecord acBlkTblRec, Transaction t)
        {
            DBObjectCollection retorno = new DBObjectCollection();
            DBObjectCollection procesar = new DBObjectCollection();

            ent.Explode(procesar);
            
            while (procesar.Count != 0)
            {
                Entity obj = (Entity) procesar[0];
                acBlkTblRec.AppendEntity(obj);
                t.AddNewlyCreatedDBObject(obj, true);
                dwgf.objetosArtificiales.Add(obj.ObjectId);
                                    
                if (obj.ObjectId.ObjectClass.DxfName == "POINT" || obj.ObjectId.ObjectClass.DxfName == "LINE")
                {
                    if (retorno.Contains(obj) == false)
                    {
                        retorno.Add(obj);
                    }                        
                }
                if (obj.ObjectId.ObjectClass.DxfName == "ARC")
                {
                   // Completar con el proceso para los arcos.                   
                    if (retorno.Contains(obj) == false)
                    {
                        retorno.Add(obj);
                    }
                }
                if (obj.ObjectId.ObjectClass.DxfName == "LWPOLYLINE")
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
        
            return retorno;
        }

        private static void log(String msg, bool debug)
        {
            if (logActivo == true)
            {
                if (((debug == true) && (logDebug == true)) || (debug == false))
                {
                    ed.WriteMessage("\n[dwgDecoder] " + System.DateTime.Now.ToString() + ": " + msg);
                }                
            }
            return;
        }

        private static bool ConfiguracionUsuario()
        {
            String userLog = "";
            while (userLog == "")
            {
                PromptStringOptions pStrOpts = new PromptStringOptions("\nIntroduzca si desea log de la decodificación (s/n): ");
                PromptResult pStrRes = ed.GetString(pStrOpts);
                if (pStrRes.Status == PromptStatus.Cancel)
                {
                    return false;
                }
                else if (pStrRes.Status == PromptStatus.OK)
                {
                    userLog = pStrRes.StringResult;
                    if ((userLog == "S") || (userLog == "s"))
                    {
                        logActivo = true;
                        break;
                    }
                    else if ((userLog == "N") || (userLog == "n"))
                    {
                        logActivo = false;
                        break;
                    }
                    else
                    {
                        userLog = "";
                    }
                }
            }

            if (logActivo == true)
            {
                String userDebug = "";
                while (userDebug == "")
                {
                    PromptStringOptions pStrOpts = new PromptStringOptions("\nIntroduzca si desea un log con información de debug del proceso de decodificación (s/n): ");
                    PromptResult pStrRes = ed.GetString(pStrOpts);
                    if (pStrRes.Status == PromptStatus.Cancel)
                    {
                        return false;
                    }
                    else if (pStrRes.Status == PromptStatus.OK)
                    {
                        userDebug = pStrRes.StringResult;
                        if ((userDebug == "S") || (userDebug == "s"))
                        {
                            logDebug = true;
                            break;
                        }
                        else if ((userDebug == "N") || (userDebug == "n"))
                        {
                            logDebug = false;
                            break;
                        }
                        else
                        {
                            userDebug = "";
                        }
                    }
                }
            }

            // System.IO.Path.IsPathRooted(@"c:\foo"); 
            String filePath = "";
            while (filePath == "")
            {
                PromptStringOptions pStrOpts = new PromptStringOptions("\nIntroduzca el nombre del fichero (ruta completa) donde quiere guardar la salida del proceso: ");
                PromptResult pStrRes = ed.GetString(pStrOpts);
                if (pStrRes.Status == PromptStatus.Cancel)
                {
                    return false;
                }
                else if (pStrRes.Status == PromptStatus.OK)
                {
                    filePath = pStrRes.StringResult;
                    try
                    {
                        filePath = Path.GetFullPath(filePath);
                        ed.WriteMessage("El fichero con toda la información sera creado en la ruta: " + filePath);
                        ruta = filePath;
                    }
                    catch (System.Exception e)
                    {
                        ed.WriteMessage("Error. No es una ruta válida");
                        ed.WriteMessage(e.ToString());
                        filePath = "";
                    }
                }
            }
            return true;
        }
    }    
}