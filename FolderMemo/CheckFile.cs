using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace FolderMemo
{
    class CheckFile
    {
        private static CheckFile m_checkfile;
        private static object locksync = new object();

        public static CheckFile instance
        {
            get
            {
                lock (locksync)
                {
                    if (m_checkfile == null)
                        m_checkfile = new CheckFile();
                }
                return m_checkfile;
            }
        }

        /// <summary>
        /// 파일 정보 체크
        /// </summary>
        /// <param name="configPath">config.xml 파일 경로</param>
        /// <param name="dataPath">data.xml 파일 경로</param>
        /// <returns></returns>
        public bool fileInfoCheck(String configPath, String dataPath)
        {
            XmlDocument doc = new XmlDocument();

            /* 1. config.xml파일 확인 */
            doc.Load(configPath);


            XmlNode settingNode = doc.SelectSingleNode("//SETTING");
            if (settingNode.SelectSingleNode("./" + DEFINE.CONFIG_SETTING_TOPMOST) == null)
                return false;
            XmlNode dataPathNode = settingNode.SelectSingleNode("./" + DEFINE.CONFIG_SETTING_MEMODATAPATH);
            if (dataPathNode == null)
            {
                XmlNode tmpNode = doc.CreateNode(XmlNodeType.Element, DEFINE.CONFIG_SETTING_MEMODATAPATH, "");
                settingNode.AppendChild(tmpNode);
                doc.Save(configPath);
            }

            /* 2. data.xml파일 확인 */
            doc.Load(dataPath);

            
            return true;
        }
    }
}
