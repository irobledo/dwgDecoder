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
	  
            foreach (KeyValuePair<ObjectId,dwgCapa> pair in dwgf.dwgCapas)
	        {
                XmlElement capa = xmldoc.CreateElement("capa");
                xmlattribute = xmldoc.CreateAttribute("id");
                xmlattribute.Value = pair.Value.objectId.ToString();
                capa.Attributes.Append(xmlattribute);

                xmlattribute = xmldoc.CreateAttribute("name");
                xmlattribute.Value = pair.Value.nombreCapa;
                capa.Attributes.Append(xmlattribute);

                xmlelement2.AppendChild(capa);

                var polycapa = dwgf.dwgPolylineas.Values.Where(x => x.capaId == pair.Value.objectId);
                
                foreach (dwgPolylinea obj in polycapa)
                {
                    XmlElement linea = xmldoc.CreateElement("linea");
                    xmlattribute = xmldoc.CreateAttribute("id");
                    xmlattribute.Value = obj.objId.ToString();
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
