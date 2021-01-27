using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading; // Thread
using System.Diagnostics; // Progress
using System.IO; // File, Stream
using System.Net; // WebRequest
using System.Windows.Forms;
using System.Drawing.Drawing2D; // Brush
using System.Xml; // XMlDocument
using System.Runtime.InteropServices; // StructLayout, DllImport

namespace FolderMemoUpdate
{
    public partial class Form1 : Form
    {
        // 버전확인용URL
        private const string m_str_server_url = "https://raw.githubusercontent.com/ghost829/FolderMemo/master/Publish";
        private const string m_str_verion_file_name = "version.xml";
        private XmlDocument m_doc_version = new XmlDocument();
        private List<string> m_arr_str_fileName = new List<string>(); // 파일목록

        // CustomForm
        private int m_radius = 10; // 폼의 꼭짓점 뭉툭함 지정
        private RectangleCorners m_vertexDirection = RectangleCorners.All; // 꼭짓점 뭉툭할 방향지정

        private delegate void Delegate_string(string text);
        private delegate void Delegate_int(int value);
        private delegate void Delegate_void();
        public enum RectangleCorners
        {
            None = 0, TopLeft = 1, TopRight = 2, BottomLeft = 4, BottomRight = 8,
            All = TopLeft | TopRight | BottomLeft | BottomRight,
            Top = TopLeft | TopRight, Bottom = BottomLeft | BottomRight
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //SetStyle(ControlStyles.DoubleBuffer, true);
            //SetStyle(ControlStyles.UserPaint, true);
            //SetStyle(ControlStyles.AllPaintingInWmPaint, true);

            this.Icon = Properties.Resources.stick_note;
            this.FormBorderStyle = FormBorderStyle.None;
            this.CenterToScreen();
            this.Refresh();

            Thread trd = new Thread(new ThreadStart(thread_check_worker));
            trd.IsBackground = true;
            trd.Start();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // 꼭짓점 뭉툭함 지정
            if (m_radius > 0)
            {
                GraphicsPath thisRegion = Create(0, 0, this.Width, this.Height, m_radius, m_vertexDirection);
                this.Region = new Region(thisRegion);
                thisRegion.Dispose();
            }
            else
            {
                this.Region = new Region(this.ClientRectangle);
            }

            Graphics graphics = e.Graphics;
            Rectangle gradient_rectangle = new Rectangle(0, 0, this.Width, this.Height);
            Color color1 = Color.FromArgb(0x77fffc00);
            Color color2 = Color.FromArgb(0x77ffffff);
            Brush b = new LinearGradientBrush(gradient_rectangle, color1, color2, 65f);
            graphics.FillRectangle(b, gradient_rectangle);

            base.OnPaint(e);
        }

        #region  ## PUBLIC STATIC_CREATE_GraphicsPath
        public static GraphicsPath Create(int x, int y, int width, int height,
                                              int radius, RectangleCorners corners)
        {
            int xw = x + width;
            int yh = y + height;
            int xwr = xw - radius;
            int yhr = yh - radius;
            int xr = x + radius;
            int yr = y + radius;
            int r2 = radius * 2;
            int xwr2 = xw - r2;
            int yhr2 = yh - r2;

            GraphicsPath p = new GraphicsPath();
            p.StartFigure();

            //Top Left Corner
            if ((RectangleCorners.TopLeft & corners) == RectangleCorners.TopLeft)
            {
                p.AddArc(x, y, r2, r2, 180, 90);
            }
            else
            {
                p.AddLine(x, yr, x, y);
                p.AddLine(x, y, xr, y);
            }

            //Top Edge
            p.AddLine(xr, y, xwr, y);

            //Top Right Corner
            if ((RectangleCorners.TopRight & corners) == RectangleCorners.TopRight)
            {
                p.AddArc(xwr2, y, r2, r2, 270, 90);
            }
            else
            {
                p.AddLine(xwr, y, xw, y);
                p.AddLine(xw, y, xw, yr);
            }

            //Right Edge
            p.AddLine(xw, yr, xw, yhr);

            //Bottom Right Corner
            if ((RectangleCorners.BottomRight & corners) == RectangleCorners.BottomRight)
            {
                p.AddArc(xwr2, yhr2, r2, r2, 0, 90);
            }
            else
            {
                p.AddLine(xw, yhr, xw, yh);
                p.AddLine(xw, yh, xwr, yh);
            }

            //Bottom Edge
            p.AddLine(xwr, yh, xr, yh);

            //Bottom Left Corner
            if ((RectangleCorners.BottomLeft & corners) == RectangleCorners.BottomLeft)
            {
                p.AddArc(x, yhr2, r2, r2, 90, 90);
            }
            else
            {
                p.AddLine(xr, yh, x, yh);
                p.AddLine(x, yh, x, yhr);
            }

            //Left Edge
            p.AddLine(x, yhr, x, yr);

            p.CloseFigure();
            return p;
        }
        #endregion

        /// <summary>
        /// 백그라운드에서 업데이트 수행
        /// </summary>
        private void thread_check_worker()
        {
            // 1. 프로세스 검사 => 현재 실행시 종료
            label1_setText("프로세스 검사");
            progressLabel1_setValue(1);
            kill_process_folderMemo();
            progressLabel1_setValue(5);
            
            // 2. 서버버전 read => m_doc_version에 담기
            label1_setText("버전파일 확인");
            read_versionFileDoc();
            progressLabel1_setValue(10);

            // 3. 로컬파일 검사 => 파일 존재하는지 검사, 없으면 다운로드
            label1_setText("로컬파일 검사");
            fileDownload_ifNotExist();
            progressLabel1_setValue(50);
            
            // 4. 최신버전 아니면 파일 교체
            label1_setText("최신버전확인");
            fileVersion_check();

            // 5. 완료
            label1_setText("버전확인 완료");
            progressLabel1_setValue(100);
            runFolderMemoAndUpdateClose();
        }

        #region ※ 폼 GUI 조작
        /// <summary>
        /// 라벨 텍스트 설정
        /// </summary>
        /// <param name="text"></param>
        private void label1_setText(string text)
        {
            if (label1.InvokeRequired)
            {
                var d = new Delegate_string(label1_setText);
                Invoke(d, new object[] { text });
            }
            else
            {
                label1.Text = text;
            }
        }

        /// <summary>
        /// 프로그레스바 값 변경
        /// </summary>
        /// <param name="value"></param>
        private void progressLabel1_setValue(int value)
        {
            if (progressLabel1.InvokeRequired)
            {
                var d = new Delegate_int(progressLabel1_setValue);
                Invoke(d, new object[] {value});
            }
            else
            {
                progressLabel1.Value = value;
            }
        }

        /// <summary>
        /// Update 폼 닫기
        /// </summary>
        private void close_form()
        {
            if (this.InvokeRequired)
            {
                var d = new Delegate_void(close_form);
                Invoke(d, new object[] { });
            }
            else
            {
                this.Close();
            }
        }
        #endregion

        /// <summary>
        /// 폴더메모 실행중이면 프로세스 닫기 수행
        /// </summary>
        private void kill_process_folderMemo()
        {
            try
            {
                string target_name = "FolderMemo";
                Process[] processList = Process.GetProcessesByName(target_name);
                //while (processList.Length < 1) // FolderMemo 실행될때까지 대기
                //{
                //    processList = Process.GetProcessesByName(target_name);
                //}
                if( processList.Length > 0)
                {
                    label1_setText("프로세스 찾음 => 제거");
                }
                while (processList.Length > 0)
                {
                    if (!processList[0].CloseMainWindow())
                    {
                        // 안닫히면 강제종료
                        processList[0].Kill();
                    }
                    processList = Process.GetProcessesByName(target_name);
                    
                }
                //if (processList.Length > 0)
                //{
                //    label1_setText("프로세스 찾음 => 제거");
                //    // Kill을 수행하면 시스템 트레이에 아이콘이 남음, CloseMainWindow 수행!
                //    if ( !processList[0].CloseMainWindow() )
                //    {
                //        // 안닫히면 강제종료
                //        processList[0].Kill(); 
                //    }
                //}
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
            finally
            {
                
            }
        }

        /// <summary>
        /// Web에 있는 Version파일 읽어오기
        /// </summary>
        private void read_versionFileDoc()
        {
            // 210128 Github version.xml 읽어오기 실패!! SSL/TLS 보안 채널을 만들 수 없습니다.
            // 해결법 ( 출처: https://shared.co.kr/183 [Life is but a dream] )

            ServicePointManager.Expect100Continue = true;

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Ssl3; // 210128 .net Framework 4.0에는 Tls11,Tls12 없음

            // Skip validation of SSL/TLS certificate
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            try{
                string str_server_version_url = m_str_server_url + "/" + m_str_verion_file_name;
                WebRequest webReq_version = WebRequest.Create(str_server_version_url);
                webReq_version.Method = "GET";

                WebResponse respon = webReq_version.GetResponse();
                Stream objStream = respon.GetResponseStream();
                StreamReader objReader = new StreamReader(objStream);
                String str_doc_version = objReader.ReadToEnd();
                objReader.Close();
                objStream.Close();
                m_doc_version.LoadXml(str_doc_version);
            }
            catch(Exception ex){
                alert_msg(ex.Message + "\n" + ex.StackTrace, MessageBoxButtons.OK);
            }
            
        }

        private void alert_msg(String msg, MessageBoxButtons nButtonType) {
            DialogResult result = MessageBox.Show(msg, "잠깐만요!!", nButtonType);
            if ( nButtonType == MessageBoxButtons.OKCancel && result == DialogResult.Cancel) {
                this.close_form();
                return;
            }
            else if ( nButtonType == MessageBoxButtons.OK)
            {
                this.close_form();
                return;
            }
        }
        
        /// <summary>
        /// 파일존재하지 않을경우 다운로드 수행
        /// </summary>
        private void fileDownload_ifNotExist()
        {
            try
            {
                string file_path = AppDomain.CurrentDomain.BaseDirectory;
                DirectoryInfo dirInfo = new DirectoryInfo(file_path);
                FileInfo[] local_files = dirInfo.GetFiles();
                // 파일검사
                List<bool> arr_b_fileExist = new List<bool>(); // 파일 존재 시 true
                List<string> arr_str_fileName = new List<string>();
                List<string> arr_str_url_filePath = new List<string>();

                XmlNodeList list_FILE_INFO = m_doc_version.SelectNodes("//FILE_INFO");
                foreach(XmlNode fileInfo in list_FILE_INFO)
                {
                    var dir = fileInfo.Attributes["dir"];
                    label1_setText(String.Format("파일 체크 리스트 확인 {0}", dir.Value));
                    XmlNodeList list_ITEM = fileInfo.SelectNodes(".//ITEM");
                    
                    // 파일체크 리스트 수집
                    foreach (XmlNode item in list_ITEM)
                    {
                        arr_b_fileExist.Add(false);
                        arr_str_fileName.Add(item.InnerText);
                        arr_str_url_filePath.Add(String.Format("{0}/{1}/{2}",m_str_server_url, dir.Value, item.InnerText));
                    }
                }

                m_arr_str_fileName = arr_str_fileName;


                // 로컬에 파일 존재여부 검사
                for (int i = 0; i < local_files.Length; i++)
                {
                    string file_name = local_files[i].Name;
                    for (int j = 0; j < arr_str_url_filePath.Count; j++)
                    {
                        if (arr_str_fileName[j] == file_name)
                        {
                            arr_b_fileExist[j] = true;

                            // 특수조건 = readme.txt는 항상 업데이트
                            if (file_name == "readme.txt")
                            {
                                arr_b_fileExist[j] = false;
                            }
                            break;
                        }
                    }
                }

                // 파일이 하나도 없음, 최초설치임, 파일 정말로 다운로드 할건지 묻기(파일들을 다 다운받으므로 디렉토리 Dirty해짐)
                if (!arr_b_fileExist.Contains(true))
                {
                    string msg = String.Format("관련파일이 하나도 없습니다.\n최초설치로 보여지며 현재 디렉토리에 모든 파일을 다운로드 하려합니다.\n여기에 정말 다운로드 하시겠습니까?\n현재경로:{0}\n다른곳에 다운받기 원하시면 취소버튼을 누른 후 이 파일을 다른 디렉토리로 옮겨주세요"
                        , AppDomain.CurrentDomain.BaseDirectory);
                    DialogResult result = MessageBox.Show(msg, "잠깐만요!!", MessageBoxButtons.OKCancel);
                    if (result == DialogResult.Cancel)
                    {
                        this.close_form();
                        return;
                    }
                }

                label1_setText(String.Format("로컬파일 검사 {0}/{1}", 0, arr_b_fileExist.Count));
                // 없는파일 다운로드
                for (int i = 0; i < arr_b_fileExist.Count; i++)
                {
                    label1_setText(String.Format("로컬파일 검사 {0}/{1}", i + 1, arr_b_fileExist.Count));

                    // 특수조건 = readme.txt는 항상 업데이트
                    if (arr_str_fileName[i] == "readme.txt")
                    {
                        label1_setText(String.Format("로컬파일 다운로드 {0}/{1}", i + 1, arr_b_fileExist.Count));
                        string filePath = arr_str_url_filePath[i];

                        HttpWebRequest webReq_file = (HttpWebRequest)WebRequest.Create(filePath);
                        webReq_file.Method = "GET";
                        webReq_file.Timeout = 30 * 1000; // 30초

                        var res = webReq_file.GetResponse();

                        //Console.WriteLine("{0} - {1} - {2}", fileName, res.ContentType, res.ContentLength);
                        Stream objStream = res.GetResponseStream();
                        StreamReader readStream = new StreamReader(objStream);
                        string str_contents = readStream.ReadToEnd();

                        // newLine이 메모장앱에서 제대로 안보임
                        var allText = str_contents.Split(new string[] { "\n" }, StringSplitOptions.None);
                        StreamWriter writeStream = new StreamWriter(arr_str_fileName[i], false);
                        writeStream.Flush();
                        for (var j = 0; j < allText.Count(); j++)
                        {
                            string line = allText[j];
                            writeStream.WriteLine(line);
                        }
                        writeStream.Close();
                        readStream.Close();
                        objStream.Close();
                    }
                    else if (!arr_b_fileExist[i])
                    {
                        label1_setText(String.Format("로컬파일 다운로드 {0}/{1}", i + 1, arr_b_fileExist.Count));
                        string filePath = arr_str_url_filePath[i];
                        
                        HttpWebRequest webReq_file = (HttpWebRequest)WebRequest.Create(filePath);
                        webReq_file.Method = "GET";
                        webReq_file.Timeout = 30 * 1000; // 30초

                        var res = webReq_file.GetResponse();

                        //Console.WriteLine("{0} - {1} - {2}", fileName, res.ContentType, res.ContentLength);
                        Stream objStream = res.GetResponseStream();
                        FileStream fileWriteStream = new FileStream(arr_str_fileName[i], FileMode.CreateNew);
                        int byteRead;
                        while ((byteRead = objStream.ReadByte()) != -1)
                        {
                            fileWriteStream.WriteByte((byte)byteRead);
                        }
                        fileWriteStream.Close();
                        objStream.Close();

                        Console.WriteLine("Downloaded file saved in the following file system folder:\n\t" + Application.StartupPath);
                    }
                }

                label1_setText("로컬파일 검사 완료");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {

            }
        }
        
        /// <summary>
        /// 파일 버전 체크
        /// </summary>
        private void fileVersion_check()
        {
            XmlNode node_version = m_doc_version.SelectSingleNode("//VERSION");
            string str_version = node_version.InnerText;
            var server_version = new Version(str_version); // 서버버전

            foreach (string fileName in m_arr_str_fileName)
            {
                if (fileName == "FolderMemo.exe") {
                    Console.WriteLine(fileName);
                    Console.WriteLine(Path.GetExtension(fileName));
                    FileVersionInfo fileVersion = FileVersionInfo.GetVersionInfo(fileName);
                    Console.WriteLine(fileVersion.ProductVersion);

                    var local_version = new Version(fileVersion.ProductVersion);

                    var result = server_version.CompareTo(local_version);
                    if (result > 0)
                    {
                        label1_setText("서버 버전이 최신임을 확인 - 파일 교체 준비");
                        Thread.Sleep(1000);
                        foreach (string tmpFileName in m_arr_str_fileName)
                        {
                            // 파일 삭제후 재 다운로드 수행
                            if( Path.GetExtension(tmpFileName).Contains("exe")
                                || Path.GetExtension(tmpFileName).Contains("dll"))
                            {
                                File.Delete(tmpFileName);
                            }
                        }
                        fileDownload_ifNotExist();
                    }
                }
            }
        }

        /// <summary>
        /// 폴더메모 앱 실행 및 업데이트 프로그램 종료
        /// </summary>
        private void runFolderMemoAndUpdateClose()
        {
            // 0.1초후 폴더메모 실행
            Thread.Sleep(100);
            ProcessStartInfo info = new ProcessStartInfo("FolderMemo.exe");
            info.CreateNoWindow = true;
            info.UseShellExecute = false;
            //Process.Start("FolderMemo.exe");
            Process.Start(info);
            this.close_form();
            //this.TopRight_CloseBtnVisible = true;
        }
    }
}
