using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

using Autodesk.AutoCAD.DatabaseServices;

using fi.upm.es.dwgDecoder.dwgElementos;

namespace fi.upm.es.dwgDecoder.tools
{
    public static class exportXml
    {
        public static void export2Xml(dwgFile dwgf)
        {
            XmlDocument xmldoc = new XmlDocument();


            // *********************************************
            // Elemento raiz. PLANO
            // *********************************************
	
            XmlNode xmlelement = xmldoc.CreateElement("plano");
            
            XmlAttribute xmlattribute = xmldoc.CreateAttribute("nombre_fichero");
            xmlattribute.Value = "";
            xmlelement.Attributes.Append(xmlattribute);

            xmlattribute = xmldoc.CreateAttribute("fecha_fichero");
            xmlattribute.Value = "";
            xmlelement.Attributes.Append(xmlattribute);

            xmlattribute = xmldoc.CreateAttribute("fecha_procesamiento");
            xmlattribute.Value = "";
            xmlelement.Attributes.Append(xmlattribute);

            xmldoc.AppendChild(xmlelement);

            // *********************************************
            // NODO HIJO: elemento_simulacion
            // *********************************************

            XmlElement xmlelement2 = xmldoc.CreateElement("elemento_simulacion");

            xmlattribute = xmldoc.CreateAttribute("tipo_elemento");
            xmlattribute.Value = "elemento_original_mapa";
            xmlelement2.Attributes.Append(xmlattribute);

            xmlelement.AppendChild(xmlelement2);
            
            // *********************************************
            // NODO HIJO: capa
            // *********************************************
	  
            foreach (dwgCapa cap in dwgf.dwgCapas.Values)
	        {
                XmlElement capa = xmldoc.CreateElement("capa");
                xmlattribute = xmldoc.CreateAttribute("id");
                xmlattribute.Value = cap.objectId.ToString();
                capa.Attributes.Append(xmlattribute);

                xmlattribute = xmldoc.CreateAttribute("name");
                xmlattribute.Value = cap.nombreCapa;
                capa.Attributes.Append(xmlattribute);

                xmlelement2.AppendChild(capa);

                var puntocapa = dwgf.dwgPuntos.Values.Where(x => x.capaId.ToString() == cap.objectId.ToString());
                          
                foreach (dwgPunto obj in puntocapa)
                {
                    XmlElement punto = exportXml.punto2xml(obj, xmldoc);
                    capa.AppendChild(punto);
                }

                var puntolinea = dwgf.dwgLineas.Values.Where(x => x.capaId.ToString() == cap.objectId.ToString() && x.parentId.ToString() == "(0)");
                
                foreach (dwgLinea obj in puntolinea)
                {
                    XmlElement linea = exportXml.linea2xml (obj, xmldoc); 
                    capa.AppendChild(linea);
                }

                var puntopolylinea = dwgf.dwgPolylineas.Values.Where(x => x.capaId.ToString() == cap.objectId.ToString());

                foreach (dwgPolylinea obj in puntopolylinea)
                {
                    XmlElement polylinea = exportXml.polylinea2xml(obj, xmldoc, cap.objectId, dwgf);
                    capa.AppendChild(polylinea);
                }

                var puntoarco = dwgf.dwgArcos.Values.Where(x => x.capaId.ToString() == cap.objectId.ToString());

                foreach (dwgArco obj in puntoarco)
                {
                    XmlElement arco = exportXml.arco2xml(obj, xmldoc, cap.objectId, dwgf);
                    capa.AppendChild(arco);
                }
            }
            try
            {
                xmldoc.Save("c:\\prueba.xml");
            }
            catch (Exception)
            {

            }            
            return;
        }

        public static XmlElement punto2xml(dwgPunto p, XmlDocument xmldoc)
        {
            XmlElement punto = xmldoc.CreateElement("punto");
            XmlAttribute xmlattribute = xmldoc.CreateAttribute("id");
            xmlattribute.Value = p.objId.ToString();
            punto.Attributes.Append(xmlattribute);

            xmlattribute = xmldoc.CreateAttribute("coord_x");
            xmlattribute.Value = p.coordenadas.X.ToString();
            punto.Attributes.Append(xmlattribute);

            xmlattribute = xmldoc.CreateAttribute("coord_y");
            xmlattribute.Value = p.coordenadas.Y.ToString();
            punto.Attributes.Append(xmlattribute);
            
            return punto;
        }

        public static XmlElement linea2xml(dwgLinea l, XmlDocument xmldoc)
        {
            XmlElement linea = xmldoc.CreateElement("linea");
            XmlAttribute xmlattribute = xmldoc.CreateAttribute("id");
            xmlattribute.Value = l.objId.ToString();
            linea.Attributes.Append(xmlattribute);

            xmlattribute = xmldoc.CreateAttribute("p1_id");
            xmlattribute.Value = l.p_origen.objId.ToString();
            linea.Attributes.Append(xmlattribute);

            xmlattribute = xmldoc.CreateAttribute("p2_id");
            xmlattribute.Value = l.p_final.objId.ToString();
            linea.Attributes.Append(xmlattribute);

            xmlattribute = xmldoc.CreateAttribute("ancho");
            xmlattribute.Value = "";
            linea.Attributes.Append(xmlattribute);

            xmlattribute = xmldoc.CreateAttribute("parent_id");
            xmlattribute.Value = l.parentId.ToString();
            linea.Attributes.Append(xmlattribute);

            return linea;
        }

        public static XmlElement polylinea2xml(dwgPolylinea p, XmlDocument xmldoc, ObjectId capaId, dwgFile dwgf)
        {
            XmlElement polylinea = xmldoc.CreateElement("polylinea");
            XmlAttribute xmlattribute = xmldoc.CreateAttribute("id");
            xmlattribute.Value = p.objId.ToString();
            polylinea.Attributes.Append(xmlattribute);

            var puntolinea2 = dwgf.dwgLineas.Values.Where(x => x.capaId.ToString() == capaId.ToString() && x.parentId.ToString() == p.objId.ToString());

            foreach (dwgLinea obj2 in puntolinea2)
            {
                XmlElement linea = exportXml.linea2xml(obj2, xmldoc);
                polylinea.AppendChild(linea);
            }

            return polylinea;
        }

        public static XmlElement arco2xml(dwgArco a, XmlDocument xmldoc, ObjectId capaId, dwgFile dwgf)
        {
            XmlElement arco = xmldoc.CreateElement("arco");
            XmlAttribute xmlattribute = xmldoc.CreateAttribute("id");
            xmlattribute.Value = a.objId.ToString();
            arco.Attributes.Append(xmlattribute);

            var puntolinea2 = dwgf.dwgLineas.Values.Where(x => x.capaId.ToString() == capaId.ToString() && x.parentId.ToString() == a.objId.ToString());

            foreach (dwgLinea obj2 in puntolinea2)
            {
                XmlElement linea = exportXml.linea2xml(obj2, xmldoc);
                arco.AppendChild(linea);
            }

            return arco;
        }

    }
}
