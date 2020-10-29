using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using PrinterAPI;
using System.Xml;
using Spire.Doc;
using System.Drawing.Printing;

namespace nh_printer
{
    
    public partial class Form1 : Form
    {
        DataTable dt = new DataTable();
        DataView dv = null;
        bool dt_have_data = false;
        public Form1()
        {
            InitializeComponent();
            
        }
        
        
        #region 毫秒延时 界面不会卡死
        public static void Delay(int mm)
        {
            DateTime current = DateTime.Now;
            while (current.AddMilliseconds(mm) > DateTime.Now)
            {
                Application.DoEvents();
            }
            return;
        }
        #endregion

       
      
        private void print_doc(string docFile)
        {
            Document doc = new Document();
            doc.LoadFromFile(docFile);

            PrintDocument printDoc = doc.PrintDocument;

            foreach (PaperSize paperSize in printDoc.PrinterSettings.PaperSizes)
            {
                if (paperSize.PaperName == "农行凭证" && paperSize.Height == 114 && paperSize.Height == 241)
                {

                    printDoc.DefaultPageSettings.PaperSize = paperSize;
                    break;
                }
            }

            printDoc.PrintController = new StandardPrintController();

            printDoc.Print();

            Delay(5000);
            

        }
        
        /***
        private void print_doc(string docFile)
        {
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            //不现实调用程序窗口,但是对于某些应用无效
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;

            //采用操作系统自动识别的模式
            p.StartInfo.UseShellExecute = true;

            //要打印的文件路径，可以是WORD,EXCEL,PDF,TXT等等
            p.StartInfo.FileName = docFile;

            //指定执行的动作，是打印，即print，打开是 open
            p.StartInfo.Verb = "print";

            //开始
            p.Start();
            //等待10秒钟
            p.WaitForExit(6000);

        }
        ***/
        private int runProcess(string fileName, string appParam)
        {
            int returnValue = -1;
            try
            {
                Process myProcess = new Process();
                ProcessStartInfo myProcessStartInfo = new ProcessStartInfo(fileName,appParam);
                myProcessStartInfo.CreateNoWindow = true;
                myProcessStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                myProcess.StartInfo = myProcessStartInfo;
                myProcess.Start();

                while (!myProcess.HasExited)
                {
                    myProcess.WaitForExit();
                }

                returnValue = myProcess.ExitCode;
                myProcess.Dispose();
                myProcess.Close();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
            return returnValue;
        }
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            string _name1;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = "\\";//注意这里写路径时要用c:\\而不是c:\
            openFileDialog.Filter = "文本文件|*.txt";
            openFileDialog.RestoreDirectory = true;
            openFileDialog.FilterIndex = 1;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                _name1 = openFileDialog.FileName;
                textBox1.Text = _name1;
            }
            //dataGridView1.Visible = false;
            //dataGridView1.Rows.Clear();
            dataGridView1.DataSource = null;
            dataGridView1.Rows.Clear();
            try
            {
                // 创建一个 StreamReader 的实例来读取文件 
                // using 语句也能关闭 StreamReader
                using (StreamReader sr = new StreamReader(textBox1.Text, System.Text.Encoding.Default))
                {
                    string line;
                    int i = 0;
                    line = sr.ReadLine();
                    
                    //从文件读取并显示行，直到文件的末尾
                    
                    while ((line = sr.ReadLine()) != null)
                    {
                        //Console.WriteLine(line);


                        
                        string[] temp_data = line.Split(',');
                        //Console.WriteLine(temp_data[0]);
                        //richTextBox1.AppendText(line);
                        i = this.dataGridView1.Rows.Add();
                        //this.dataGridView1.Rows[i].Cells[0].Value = temp_data[6];
                        this.dataGridView1.Rows[i].Cells[0].Value = i+1;
                        this.dataGridView1.Rows[i].Cells[1].Value = temp_data[6];
                        this.dataGridView1.Rows[i].Cells[2].Value = temp_data[5];
                        this.dataGridView1.Rows[i].Cells[3].Value = temp_data[1];
                        this.dataGridView1.Rows[i].Cells[4].Value = temp_data[9];
                        this.dataGridView1.Rows[i].Cells[5].Value = temp_data[8];
                        this.dataGridView1.Rows[i].Cells[6].Value = temp_data[10];
                        this.dataGridView1.Rows[i].Cells[7].Value = temp_data[7];

                        i++;
                        //richTextBox1.AppendText(temp_data[6]+"\r\n");


                    }

                    label2.Text = "总计记录：";
                    label3.Text = (dataGridView1.Rows.Count-1).ToString();
                    textBox5.Text = (dataGridView1.Rows.Count - 1).ToString();
                    //dataGridView1.Visible = true;
                    //dataGridView1.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    string path = textBox1.Text.Replace(@"\","/");
                    label4.Visible = true;
                    label4.Text = "正在生成数据，请稍后。。。。";
                    Task.Factory.StartNew(() =>
                    {
                        runProcess("py_nh.exe", path);
                        label4.Text = "数据生成完毕";
                    });
                    
                }

                }
            catch (IOException ex)
            {
                // 向用户显示出错消息
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(ex.Message);
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            dt.TableName = "tmp";
            //将xml数据加载
            //string StrPath = AppDomain.CurrentDomain.BaseDirectory + "data.xml";
            //string Astr = File.ReadAllText(StrPath, Encoding.GetEncoding("gb2312"));
            //dt = XmlToDataTable(Astr);
            //dv = dt.DefaultView;
            //dataGridView1.DataSource = dv;
            //初始化打印机列表
            foreach (string s in PrinterSettings.InstalledPrinters)
            {
                comboBox1.Items.Add(s);
            }
            comboBox1.SelectedIndex = 0;

            textBox4.Text = "1";
        }

        private void label1_Click(object sender, EventArgs e)
        {
            toolStripButton1_Click(sender,e);
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            string[] str = new string[dataGridView1.Rows.Count];
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                if (dataGridView1.Rows[i].Selected == true)
                {
                    str[i] = dataGridView1.Rows[i].Cells[3].Value.ToString();

                    //Console.WriteLine(str[i]);
                   
                     label2.Text = "正在打印：";
                     label3.Text = str[i];
                     print_doc(@"结果\" + str[i] + ".docx");

                   
                    
                        
                    
                }
            }
            //print_doc(@"C:\Users\Administrator\Desktop\sb_yb_printer\农行打印程序\nh_printer\nh_printer\bin\Release\结果\100000008935591.docx");
            
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            var files = Directory.GetFiles(@"结果\","*.docx");

            foreach (var file in files)
                //Console.WriteLine(file);
                print_doc(file);
                //label3.Text = file;
                
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// 将datatable转为xml
        /// </summary>
        /// <param name="vTable">要生成XML的DataTable</param>
        /// <returns></returns>
        public static string DataTable2Xml(DataTable vTable)
        {
            if (null == vTable) return string.Empty;
            StringWriter writer = new StringWriter();
            vTable.WriteXml(writer);
            string xmlstr = writer.ToString();
            writer.Close();
            return xmlstr;
        }

        /// <summary>
        /// 将XML生成DataTable
        /// </summary>
        /// <param name="xmlStr">XML字符串</param>
        /// <returns></returns>
        public static DataTable XmlToDataTable(string xmlStr)
        {
            if (!string.IsNullOrEmpty(xmlStr))
            {
                StringReader StrStream = null;
                System.Xml.XmlTextReader Xmlrdr = null;
                try
                {
                    DataSet ds = new DataSet();
                    //读取字符串中的信息
                    StrStream = new StringReader(xmlStr);
                    //获取StrStream中的数据
                    Xmlrdr = new XmlTextReader(StrStream);
                    //ds获取Xmlrdr中的数据               
                    ds.ReadXml(Xmlrdr);
                    return ds.Tables[0];
                }
                catch (Exception e)
                {
                    return null;
                }
                finally
                {
                    //释放资源
                    if (Xmlrdr != null)
                    {
                        Xmlrdr.Close();
                        StrStream.Close();
                        StrStream.Dispose();
                    }
                }
            }
            return null;
        }

        
   

        private void get_dt(DataTable dt,DataGridView dataGridView1)
        {
            //添加列
            for (int i = 0; i < dataGridView1.Columns.Count; i++)
            {
                dt.Columns.Add(dataGridView1.Columns[i].Name);
            }

            //添加行
            for (int j = 0; j < dataGridView1.Rows.Count - 1; j++)
            {
                DataRow dr = dt.NewRow();
                for (int k = 0; k < dataGridView1.Columns.Count; k++)
                {
                    dr[k] = dataGridView1.Rows[j].Cells[k].Value.ToString();
                }
                dt.Rows.Add(dr);
            }
            
            string Astr = DataTable2Xml(dt);
            string StrPath = AppDomain.CurrentDomain.BaseDirectory + "\\data.xml";
            File.WriteAllText(StrPath, Astr, Encoding.GetEncoding("gb2312"));//不存在该XML文件时会自动生成一个文件

        }
        

        private void button1_Click(object sender, EventArgs e)
        {
            //dataGridView1.Sort(dataGridView1.Columns[0], ListSortDirection.Descending);
            //dt = new DataTable();
            if (!dt_have_data)
            {
                get_dt(dt, dataGridView1);
                dt_have_data = true;
            }
            
               
                //dataGridView1.Rows.Clear();
            
            //dataGridView1.Columns.Clear();
            
            dv = dt.DefaultView;
            
            //dv.RowFilter = "Column7 LIKE '%莎车%' AND Column8  LIKE '%工行%' ";
            string rfs1 = "Column6 = '" + textBox2.Text.Trim() + "'";
            string rfs2 = "Column8 LIKE '%" + textBox3.Text.Trim() + "%'";
            string rfs = rfs1 + " AND " + rfs2;

            dv.RowFilter = rfs;
            //textBox3.Text = get_rule(textBox2.Text, textBox3.Text);
            dataGridView1.DataSource = dv;
            toolStripButton2.Enabled = false;
            toolStripButton1.Enabled = false;
            dataGridView1.Columns[0].Visible = false;
            
            
        }

        private void dataGridView1_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            Rectangle rectangle = new Rectangle(e.RowBounds.Location.X,
                e.RowBounds.Location.Y,
                dataGridView1.RowHeadersWidth - 4,
                e.RowBounds.Height);

            TextRenderer.DrawText(e.Graphics, (e.RowIndex + 1).ToString(),
                dataGridView1.RowHeadersDefaultCellStyle.Font,
                rectangle,
                dataGridView1.RowHeadersDefaultCellStyle.ForeColor,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Right);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            
            dv.RowFilter = null;
            dataGridView1.DataSource = dv;
            

        }

        private void button3_Click(object sender, EventArgs e)
        {
            string printerName = null;
            //comboBox1.SelectedIndex = 0;
            printerName = comboBox1.SelectedItem.ToString();
            if (printerName != null)
            {
                Printer.AddCustomPaperSize(printerName, "农行凭证", 241, 115);
                Printer.SetDefaultPrinter(printerName);
                
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            int start = Convert.ToInt32(textBox4.Text); //起始行    
            int end = Convert.ToInt32(textBox5.Text);//结束行
            dataGridView1.Rows[0].Selected = false;    
            for (int i = start-1; i < end  ; i++)//从起始行到结束行之内循环
            {
                string s = this.dataGridView1.Rows[i].Cells[3].Value.ToString();
                
                textBox6.Text = (i+1).ToString();
                label2.Text = "正在打印：";
                label3.Text = s;
                dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;

                print_doc(@"结果\" + s + ".docx");
            }

            
        }

        private void label13_Click(object sender, EventArgs e)
        {

        }
    }
}
