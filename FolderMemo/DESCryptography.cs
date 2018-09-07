using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security;
using System.Security.Cryptography;

namespace FolderMemo
{
    public partial class DESCryptography
    {
        private static DESCryptography m_descryptography;
        private static object synchroot = new object();

        /// <summary>
        /// Instance
        /// </summary>
        public static DESCryptography getInstance
        {
            get
            {
                lock (synchroot)
                {
                    if (m_descryptography == null)
                    {
                        m_descryptography = new DESCryptography();
                    }
                    return m_descryptography;
                }
            }
        }

        //암호화키(영문,숫자 8자리 이하)
        private string m_desKey = string.Empty;

        ///// <summary>
        ///// 암호화키를 받지 않는 생성자
        ///// </summary>
        //public DESCryptography()
        //{

        //}
        ///// <summary>
        ///// 암호화키를 받는 생성자
        ///// </summary>
        ///// <param name="deskey">암호화키(영문,숫자 8자리 이하)</param>
        //public DESCryptography(string deskey)
        //{
        //    setDesKey(deskey);
        //}

        /// <summary>
        /// DESkey 지정
        /// </summary>
        /// <param name="desky">암호화키(영문,숫자 8자리 이하)</param>
        public void setDesKey(string desky)
        {
            if (desky.Length > 8)
            {
                throw(new Exception("Key length must be 8 byte or less"));
            }

            StringBuilder sb = new StringBuilder();
            sb.Append(desky);
            int paddingSize = desky.Length % 8;
            for (int i = paddingSize; i > 0; i--)
            {
                sb.Append(" ");
            }
            m_desKey = sb.ToString();
        }

        // Public Function
        /// <summary>
        /// 문자열 암호화
        /// </summary>
        public string DESEncrypt(string inStr)
        {
            return DesEncrypt(inStr, m_desKey);
        }

        //문자열 암호화
        private string DesEncrypt(string str, string key)
        {
            //키 유효성 검사
            byte[] btKey = ConvertStringToByteArrayA(key);

            //키가 8Byte가 아니면 예외발생
            if (btKey.Length != 8)
            {
                throw (new Exception("Invalid key. Key length must be 8 byte."));
            }

            //소스 문자열
            byte[] btSrc = ConvertStringToByteArray(str);
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();

            des.Key = btKey;
            des.IV = btKey;

            ICryptoTransform desencrypt = des.CreateEncryptor();

            MemoryStream ms = new MemoryStream();

            CryptoStream cs = new CryptoStream(ms, desencrypt,
             CryptoStreamMode.Write);

            cs.Write(btSrc, 0, btSrc.Length);
            cs.FlushFinalBlock();


            byte[] btEncData = ms.ToArray();

            return (ConvertByteArrayToStringB(btEncData));
        }//end of func DesEncrypt

        // Public Function
        /// <summary>
        /// 문자열 복호화
        /// </summary>
        public string DESDecrypt(string inStr) // 복호화
        {
            return DesDecrypt(inStr, m_desKey);
        }

        //문자열 복호화
        private string DesDecrypt(string str, string key)
        {
            //키 유효성 검사
            byte[] btKey = ConvertStringToByteArrayA(key);

            //키가 8Byte가 아니면 예외발생
            if (btKey.Length != 8)
            {
                throw (new Exception("Invalid key. Key length must be 8 byte."));
            }


            byte[] btEncData = ConvertStringToByteArrayB(str);
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();

            des.Key = btKey;
            des.IV = btKey;

            ICryptoTransform desdecrypt = des.CreateDecryptor();

            MemoryStream ms = new MemoryStream();

            CryptoStream cs = new CryptoStream(ms, desdecrypt,
             CryptoStreamMode.Write);

            cs.Write(btEncData, 0, btEncData.Length);

            cs.FlushFinalBlock();

            byte[] btSrc = ms.ToArray();


            return (ConvertByteArrayToString(btSrc));

        }//end of func DesDecrypt

        #region convert
        //문자열->유니코드 바이트 배열
        private static Byte[] ConvertStringToByteArray(String s)
        {
            return (new UnicodeEncoding()).GetBytes(s);
        }

        //유니코드 바이트 배열->문자열
        private static string ConvertByteArrayToString(byte[] b)
        {
            return (new UnicodeEncoding()).GetString(b, 0, b.Length);
        }

        //문자열->안시 바이트 배열
        private static Byte[] ConvertStringToByteArrayA(String s)
        {
            return (new ASCIIEncoding()).GetBytes(s);
        }

        //안시 바이트 배열->문자열
        private static string ConvertByteArrayToStringA(byte[] b)
        {
            return (new ASCIIEncoding()).GetString(b, 0, b.Length);
        }

        //문자열->Base64 바이트 배열
        private static Byte[] ConvertStringToByteArrayB(String s)
        {
            return Convert.FromBase64String(s);
        }

        //Base64 바이트 배열->문자열
        private static string ConvertByteArrayToStringB(byte[] b)
        {
            return Convert.ToBase64String(b);
        }
        #endregion
    }
}
