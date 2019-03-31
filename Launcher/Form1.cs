using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Launcher
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string sourceDirectory = @"c:\prueba";
            
            List<CheckBox> CheckBoxes = new List<CheckBox>();
            List<Label> LabelsVersiones = new List<Label>();
            List<Label> LabelsSizes = new List<Label>();

            comboBox1.SelectedIndex = comboBox1.Items.IndexOf("Normal");
            
            try
            {
                //insertamos primero los checkboxes
                int cont = 0;
                
                //este lo usaremos cuando lo metamos en la carpeta definitica, las APPS irán en una carpeta llamada Apps
                //string[] filePaths = Directory.GetFiles(@"c:\prueba\");

                string[] filePaths = Directory.GetDirectories(@"C:\prueba");

                //este lo usaremos cuando lo metamos en la carpeta definitica, las APPS irán en una carpeta llamada Apps
                //string folder = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + @"\Apps\";
                //string[] filePaths = Directory.GetDirectories(folder);

                string[] files = Directory.GetFiles(@"c:\prueba\", "*.exe", SearchOption.AllDirectories);
                
                //este lo usaremos cuando lo metamos en la carpeta definitica, las APPS irán en una carpeta llamada Apps
                //string[] files = Directory.GetFiles(folder, "*.exe", SearchOption.AllDirectories);

                foreach (string currentFilePath in filePaths)
                {
                    
                    cont++;
                    
                    CheckBox cb = new CheckBox();
                    cb.Location = new System.Drawing.Point(15, 30 + cont * 30);
                    //cb.Text = currentFilePath.Substring(sourceDirectory.Length+6);
                    cb.Text = currentFilePath.Substring(sourceDirectory.Length + 1);
                    cb.CheckAlign = ContentAlignment.MiddleLeft;
                    cb.TextAlign = ContentAlignment.MiddleLeft;
                    cb.AutoSize = true;
                    CheckBoxes.Add(cb);
                                     
                }
                //ahora vamos a insertar las versiones de los archivos
                cont = 0;
                foreach (string currentFile in files)
                {
                    cont++;
                    Label lb = new Label();
                    Label lb2 = new Label();
                    lb.Location = new System.Drawing.Point(200, 30 + cont * 30);
                    lb2.Location = new System.Drawing.Point(300, 30 + cont * 30);
                    var pathToFile = Path.GetFullPath(currentFile);
                    var versionInfo = FileVersionInfo.GetVersionInfo(pathToFile);
                    if (versionInfo.ProductVersion != "")
                    {
                        lb.Text = versionInfo.ProductVersion;
                    }else if (versionInfo.FileVersion != "")
                    {
                        lb.Text = versionInfo.FileVersion;
                    }else
                    {
                        lb.Text = "N/A";
                    }
                    
                    
                    FileInfo f = new FileInfo(pathToFile);
                    var sizeInfo = f.Length;
                    lb2.Text = Math.Round(Convert.ToDouble(sizeInfo/1048576),0).ToString() + "Mb";
                    LabelsVersiones.Add(lb);
                    LabelsSizes.Add(lb2);
                }
            }

            catch (Exception ex)
            {

                Console.Write(ex.Message);
                MessageBox.Show(ex.Message);
            }
            
            foreach (CheckBox c in CheckBoxes) {
                //this.Controls.Add(c);
                panel1.Controls.Add(c);
            }
            foreach (Label l in LabelsVersiones)
            {
                //this.Controls.Add(l);
                panel1.Controls.Add(l);
            }
            foreach (Label l2 in LabelsSizes)
            {
                //this.Controls.Add(l2);
                panel1.Controls.Add(l2);
            }

            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady && drive.Name == @"C:\")
                {
                    int espacio = (int)Math.Round(Convert.ToDouble(drive.TotalFreeSpace / 1048576), 0);
                    label6.Text = espacio.ToString() + "Mb";
                }
 
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("mailto:jcescm@gmail.com");
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true)
            {
                foreach (CheckBox control in panel1.Controls.OfType<CheckBox>())
                {
                    control.Checked = true;
                }
            }
            else
            {
                foreach (CheckBox control in panel1.Controls.OfType<CheckBox>())
                {
                    control.Checked = false;
                }
            }
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int modoInstalacion = comboBox1.SelectedIndex;
            foreach (CheckBox control in panel1.Controls.OfType<CheckBox>())
            {
                if (control.Checked == true){
                    //modo de instalación normal
                    if (modoInstalacion == 0)
                    {
                        Process p = new Process();
                        string[] fileName = Directory.GetFiles(@"c:\prueba\" + control.Text, "*.exe");
                        MessageBox.Show(fileName[0]);
                        p.StartInfo.FileName = fileName[0];
                        p.Start();
                        p.WaitForExit();
                    }
                    else//modo de instalación silenciosa
                    {
                       
                        MessageBox.Show(IsAdministrator().ToString());
                        ProcessStartInfo startInfo = new ProcessStartInfo();
                        startInfo.CreateNoWindow = true;
                        startInfo.UseShellExecute = false;
                        string[] fileName = Directory.GetFiles(@"c:\prueba\" + control.Text, "*.exe");
                        startInfo.Verb = "runas";
                        startInfo.FileName = fileName[0];
                        startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                        startInfo.Arguments = "/S";
                        using (Process exeProcess = Process.Start(startInfo))
                        {
                            exeProcess.WaitForExit();
                           
                        }

                    }
                }
            }
        }

        public static bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            pictureBox1.Image = Image.FromFile(@"c:\prueba\img\2.jpg");
        }

        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            pictureBox1.Image = Image.FromFile(@"c:\prueba\img\1.jpg");
        }
    }
}
