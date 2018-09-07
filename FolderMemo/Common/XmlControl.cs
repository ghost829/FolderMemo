using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace FolderMemo.Common
{
    class XmlControl
    {
        private static XmlControl xmlcontrol;

        private XmlControl()
        {

        }

        public static XmlControl getInstance()
        {
            if (xmlcontrol == null)
                xmlcontrol = new XmlControl();
            return xmlcontrol;
        }


        public XmlDocument xmlLoad(string xmlPath)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(xmlPath);
                return doc;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return doc;
            }
        }

        public XmlNodeList xmlLoad(string xmlPath, string xPath)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(xmlPath);
                XmlNodeList node = doc.SelectNodes(xPath);
                return node;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool xmlSave(XmlDocument doc, string xmlPath)
        {
            doc.Save(xmlPath);
            return true;
        }

        public bool xmlSave(string xmlString, string xmlPath)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlString);
            doc.Save(xmlPath);
            return true;
        }

        public bool exportFormXml(string path, string xmlString)
        {
            if (this.xmlSave(xmlString, path))
                return true;

            return false;
        }

    }//END Class
}//END Namespace
