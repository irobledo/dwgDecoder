using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

using Autodesk.AutoCAD.DatabaseServices;

using fi.upm.es.dwgDecoder.dwgElementos;

namespace fi.upm.es.dwgDecoder.tools
{
    public static class exportXml
    {
        public static void serializar(dwgFile dwgf, String ruta)
        {
            using (StreamWriter writer = new StreamWriter(ruta))
            {
                writer.WriteLine(" #### CAPAS #########");

                foreach (dwgCapa obj in dwgf.dwgCapas.Values)
                {
                    writer.WriteLine(obj.ToString());
                }

                writer.WriteLine(" #### PUNTOS #########");

                foreach (dwgPunto obj in dwgf.dwgPuntos.Values)
                {
                    writer.WriteLine(obj.ToString());
                }


                writer.WriteLine(" #### ARCOS #########");

                foreach (dwgArco obj in dwgf.dwgArcos.Values)
                {
                    writer.WriteLine(obj.ToString());
                }

                writer.WriteLine(" #### POLYLINEAS #########");

                foreach (dwgPolylinea obj in dwgf.dwgPolylineas.Values)
                {
                    writer.WriteLine(obj.ToString());
                }

                return;
            }
        }

        public static void export2Xml(dwgFile dwgf, String ruta)
        {
            XmlDocument xmldoc = new XmlDocument();


            // *********************************************
            // Elemento raiz. PLANO
            // *********************************************
	
            XmlNode xmlelement = xmldoc.CreateElement("plano");
            
            XmlAttribute xmlattribute = xmldoc.CreateAttribute("nombre_fichero");
            xmlattribute.Value = dwgf.nombre_fichero_original;
            xmlelement.Attributes.Append(xmlattribute);

            xmlattribute = xmldoc.CreateAttribute("fecha_fichero");
            xmlattribute.Value = "";
            xmlelement.Attributes.Append(xmlattribute);

            xmlattribute = xmldoc.CreateAttribute("fecha_procesamiento");
            xmlattribute.Value = System.DateTime.Now.ToString();
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
                XmlElement capa = exportXml.capa2xml(cap, xmldoc);
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
                xmldoc.Save(ruta);
            }
            catch (Exception)
            {

            }            
            return;
        }

        public static XmlElement capa2xml(dwgCapa c, XmlDocument xmldoc)
        {
            XmlElement capa = xmldoc.CreateElement("capa");
            XmlAttribute xmlattribute = xmldoc.CreateAttribute("id");
            xmlattribute.Value = c.objectId.ToString();
            capa.Attributes.Append(xmlattribute);

            xmlattribute = xmldoc.CreateAttribute("name");
            xmlattribute.Value = c.nombreCapa;
            capa.Attributes.Append(xmlattribute);

            XmlElement mapa = xmldoc.CreateElement("mapa_atributos");
            capa.AppendChild(mapa);

            
            XmlElement mapa_elto = xmldoc.CreateElement("atributo");
            mapa.AppendChild(mapa_elto);
            xmlattribute = xmldoc.CreateAttribute("key");
            xmlattribute.Value = "handleId";
            mapa_elto.Attributes.Append(xmlattribute);
            xmlattribute = xmldoc.CreateAttribute("valor");
            xmlattribute.Value = c.handleId.ToString();
            mapa_elto.Attributes.Append(xmlattribute);

            mapa_elto = xmldoc.CreateElement("atributo");
            mapa.AppendChild(mapa_elto);
            xmlattribute = xmldoc.CreateAttribute("key");
            xmlattribute.Value = "apagada";
            mapa_elto.Attributes.Append(xmlattribute);
            xmlattribute = xmldoc.CreateAttribute("valor");
            xmlattribute.Value = c.apagada.ToString();
            mapa_elto.Attributes.Append(xmlattribute);

            mapa_elto = xmldoc.CreateElement("atributo");
            mapa.AppendChild(mapa_elto);
            xmlattribute = xmldoc.CreateAttribute("key");
            xmlattribute.Value = "bloqueada";
            mapa_elto.Attributes.Append(xmlattribute);
            xmlattribute = xmldoc.CreateAttribute("valor");
            xmlattribute.Value = c.bloqueada.ToString();
            mapa_elto.Attributes.Append(xmlattribute);

            mapa_elto = xmldoc.CreateElement("atributo");
            mapa.AppendChild(mapa_elto);
            xmlattribute = xmldoc.CreateAttribute("key");
            xmlattribute.Value = "enUso";
            mapa_elto.Attributes.Append(xmlattribute);
            xmlattribute = xmldoc.CreateAttribute("valor");
            xmlattribute.Value = c.enUso.ToString();
            mapa_elto.Attributes.Append(xmlattribute);

            mapa_elto = xmldoc.CreateElement("atributo");
            mapa.AppendChild(mapa_elto);
            xmlattribute = xmldoc.CreateAttribute("key");
            xmlattribute.Value = "oculta";
            mapa_elto.Attributes.Append(xmlattribute);
            xmlattribute = xmldoc.CreateAttribute("valor");
            xmlattribute.Value = c.oculta.ToString();
            mapa_elto.Attributes.Append(xmlattribute);

            /*
            mapa_elto = xmldoc.CreateElement("atributo");
            mapa.AppendChild(mapa_elto);
            xmlattribute = xmldoc.CreateAttribute("key");
            xmlattribute.Value = "grueso_por_defecto_lineas";
            mapa_elto.Attributes.Append(xmlattribute);
            xmlattribute = xmldoc.CreateAttribute("valor");
            xmlattribute.Value = c.default_gruesoLinea.ToString();
            mapa_elto.Attributes.Append(xmlattribute);
            */

            return capa;
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

            XmlElement color = rgb2xml(p.color_R, p.color_G, p.color_B, xmldoc);
            punto.AppendChild(color);

            XmlElement mapa = xmldoc.CreateElement("mapa_atributos");
            punto.AppendChild(mapa);

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
            xmlattribute.Value = l.LineWeight.ToString();
            linea.Attributes.Append(xmlattribute);

            XmlElement color = rgb2xml(l.color_R, l.color_G, l.color_B, xmldoc);
            linea.AppendChild(color);

            XmlElement mapa = xmldoc.CreateElement("mapa_atributos");
            linea.AppendChild(mapa);

            /*
            xmlattribute = xmldoc.CreateAttribute("parent_id");
            xmlattribute.Value = l.parentId.ToString();
            linea.Attributes.Append(xmlattribute);
            */

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

            var puntolinea3 = dwgf.dwgArcos.Values.Where(x => x.capaId.ToString() == capaId.ToString() && x.parentId.ToString() == p.objId.ToString());

            foreach (dwgArco obj2 in puntolinea3)
            {
                XmlElement arco = exportXml.arco2xml(obj2, xmldoc, capaId, dwgf);
                polylinea.AppendChild(arco);
            }

            XmlElement mapa = xmldoc.CreateElement("mapa_atributos");
            polylinea.AppendChild(mapa);

            return polylinea;
        }

        public static XmlElement arco2xml(dwgArco a, XmlDocument xmldoc, ObjectId capaId, dwgFile dwgf)
        {
            XmlElement arco = xmldoc.CreateElement("arco");
            XmlAttribute xmlattribute = xmldoc.CreateAttribute("id");
            xmlattribute.Value = a.objId.ToString();
            arco.Attributes.Append(xmlattribute);

            xmlattribute = xmldoc.CreateAttribute("radio");
            xmlattribute.Value = a.radio.ToString();
            arco.Attributes.Append(xmlattribute);

            xmlattribute = xmldoc.CreateAttribute("angulo_inicio");
            xmlattribute.Value = a.angulo_inicio.ToString();
            arco.Attributes.Append(xmlattribute);

            xmlattribute = xmldoc.CreateAttribute("angulo_final");
            xmlattribute.Value = a.angulo_final.ToString();
            arco.Attributes.Append(xmlattribute);

            xmlattribute = xmldoc.CreateAttribute("center_point_id");
            xmlattribute.Value = a.punto_centro.ToString();
            arco.Attributes.Append(xmlattribute);

            var puntolinea2 = dwgf.dwgLineas.Values.Where(x => x.capaId.ToString() == capaId.ToString() && x.parentId.ToString() == a.objId.ToString());

            foreach (dwgLinea obj2 in puntolinea2)
            {
                XmlElement linea = exportXml.linea2xml(obj2, xmldoc);
                arco.AppendChild(linea);
            }

            XmlElement mapa = xmldoc.CreateElement("mapa_atributos");
            arco.AppendChild(mapa);

            return arco;
        }

        public static XmlElement rgb2xml(int R, int G, int B, XmlDocument xmldoc)
        {
            XmlElement rgb = xmldoc.CreateElement("color");
            
            XmlAttribute xmlattribute = xmldoc.CreateAttribute("R");
            xmlattribute.Value = R.ToString();
            rgb.Attributes.Append(xmlattribute);

            xmlattribute = xmldoc.CreateAttribute("G");
            xmlattribute.Value = G.ToString();
            rgb.Attributes.Append(xmlattribute);

            xmlattribute = xmldoc.CreateAttribute("B");
            xmlattribute.Value = B.ToString();
            rgb.Attributes.Append(xmlattribute);

            return rgb;
        }


    }
}
