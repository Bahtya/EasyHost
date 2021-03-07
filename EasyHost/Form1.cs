using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Diagnostics;

namespace EasyHost
{
    public partial class Form1 : Form
    {
        string gHost;
        private static string CmdPath = @"C:\Windows\System32\cmd.exe";
        string path = @"C:\Windows\System32\drivers\etc\hosts";
        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 执行cmd命令
        /// 多命令请使用批处理命令连接符：
        /// <![CDATA[
        /// &:同时执行两个命令
        /// |:将上一个命令的输出,作为下一个命令的输入
        /// &&：当&&前的命令成功时,才执行&&后的命令
        /// ||：当||前的命令失败时,才执行||后的命令]]>
        /// 其他请百度
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="output"></param>
        public static void RunCmd(string cmd, out string output)
        {
            cmd = cmd.Trim().TrimEnd('&') + "&exit";//说明：不管命令是否成功均执行exit命令，否则当调用ReadToEnd()方法时，会处于假死状态
            using (Process p = new Process())
            {
                p.StartInfo.FileName = CmdPath;
                p.StartInfo.UseShellExecute = false;        //是否使用操作系统shell启动
                p.StartInfo.RedirectStandardInput = true;   //接受来自调用程序的输入信息
                p.StartInfo.RedirectStandardOutput = true;  //由调用程序获取输出信息
                p.StartInfo.RedirectStandardError = true;   //重定向标准错误输出
                p.StartInfo.CreateNoWindow = true;          //不显示程序窗口
                p.Start();//启动程序

                //向cmd窗口写入命令
                p.StandardInput.WriteLine(cmd);
                p.StandardInput.AutoFlush = true;

                //获取cmd窗口的输出信息
                output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();//等待程序执行完退出进程
                p.Close();
            }
        }

        public bool HttpDownload(string url, string path)
        {
            string tempPath = System.IO.Path.GetDirectoryName(path) + @"\temp";
            System.IO.Directory.CreateDirectory(tempPath);  //创建临时文件目录
            string tempFile = tempPath + @"\" + System.IO.Path.GetFileName(path) + ".temp"; //临时文件
            if (System.IO.File.Exists(tempFile))
            {
                System.IO.File.Delete(tempFile);    //存在则删除
            }
            try
            {
                FileStream fs = new FileStream(tempFile, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                // 设置参数
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                //发送请求并获取相应回应数据
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                //直到request.GetResponse()程序才开始向目标网页发送Post请求
                Stream responseStream = response.GetResponseStream();
                //创建本地文件写入流
                //Stream stream = new FileStream(tempFile, FileMode.Create);
                byte[] bArr = new byte[1024];
                int size = responseStream.Read(bArr, 0, (int)bArr.Length);
                while (size > 0)
                {
                    //stream.Write(bArr, 0, size);
                    fs.Write(bArr, 0, size);
                    size = responseStream.Read(bArr, 0, (int)bArr.Length);
                }
                //stream.Close();
                fs.Close();
                responseStream.Close();
                System.IO.File.Move(tempFile, path);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public static string DoGetRequestSendData(string url)
        {
            HttpWebRequest hwRequest = null;
            HttpWebResponse hwResponse = null;

            string strResult = string.Empty;
            try
            {
                hwRequest = (System.Net.HttpWebRequest)WebRequest.Create(url);
                //hwRequest.Timeout = 30000;
                hwRequest.Method = "GET";
                hwRequest.ContentType = "application/x-www-form-urlencoded";
            }
            catch (System.Exception err)
            {

            }
            try
            {
                hwResponse = (HttpWebResponse)hwRequest.GetResponse();
                StreamReader srReader = new StreamReader(hwResponse.GetResponseStream(), Encoding.ASCII);
                strResult = srReader.ReadToEnd();
                srReader.Close();
                hwResponse.Close();
            }
            catch (System.Exception err)
            {
            }
            return strResult;
        }
        private void replace()
        {
            System.IO.File.Delete(path);
            try
            {
                File.WriteAllText(path, gHost);
                MessageBox.Show("文件已保存");
            }
            catch (Exception)
            {
                MessageBox.Show("文件保存失败！！");
                throw;
            }
        }
        private string read()
        {
            //下载github——host
            gHost = DoGetRequestSendData(@"https://hub.fastgit.org/ButterAndButterfly/GithubHost/releases/download/v1/host.txt");
            textBox1.Text = gHost;
            //读取电脑hosts
            string st;
            try
            {
                st = File.ReadAllText(path);
                textBox2.Text = st;
                return st;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void backup()
        {
            System.IO.File.Delete(@"C:\Windows\System32\drivers\etc\hosts.bak");
            System.IO.File.Copy(path, @"C:\Windows\System32\drivers\etc\hosts.bak");
        }
        private void recover()
        {
            System.IO.File.Delete(path);
            System.IO.File.Copy(@"C:\Windows\System32\drivers\etc\hosts.bak",path);
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            backup();
            read();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            replace();
            read();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            recover();
            read();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            backup();
            read();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            string output = "";
            string cmd = @"ipconfig /flushdns";
            RunCmd(cmd, out output);
            MessageBox.Show(output);
        }
    }
}
