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
            //en producción, lo que haremos será coger el directorio actual como sourceDirectory y a partir de ahí calcular las rutas.
            string currentDirectory = Directory.GetCurrentDirectory();
            
            comboBox1.SelectedIndex = comboBox1.Items.IndexOf("Normal");


            try
            {
                int cont = 0;
                
                //este lo usaremos cuando lo metamos en la carpeta definitica, las APPS irán en una carpeta llamada Apps
                //string[] filePaths = Directory.GetFiles(@"c:\prueba\");

                string[] filePaths = Directory.GetDirectories(sourceDirectory);

                //este lo usaremos cuando lo metamos en la carpeta definitica, las APPS irán en una carpeta llamada Apps
                //string folder = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + @"\Apps\";
                //string[] filePaths = Directory.GetDirectories(folder);

                string[] files = Directory.GetFiles(sourceDirectory, "*.exe", SearchOption.AllDirectories);

                //este lo usaremos cuando lo metamos en la carpeta definitica, las APPS irán en una carpeta llamada Apps
                //string[] files = Directory.GetFiles(folder, "*.exe", SearchOption.AllDirectories);
                //MessageBox.Show (filePaths[0]);
                /*
                foreach (string currentFilePath in filePaths)
                {
                    
                    cont++;
                    dataGridView1.Rows.Insert(cont - 1, false, currentFilePath.Substring(sourceDirectory.Length + 1));
                                                         
                }
                */
                //ahora vamos a insertar las versiones de los archivos
                cont = 0;
                foreach (string currentFile in files)
                {
                    int posFinal = currentFile.LastIndexOf(@"\");
                    //MessageBox.Show(posFinal.ToString());
                    int posIni = currentFile.LastIndexOf(@"\", posFinal-1);
                    //MessageBox.Show(posIni.ToString());
                    string folder = currentFile.Substring(posIni+1, posFinal - posIni-1);
                    //MessageBox.Show(folder);
                    cont++;

                    dataGridView1.Rows.Insert(cont - 1, false, folder);

                    //var pathToFile = Path.GetFullPath(currentFile);
                    var versionInfo = FileVersionInfo.GetVersionInfo(currentFile);
                    FileInfo f = new FileInfo(currentFile);
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

                    //VAMOS A ABRIR EL FICHERO DE TEXTO PARA EXTRAER LOS DATOS DE ÉL
                    int lineas = 0;
                    string line;
                    //MessageBox.Show(Directory.GetFiles(folder, "*.info", SearchOption.AllDirectories)[0]);
                    string ruta = currentFile.Substring(0, posFinal);
                    MessageBox.Show(Directory.GetFiles(ruta, "info.txt", SearchOption.AllDirectories)[0]);
                    StreamReader file = new StreamReader(Directory.GetFiles(ruta, "info.txt", SearchOption.AllDirectories)[0],Encoding.Default);
                    while ((line = file.ReadLine()) != null)
                    {
                        switch (lineas)
                        {
                            case 0://leemos tamaño de instalación
                                dataGridView1.Rows[cont - 1].Cells[4].Value = line;
                                lineas++;
                                break;
                            case 1://leemos fecha de release
                                dataGridView1.Rows[cont - 1].Cells[3].Value = line;
                                lineas++;
                                break;
                            case 2://leemos descripción del programa
                                dataGridView1.Rows[cont - 1].Cells[5].Value = line;
                                lineas++;
                                break;
                            case 3://leemos parámetro de instalación silenciosa
                                dataGridView1.Rows[cont - 1].Cells[6].Value = line;
                                lineas++;
                                break;
                            default:
                                lineas++;
                                break;
                        }

                    }
                    file.Close();
                }
            }

            

            catch (Exception ex)
            {

                Console.Write(ex.Message);
                MessageBox.Show(ex.Message);
            }

            Colorea_Grid();


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



        private void button1_Click(object sender, EventArgs e)
        {
            int modoInstalacion = comboBox1.SelectedIndex;

            if (double.Parse(label6.Text.ToString()) < double.Parse(label10.Text))
            {
                MessageBox.Show("NO TIENE ESPACIO SUFICIENTE EN SU DISCO DURO!!!");
            }
            else
            {

                foreach (CheckBox control in panel1.Controls.OfType<CheckBox>())
                {
                    if (control.Checked == true)
                    {
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
            //
            // Detecta si se ha seleccionado el header de la grilla
            //
            if (e.RowIndex == -1)
                return;
            double sum = 0;
            if (dataGridView1.Columns[e.ColumnIndex].Index == 0)
            {
                //
                // Se toma la fila seleccionada
                //
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

                //
                // Se selecciona la celda del checkbox
                //
                DataGridViewCheckBoxCell cellSelecion = row.Cells[0] as DataGridViewCheckBoxCell;

                
                if (Convert.ToBoolean(cellSelecion.Value))
                {

                    sum = double.Parse(label10.Text) + double.Parse(row.Cells[4].Value.ToString());

                }
                else
                {

                    sum = double.Parse(label10.Text) - double.Parse(row.Cells[4].Value.ToString());


                }
            }
            label10.Text = sum.ToString();
        }

        private void dataGridView1_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dataGridView1.IsCurrentCellDirty)
            {
                dataGridView1.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void Colorea_Grid()
        {
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells["Param"].Value is null)
                {
                    row.DefaultCellStyle.BackColor = Color.Yellow;
                    row.Cells["Smode"].Value = "NO";
                }
                else
                {
                    row.Cells["Smode"].Value = "SI";
                }
            }
        }
    }
}
