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
                    XmlElement punto = xmldoc.CreateElement("punto");
                    xmlattribute = xmldoc.CreateAttribute("id");
                    xmlattribute.Value = obj.objId.ToString();
                    punto.Attributes.Append(xmlattribute);
                
                    xmlattribute = xmldoc.CreateAttribute("coord_x");
                    xmlattribute.Value = obj.coordenadas.X.ToString();
                    punto.Attributes.Append(xmlattribute);

                    xmlattribute = xmldoc.CreateAttribute("coord_y");
                    xmlattribute.Value = obj.coordenadas.Y.ToString();
                    punto.Attributes.Append(xmlattribute);

                    capa.AppendChild(punto);
                }

                var puntolinea = dwgf.dwgLineas.Values.Where(x => x.capaId.ToString() == cap.objectId.ToString());

                foreach (dwgLinea obj in puntolinea)
                {
                    XmlElement linea = xmldoc.CreateElement("linea");
                    xmlattribute = xmldoc.CreateAttribute("id");
                    xmlattribute.Value = obj.objId.ToString();
                    linea.Attributes.Append(xmlattribute);

                    xmlattribute = xmldoc.CreateAttribute("p1_id");
                    xmlattribute.Value = obj.p_origen.objId.ToString();
                    linea.Attributes.Append(xmlattribute);

                    xmlattribute = xmldoc.CreateAttribute("p2_id");
                    xmlattribute.Value = obj.p_final.objId.ToString();
                    linea.Attributes.Append(xmlattribute);

                    xmlattribute = xmldoc.CreateAttribute("ancho");
                    xmlattribute.Value = "";
                    linea.Attributes.Append(xmlattribute);

                    capa.AppendChild(linea);
                }
            }
            try
            {
                xmldoc.Save("c:\\prueba.xml");
            }
            catch (Exception)
            {

            }

            
            
	
	      /*
          
		
              // *********************************************
              // NODO HIJO: lineas / puntos
              // *********************************************
              // Para cada linea en la capa generamos los elementos de los puntos
              // y los elementos de las propias lineas.
		
              for (Object linea: c.lineas) {
		
                  Element punto1 = doc.createElement("punto");
                  Element mapa_atributos = doc.createElement("mapa_atributos");
                  punto1.appendChild(mapa_atributos);
                  Attr attr1 = doc.createAttribute("id");
                  Attr attr2 = doc.createAttribute("coord_x");
                  Attr attr3 = doc.createAttribute("coord_y");
                  attr1.setValue(linea.p1.pointId);
                  attr2.setValue(linea.p1.x.toString());
                  attr3.setValue(linea.p1.y.toString());
                  punto1.setAttributeNode(attr1);
                  punto1.setAttributeNode(attr2);
                  punto1.setAttributeNode(attr3);
                  capa.appendChild(punto1);
			
                  Element punto2 = doc.createElement("punto");
                  Element mapa_atributos2 = doc.createElement("mapa_atributos");
                  punto2.appendChild(mapa_atributos2);
                  Attr attr1b = doc.createAttribute("id");
                  Attr attr2b = doc.createAttribute("coord_x");
                  Attr attr3b = doc.createAttribute("coord_y");
                  attr1b.setValue(linea.p2.pointId);
                  attr2b.setValue(linea.p2.x.toString());
                  attr3b.setValue(linea.p2.y.toString());
                  punto2.setAttributeNode(attr1b);
                  punto2.setAttributeNode(attr2b);
                  punto2.setAttributeNode(attr3b);
                  capa.appendChild(punto2);
			
                  Element eltoLinea = doc.createElement("linea");
                  Element mapa_atributos = doc.createElement("mapa_atributos");
                  eltoLinea.appendChild(mapa_atributos);
                  Attr attr1 = doc.createAttribute("id");
                  attr1.setValue(linea.lineId);
                  Attr attr2 = doc.createAttribute("ancho");
                  attr2.setValue("5");
                  Attr attr3 = doc.createAttribute("p1_id");
                  attr3.setValue(linea.p1.pointId);
                  Attr attr4 = doc.createAttribute("p2_id");
                  attr4.setValue(linea.p2.pointId);
                  eltoLinea.setAttributeNode(attr1);
                  eltoLinea.setAttributeNode(attr2);
                  eltoLinea.setAttributeNode(attr3);
                  eltoLinea.setAttributeNode(attr4);
                  capa.appendChild(eltoLinea);
              }
          }
	
          // VOLCAMOS LA SALIDA AL FICHERO XML.
          TransformerFactory transformerFactory = TransformerFactory.newInstance();
          Transformer transformer = transformerFactory.newTransformer();
          transformer.setOutputProperty(OutputKeys.INDENT, "yes");
          DOMSource source = new DOMSource(doc);
          StreamResult result = new StreamResult(new File(fileName));
          transformer.transform(source, result);
          */
            return;
        }
    }
}
