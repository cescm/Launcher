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
                MessageBox.Show (filePaths[0]);
                
                foreach (string currentFilePath in filePaths)
                {
                    
                    cont++;
                    dataGridView1.Rows.Insert(cont - 1, false, currentFilePath.Substring(sourceDirectory.Length + 1));
                                                         
                }
                //ahora vamos a insertar las versiones de los archivos
                cont = 0;
                foreach (string currentFile in files)
                {
                    cont++;
                    var pathToFile = Path.GetFullPath(currentFile);
                    var versionInfo = FileVersionInfo.GetVersionInfo(pathToFile);
                   
                    
                    FileInfo f = new FileInfo(pathToFile);
                    var sizeInfo = f.Length;

                    if (versionInfo.ProductVersion != "")
                    {
                        dataGridView1.Rows[cont - 1].Cells["version"].Value = versionInfo.ProductVersion;
                    }
                    else if (versionInfo.FileVersion != "")
                    {
                        dataGridView1.Rows[cont - 1].Cells["version"].Value = versionInfo.FileVersion;
                    }
                    else
                    {
                        dataGridView1.Rows[cont - 1].Cells["version"].Value = "N/A";
                    }

                    dataGridView1.Rows[cont - 1].Cells["size"].Value = Math.Round(f.Length/1048576.00,2);

                    /*recorrer grid
                    foreach (DataGridViewRow row in dtaPagos.Rows)
                    {
                        MessageBox.Show(row.Cells["Pago"].Value.ToString());
                        MessageBox.Show(row.Cells["Cantidad"].Value.ToString());
                        MessageBox.Show(row.Cells["Observaciones"].Value.ToString());
                    }
                    */

                }
            }

            catch (Exception ex)
            {

                Console.Write(ex.Message);
                MessageBox.Show(ex.Message);
            }
            

            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady && drive.Name == @"C:\")
                {
                    int espacio = (int)Math.Round(Convert.ToDouble(drive.TotalFreeSpace / 1048576), 2);
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
            //pictureBox1.Image = Image.FromFile(@"c:\prueba\img\2.jpg");
        }

        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            //pictureBox1.Image = Image.FromFile(@"c:\prueba\img\1.jpg");
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            MessageBox.Show("pulsado" + dataGridView1.CurrentRow.Cells["size"].Value.ToString());
        }
    }
}
