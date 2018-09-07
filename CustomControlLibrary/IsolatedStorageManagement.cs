using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace CustomControlLibrary
{
    static class IsolatedStorageManagement
    {
        public static void WriteIsolated(string keyName, string data)
        {
            //System.Drawing.Rectangle rect = new System.Drawing.Rectangle();
            //System.Drawing.RectangleConverter rc = new System.Drawing.RectangleConverter();
            //rect = (System.Drawing.Rectangle)rc.ConvertFromString(data);
            IsolatedStorageFile iss = IsolatedStorageFile.GetUserStoreForDomain();
            if (!iss.FileExists(keyName))
            {
                IsolatedStorageFileStream tmpIst = iss.CreateFile(keyName);
                tmpIst.Close();
            }

            IsolatedStorageFileStream ist = iss.OpenFile(keyName, FileMode.Create, FileAccess.Write, FileShare.None);
            using (StreamWriter stw = new StreamWriter(ist))
            {
                stw.Write(data);
                stw.Close();
            }
            ist.Close();
        }

        public static string readIsolated(string keyName)
        {
            IsolatedStorageFile iss = IsolatedStorageFile.GetUserStoreForDomain();
            //DeleteIsolated(keyName);
            string readData = string.Empty;
            if (iss.FileExists(keyName))
            {
                 IsolatedStorageFileStream ist = iss.OpenFile(keyName, FileMode.Open, FileAccess.Read, FileShare.None);
                 using (StreamReader stw = new StreamReader(ist))
                 {
                     readData = stw.ReadToEnd();
                     stw.Close();
                 }
                 ist.Close();
            }
            return readData;
        }

        public static void DeleteIsolated(string keyName)
        {
            IsolatedStorageFile iss = IsolatedStorageFile.GetUserStoreForDomain();
            if (iss.FileExists(keyName))
            {
                iss.DeleteFile(keyName);
            }
        }
    }
}
