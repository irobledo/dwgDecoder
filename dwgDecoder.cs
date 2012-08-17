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
 
    /** 
     * @brief   Clase que implementa el plugin de Autocad. Contiene toda la lógica de decodificación
     *          de la base de datos DWG, todo la logica de almacenamiento en estructuras en memoria y 
     *          toda la interacción con el usuario.
     */

    public class dwgDecoder
    {
        /**
         * @brief   Variable estatica del tipo "Autodesk.AutoCAD.DatabaseServices.Database
         *          que sera la referencia en todo momento del plugin para el acceso y el manipulado
         *          del contenido del fichero de AutoCAD. Cada vez que se invoca  el plugin se 
         *          asocia a la base de datos del fichero activo que hay en ese momento en la 
         *          ventana del Autocad
         */
        static Database db;
        
        /**
         * @brief   Variable estatica del tipo "Autodesk.AutoCAD.EditorInput.Editor
         *          que sera la referencia en todo momento del plugin para la interacción con
         *          el usuario, ya sea para mostrar mensajes o para pedir información al usuario.
         *          Cada vez que se invoca el plugin se asocia al editor activo que hay en ese momento
         *          en la ventana del Autocad.
         */
        static Editor ed;
        
        /**
         * @brief   Variable estatica del tipo "Autodesk.AutoCAD.ApplicationServicies.Document
         *          que sera la referencia en todo momento del plugin del documento activo en la
         *          ventana de Autodesk.Cada vez que se invoca el plugin se asocia al documento 
         *          activo que hay en ese momento en la ventana del Autocad.
         */
        static Document doc;

        /**
         * @brief   Variable estatica que indica si el plugin debe generar informacion de log
         */
        static bool logActivo = false;
        
        /**
         * @brief   Variable estatica que indica si el plugin debe generar de debug junto a la informacion de log.
         *          Para poder generarse es necesario que la información de log este activada.
         */
        static bool logDebug = false;
        
        /**
         * @brief   Variable estatica que indica donde se guardará el fichero XML con la salida final
         *          del proceso.
         */
        static String ruta;
        
        /**
         * @brief   Variable estatica que indica si el usuario va a realizar una seleccion manual de
         *          las capas del fichero DWG que deben ser procesadas.
         */
        static bool selectLayers = false;
        
        /**
         * @brief   Variable estatica que indica si la configuración solicitada al usuario ha sido
         *          recibida y procesada de forma correcta.
         */
        static bool configuracionUsuario = false;
        
        /**
         * @brief   Variable estatica que indica el ancho por defecto que tendran las líneas en el caso
         *          de no poder leer esta información del propio fichero DWG.
         */
        static Double dlwdefault = 25;
        
        /**
         * @brief   Variable estatica que referencia a un objeto dwgFile que contendrá toda la información
         *          extraida de la base de datos del fichero DWG de AutoCad. Este fichero será el que luego
         *          se vuelque a un formato XML.
         */
        static dwgFile dwgf = new dwgFile();

        /**
         * @brief   Metodo que implementa el comando "serializarDWG" dentro del propio AutoCAD y que se
         *          encarga de extraer toda la información necesaria de la base de datos del propio 
         *          fichero DWG y almacenarla de forma estructurada en la variable dwgf.
         *          
         *          [CommandMethod("serializarDWG")] indica a AUTOCAD el nombre del nuevo comando a implementar.
         *          
         *          El proceso funciona en dos 4 etapas principales:
         *          
         *          1) Solicitar la configuración necesaria por parte del usuario:<br/><br/>
         *              1.1)    ¿Activar el log?<br/>
         *              1.2)    ¿Incorporar información de debug al log?<br/>
         *              1.3)    Indicar la ruta donde se guardará el fichero de salida con el contenido de la base de datos
         *                      de Autocad.<br/>
         *              1.4)    ¿Seleccionará el usuario manualmente las capas a procesar?<br/><br/>
         * 
         *           2) Leer el conjunto de capas disponible para ser analizadas y si el usuario lo
         *              ha configurado, solicitarle que seleccione cuales quiere utilizar en el proceso.<br/><br/>
         *
         *           3) Analizar cada una de las entidades dentro del fichero siempre que este dentro de una de las capas
         *              a procesar y sea una de las entidades reconocidas: punto, linea, polilinea y arco.<br/><br/>
         *  
         *           4) Exportar toda la información resultante a un fichero XML configurado por el usuario.
         *
         */
        [CommandMethod("serializarDWG")]
        public static void serializarDWG()
        {
            db = Application.DocumentManager.MdiActiveDocument.Database;
            ed = Application.DocumentManager.MdiActiveDocument.Editor;
            doc = Application.DocumentManager.MdiActiveDocument;

            // Solicitamos configuración al usuario.
            configuracionUsuario = ConfiguracionUsuario();
            if (configuracionUsuario == false)
            {
                log("No ha sido posible realizar la configuración del usuario.", false, false);
                log("Finalizando proceso de decodificación.", false, false);
                return;
            }

            // Reseteamos el dwgFile
            log("Reseteando estructuras en memoria.",false, false);
            dwgf.resetDwgFile();
            dwgf.nombre_fichero_original = doc.Name;
            
            // Tratamos de cargar el valor por defecto del ancho de linea.
            log("Obteniendo el ancho de línea por defecto.",false, false);
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
            log("Abrimos transacción para empezar a trabajar con la BBDD de AutoCad.",false, false);
            using (Transaction t = db.TransactionManager.StartTransaction())
            {
                log("Abrimos la tabla de capas.",false, false);
                // Leemos las capas
                LayerTable acLyrTbl = (LayerTable)t.GetObject(db.LayerTableId, OpenMode.ForRead);

                log("Procesamos las diferentes capas y las almacenamos en memoria.",false, false);
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
                    if ((capa.default_gruesoLinea == -1) || (capa.default_gruesoLinea == -2) || (capa.default_gruesoLinea == -3))
                    {
                        capa.default_gruesoLinea = dlwdefault;
                    }
                    
                    // Preguntamos al usuario si decide procesar la capa.
                    if (selectLayers == true)
                    {
                        String incluirCapa = IncluirCapa(capa);
                        if (incluirCapa == "C")
                        {
                            log("Finalizando proceso de decodificación.", false, false);
                            return;
                        }
                        if ((incluirCapa == "S") || (incluirCapa == "s"))
                        {
                            dwgf.dwgCapas.Add(capa.objectId, capa);
                            log("Procesada capa:" + acLyrTblRec.Name, true, false);                     
                        }
                    }
                    else
                    {
                        dwgf.dwgCapas.Add(capa.objectId, capa);
                        log("Procesada capa:" + acLyrTblRec.Name, true, false);                     
                    }            
                    // acLyrTblRec.LinetypeObjectId;
                    // acLyrTblRec.IsPersistent;
                    // acLyrTblRec.Transparency;             
                }

                log("Abrimos la tabla de bloques que contiene las entidades.", false, false);
                // Open the Block table for read
                BlockTable acBlkTbl = (BlockTable) t.GetObject(db.BlockTableId,OpenMode.ForWrite);

                log("Abrimos la tabla de bloques - Model Space, única soportada por este proceso.", false, false);
                // Open the Block table record Model space for read
                BlockTableRecord acBlkTblRec = (BlockTableRecord) t.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],OpenMode.ForWrite);
                
                // Step through the Block table record
                foreach (ObjectId acObjId in acBlkTblRec)
                {
                    Entity ent = (Entity)t.GetObject(acObjId, OpenMode.ForRead);
                    log("Objeto a procesar : " + acObjId.ToString(), true, true);

                    if (dwgf.dwgCapas.ContainsKey(ent.LayerId) == false) 
                    {
                        log("No se procesa el objeto. Esta en una capa seleccionada para no ser tratada.", true, true);
                        continue;
                    }

                    if (dwgf.objetosArtificiales.Contains(ent.ObjectId) == true)
                    {
                        log("No se procesa el objeto. Es un objeto no original del mapa creado por este proceso.", true, true);
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
                            log("Procesado punto/linea/polylinea:/arco " + acObjId.ToString(),true,true);
                            break;                        
                        default:
                            log("Tipo de objeto no reconocido por dwgDecoder: " + acObjId.ObjectClass.DxfName.ToString(), true, true);
                            break;
                    }
                }
            }

            // exportXml.serializar(dwgf);
            log("Exportamos al formato XML el contenido de la base de datos de Autocad.", false, false);
            exportXml.export2Xml(dwgf,ruta);
        }

        /**
         * @brief   Metodo que contiene toda la lógica para procesar cada tipo de entidad. En función del tipo de entidad crea las estructuras
         *          en memoria necesarias y almacena toda la información en la clase dwgFile para que posteriormente sea explotada.
         *          
         **/
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
                    punto.colorPunto = porigen.Color;
                    punto.color_R = porigen.Color.Red;
                    punto.color_G = porigen.Color.Green;
                    punto.color_B = porigen.Color.Blue;

                    
                    if (dwgf.dwgPuntos.ContainsKey(punto.objId) == false)
                    {
                        dwgf.dwgPuntos.Add(punto.objId, punto);
                    }
                    log("Procesado punto: " + punto.objId.ToString(),true,true);
                    break;
                case "LINE":
                    Line lorigen = (Line)ent;
                    dwgLinea linea = new dwgLinea();
                    linea.objId = acObjId;
                    linea.capaId = ent.LayerId;
                    linea.parentId = parentId;
                    linea.colorLinea = lorigen.Color;
                    linea.color_R = lorigen.Color.Red;
                    linea.color_G = lorigen.Color.Green;
                    linea.color_B = lorigen.Color.Blue;

                    linea.LineWeight = (Double)lorigen.LineWeight;

                    if (linea.LineWeight == -1)
                    {
                        log("Ancho de la linea igual al ancho de la capa: " + linea.objId.ToString(), true, true);
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
                        log("Ancho de la linea igual al del bloque o al ancho por defecto: " + linea.objId.ToString(), true, true);
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
                    log("Procesada linea: " + linea.objId.ToString(), true, true);
                    break;
                case "LWPOLYLINE":
                    dwgPolylinea poli = new dwgPolylinea();
                    poli.objId = acObjId;
                    poli.capaId = ent.LayerId;
                    poli.parentId = parentId;
                    
                    // Descomponemos en subcomponentes.
                    DBObjectCollection entitySet = new DBObjectCollection();
                    log("Descomponemos polylinea en lineas y puntos: " + poli.objId.ToString(), true, true);
                    entitySet = dwgDecoder.ObtenerPuntosyLineas(ent, acBlkTbl, acBlkTblRec, t);
                    
                    // Procesamos cada uno de los subcomponentes.
                    // Solo pueden ser: lineas y arcos. Una polylinea no puede formarse con nada mas.
                    foreach (Entity ent2 in entitySet)
                    {
                        switch (ent2.ObjectId.ObjectClass.DxfName)
                        {
                            case "LINE":
                                log("Obtenida linea: " + poli.objId.ToString() + ":" + ent2.ObjectId.ToString(),true, true);
                                poli.lineas.Add(ent2.ObjectId);
                                break;
                            case "ARC":
                                log("Obtenido arco: " + poli.objId.ToString() + ":" + ent2.ObjectId.ToString(), true, true);
                                poli.arcos.Add(ent2.ObjectId);
                                break;
                            default:
                                log("Al descomponer polylinea, objeto no reconocido:" + ent2.ObjectId.ObjectClass.DxfName, true, true);
                                break;
                        }
                        log("Procesamos la nueva entidad obtenida - " + ent2.ObjectId.ObjectClass.DxfName + ":" + ent2.ObjectId, true, true);
                        dwgDecoder.ProcesarObjetos(ent2.ObjectId, acBlkTbl, acBlkTblRec, t, poli.objId);
                        
                    }

                    if ((entitySet.Count > 0) && (dwgf.dwgPolylineas.ContainsKey(poli.objId) == false))
                    {
                        dwgf.dwgPolylineas.Add(poli.objId, poli);
                    }
                    log("Procesada polilinea: " + poli.objId.ToString(),true, true);
                    break;
                case "ARC":
                    Arc ar = (Arc) ent;
                    dwgArco arco = new dwgArco();
                    arco.objId = acObjId;
                    arco.capaId = ent.LayerId;
                    arco.parentId = parentId;
                    arco.radio = ar.Radius;
                    arco.angulo_inicio = ar.StartAngle;
                    arco.angulo_final = ar.EndAngle;

                    DBPoint p_centro = new DBPoint(ar.Center);
                    acBlkTblRec.AppendEntity(p_centro);
                    t.AddNewlyCreatedDBObject(p_centro, true);
                                        
                    dwgPunto p_centro_1 = new dwgPunto();
                    p_centro_1.objId = p_centro.ObjectId;
                    p_centro_1.coordenadas = p_centro.Position;
                    p_centro_1.capaId = arco.capaId;

                    if (dwgf.dwgPuntos.ContainsKey(p_centro_1.objId) == false)
                    {
                        dwgf.dwgPuntos.Add(p_centro_1.objId, p_centro_1);
                    }

                    arco.punto_centro = p_centro_1.objId;

                    // Descomponemos en subcomponentes.
                    log("Descomponemos arco en lineas: " + arco.objId.ToString(), true, true);
                    DBObjectCollection entitySet2 = new DBObjectCollection();
                    
                    entitySet2 = herramientasCurvas.curvaAlineas((Curve)ent, 5, acBlkTbl, acBlkTblRec, t,arco.capaId, dwgf);

                    // Procesamos cada uno de los subcomponentes.
                    // Solo pueden ser: lineas. Eso lo garantiza la funcion curvaAlineas
                    foreach (Entity ent2 in entitySet2)
                    {
                        log("Nueva entidad - " + ent2.ObjectId.ObjectClass.DxfName + ":" + ent2.ObjectId, true, true);
                        arco.lineas.Add(ent2.ObjectId);
                        dwgDecoder.ProcesarObjetos(ent2.ObjectId, acBlkTbl, acBlkTblRec, t, arco.objId);                        
                    }

                    if (dwgf.dwgArcos.ContainsKey(arco.objId) == false)
                    {
                        dwgf.dwgArcos.Add(arco.objId, arco);
                    }
                    log("Procesado arco: " + arco.objId.ToString(),true, true);
                    break;                
                default:
                    log("Elemento no reconocido para procesar. No procesado. " + acObjId.ObjectClass.ClassVersion.ToString(),true,true);
                    break;
            }
            return;
        }

        /**
         * @brief   Metodo que descompone una enitdad AUTOCAD en sub-entidades cuando es posible. Replica el comportamiento del comando 
         *          DESCOMPONER / EXPLODE de AutoCAD. Las unidades básicas que devuelve son puntos y lineas. Descompone recursivamente
         *          las entidades hasta dejarlas representadas como puntos, lineas y arcos.
         *          
         * @param   ent         Entidad que debe ser descompuesta
         * @param   acBlkTbl    Tabla de bloques de AutoCAD para buscar nuevos objetos y añadir nuevos objetos generados.
         * @param   acBlkTblRec Tabla de registros de los bloques de AutoCAD para buscar nuevos objetos y añadir nuevos objetos generados.
         * @param   t           Transaccion abierta para manipular la tabla de bloques de AutoCAD.
         * 
         * @return              Devuelve una colección de entidades bajo la clase DBObjectCollection.
         **/
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

        /**
         * @brief   Metodo que controla el volcado de información hacia el usuario final (control de log).
         *          Cualquier salida que quiera enviarse hacia el usuario o cualquier log de la aplicación
         *          debe realizarse a través de este método.
         *  
         * @param   msg     Mensaje que quiere enviarse por la salida al usuario o al log de la aplicación.
         * @param   log     Parámetro booleano que identifica si el mensaje es un mensaje de log (true) o un mensaje estadar para el usuario (false)
         * @param   debug   Parámetro booleano que identifica si el mensaje de log es un mensaje de log estandar o cotiene información de
         *                  debug del proceso.         *                  
         *                  
         **/
        private static void log(String msg, bool log, bool debug)
        {
            if (log == false)
            {
                ed.WriteMessage("\n[dwgDecoder] " + System.DateTime.Now.ToString() + ": " + msg);
            }
            else
            {
                if (logActivo == true)
                {
                    if (((debug == true) && (logDebug == true)) || (debug == false))
                    {
                        ed.WriteMessage("\n[dwgDecoder] " + System.DateTime.Now.ToString() + ": " + msg);
                    }
                }
            }
            return;
        }

        /**
         * @brief   Metodo que contiene toda la lógica de interacción con el usuario final para realizar la 
         *          configuración necesaria del proceso:<br/><br/>
         *          
         *          1) Activación del log (s/n).<br/>
         *          2) En caso afirmativo, activación de la información de debug.<br/>
         *          3) Ruta del fichero donde se guardará la información obtenida de la base de datos de AutoCAD.<br/>
         *          4) Selección manual de las capas a procesar (s/n)
         * 
         * @return  Devuelve "Verdadero" si ha podido finalizar la configuración del usuario.<br/>
         *          Devuelve "Falso" si el usuario ha cancelado la configuración o se ha producido un error.<br/>
         * */
        private static bool ConfiguracionUsuario()
        {
            try
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
                            log("El fichero con toda la información sera creado en la ruta: " + filePath, false, false);
                            ruta = filePath;
                        }
                        catch (System.Exception e)
                        {
                            log("Error. No es una ruta válida", false, false);
                            log(e.ToString(),false,false);
                            filePath = "";
                        }
                    }
                }

                String userLayers = "";
                while (userLayers == "")
                {
                    PromptStringOptions pStrOpts = new PromptStringOptions("\nIntroduzca si desea configurar manualmente las capas a procesar (s/n): ");
                    PromptResult pStrRes = ed.GetString(pStrOpts);
                    if (pStrRes.Status == PromptStatus.Cancel)
                    {
                        return false;
                    }
                    else if (pStrRes.Status == PromptStatus.OK)
                    {
                        userLayers = pStrRes.StringResult;
                        if ((userLayers == "S") || (userLayers == "s"))
                        {
                            selectLayers = true;
                            break;
                        }
                        else if ((userLayers == "N") || (userLayers == "n"))
                        {
                            selectLayers = false;
                            break;
                        }
                        else
                        {
                            userLayers = "";
                        }
                    }
                }
                return true;
            }
            catch (System.Exception e)
            {
                log("No ha sido posible realizar la configuración de usuario.", false, false);
                log(e.ToString(), false, false);
                return false;
            }
        }
        /**
         * @brief   Metodo que contiene toda la lógica de interacción con el usuario final para decidir si
         *          una capa concreta debe ser procesada.
         *          
         * @param   c   Objeto de tipo dwgCapa que contiene toda la información obtenida de la capa.
         *
         * @return  Retorna una cadena de texto con 3 valores posibles:<br/><br/>
         *              1) C: Cancelar el proceso.<br/>
         *              2) S: incluir la capa en el proceso de extracción de la información.<br/>
         *              3) N: no incluir la capa en el proceso de extracción de la información.<br/>
         *              
         * */        
        private static String IncluirCapa(dwgCapa c)
        {
            String incluir = "";
            while (incluir == "")
            {
                PromptStringOptions pStrOpts = new PromptStringOptions("\nIntroduzca si desea procesar la capa " + c.nombreCapa + "(" + c.objectId.ToString() + ") (s/n): ");
                PromptResult pStrRes = ed.GetString(pStrOpts);
                if (pStrRes.Status == PromptStatus.Cancel)
                {
                    incluir = "C";
                    break;
                }
                else if (pStrRes.Status == PromptStatus.OK)
                {
                    incluir = pStrRes.StringResult;
                    if ((incluir == "S") || (incluir == "s"))
                    {
                        break;
                    }
                    else if ((incluir == "N") || (incluir == "n"))
                    {
                        break;
                    }
                    else
                    {
                        incluir = "";
                    }
                }
            }

            return incluir;
        }
    }    
}