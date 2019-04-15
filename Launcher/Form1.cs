using Microsoft.Win32;
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
        //VARIBLE GLOBAL PARA EL CHECKBOX QUE VAMOS A CREAR "MANUALMENTE" EN LA CABECERA DE LA COLUMNA DEL DATAGRIDVIEW
        CheckBox headerCheckBox = new CheckBox();

        public Form1()
        {
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            

            comboBox1.SelectedIndex = comboBox1.Items.IndexOf("Normal");

            //Add a CheckBox Column to the DataGridView Header Cell.

            //Find the Location of Header Cell.
            Point headerCellLocation = this.dataGridView1.GetCellDisplayRectangle(0, -1, true).Location;

            //Place the Header CheckBox in the Location of the Header Cell.
            headerCheckBox.Location = new Point(headerCellLocation.X + 8, headerCellLocation.Y + 2);
            headerCheckBox.BackColor = Color.White;
            headerCheckBox.Size = new Size(18, 18);

            //Assign Click event to the Header CheckBox.
            headerCheckBox.Click += new EventHandler(HeaderCheckBox_Clicked);
            dataGridView1.Controls.Add(headerCheckBox);

            //Add a CheckBox Column to the DataGridView at the first position.
            DataGridViewCheckBoxColumn checkBoxColumn = new DataGridViewCheckBoxColumn();
            checkBoxColumn.HeaderText = "";
            checkBoxColumn.Width = 30;
            checkBoxColumn.Name = "checkBoxColumn";
            dataGridView1.Columns.Insert(0, checkBoxColumn);

            //Assign Click event to the DataGridView Cell.
            dataGridView1.CellContentClick += new DataGridViewCellEventHandler(DataGridView_CellClick);

            //en producción, lo que haremos será coger el directorio actual como sourceDirectory y a partir de ahí calcular las rutas.
            string currentDirectory = Directory.GetCurrentDirectory() + @"\Apps";

            try
            {
                int cont = 0;

                string[] filePaths = Directory.GetDirectories(currentDirectory);

                string[] files = Directory.GetFiles(currentDirectory, "*.exe", SearchOption.AllDirectories);


                //ahora vamos a insertar las versiones de los archivos
                cont = 0;
                foreach (string currentFile in files)
                {
                    int posFinal = currentFile.LastIndexOf(@"\");
                    //MessageBox.Show(posFinal.ToString());
                    int posIni = currentFile.LastIndexOf(@"\", posFinal - 1);
                    //MessageBox.Show(posIni.ToString());
                    string folder = currentFile.Substring(posIni + 1, posFinal - posIni - 1);
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

                    dataGridView1.Rows[cont - 1].Cells["size"].Value = Math.Round(f.Length / 1048576.00, 2);
                    dataGridView1.Rows[cont - 1].Cells["path"].Value = currentFile;

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
                    //MessageBox.Show(Directory.GetFiles(ruta, "info.txt", SearchOption.AllDirectories)[0]);
                    StreamReader file = new StreamReader(Directory.GetFiles(ruta, "info.txt", SearchOption.AllDirectories)[0], Encoding.Default);
                    while ((line = file.ReadLine()) != null)
                    {
                        switch (lineas)
                        {
                            case 0://leemos tamaño de instalación
                                dataGridView1.Rows[cont - 1].Cells["size"].Value = line;
                                lineas++;
                                break;
                            case 1://leemos fecha de release
                                dataGridView1.Rows[cont - 1].Cells["Fecha"].Value = line;
                                lineas++;
                                break;
                            case 2://leemos descripción del programa
                                dataGridView1.Rows[cont - 1].Cells["Descrip"].Value = line;
                                lineas++;
                                break;
                            case 3://leemos parámetro de instalación silenciosa
                                dataGridView1.Rows[cont - 1].Cells["Param"].Value = line;
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

                MessageBox.Show(ex.Message,"Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }

            Colorea_Grid();
            label6.Text = Calcula_Tamano_unidad().ToString();
            Comprueba_Registro();



        }

        //EVENTO PARA MARCAR TODOS LOS CHECKS
        private void HeaderCheckBox_Clicked(object sender, EventArgs e)
        {
            //Necessary to end the edit mode of the Cell.
            dataGridView1.EndEdit();

            //Loop and check and uncheck all row CheckBoxes based on Header Cell CheckBox.
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                DataGridViewCheckBoxCell checkBox = (row.Cells["checkBoxColumn"] as DataGridViewCheckBoxCell);
                checkBox.Value = headerCheckBox.Checked;
            }
            Recalcular_Tamanos();
        }

        //SI MARCAMOS MANUALMENTE CADA CHECKBOX, QUE SE MARQUE EL GENERAL TAMBIÉN
        private void DataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            //Check to ensure that the row CheckBox is clicked.
            if (e.RowIndex >= 0 && e.ColumnIndex == 0)
            {
                //Loop to verify whether all row CheckBoxes are checked or not.
                bool isChecked = true;
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (Convert.ToBoolean(row.Cells["checkBoxColumn"].EditedFormattedValue) == false)
                    {
                        isChecked = false;
                        break;
                    }
                }
                headerCheckBox.Checked = isChecked;
            }
        }


        //METODO QUE CALCULA EL ESPACIO LIBRE EN LA UNIDAD DONDE ESTÁ INSTALADO EL WINDOWS
        private double Calcula_Tamano_unidad()
    {
        double espacio = 0;
        foreach (DriveInfo drive in DriveInfo.GetDrives())
        {
            string path = Path.GetPathRoot(Environment.SystemDirectory);
            if (drive.IsReady && drive.Name == path)
            {
                espacio = Math.Round(Convert.ToDouble(drive.TotalFreeSpace / 1048576.0), 2);
            }
        }
        return espacio;
    }


        //BOTON CANCELAR, SALIMOS DE LA APP
        private void button2_Click(object sender, EventArgs e)
        {
            var res = MessageBox.Show("¿Seguro que desea salir?", "Salir", MessageBoxButtons.YesNo);
            if (res == DialogResult.Yes)
            {
                Application.Exit();
            }

        }

        //EVENTO DE LA ETIQUETA DEL MAIL DEL DESARROLLADOR PARA QUE SALTE AL CLIENTE DE CORREO POR DEFECTO
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("mailto:jcescm@gmail.com");
        }


        //BOTON INSTALAR SELECCIONADAS, COMPRUEBA EL METODO SELECCIONADO, COMPRUEBA TAMAÑO Y PROCEDE A INSTALAR.
        private void button1_Click(object sender, EventArgs e)
        {
            int modoInstalacion = comboBox1.SelectedIndex;
            if (double.Parse(label6.Text.ToString()) < double.Parse(label10.Text))
            {
                MessageBox.Show("NO TIENE ESPACIO SUFICIENTE EN SU DISCO DURO!!!","Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }
            else
            {   
                if (modoInstalacion == 0)
                {
                    var res = MessageBox.Show("Se va a proceder a la instalación normal de las aplicaciones seleccionadas", "Instalación normal", MessageBoxButtons.YesNo);
                    if (res == DialogResult.Yes)
                    {
                        try
                        {
                            foreach (DataGridViewRow row in dataGridView1.Rows)
                            {
                                DataGridViewCheckBoxCell checkBox = (row.Cells["checkBoxColumn"] as DataGridViewCheckBoxCell);
                                if ((bool)checkBox.Value == true)
                                {
                                    try
                                    {
                                        Process p = new Process();
                                        p.StartInfo.FileName = row.Cells["path"].Value.ToString();
                                        p.Start();
                                        p.WaitForExit();
                                    }
                                    catch (Exception ex)
                                    {
                                        res = MessageBox.Show("No se ha podido realizar la instalación de la aplicación:" + row.Cells["aplicacion"].Value.ToString() + "por el motivo:" + ex.Message,
                                                        "Error en la instalación", MessageBoxButtons.OK,MessageBoxIcon.Error);
                                    }
                                }
                            }
                            MessageBox.Show("Se ha realizado la instalación. Si ha recibido algún mensaje de error, revise configuración en concreto de dicha aplicación",
                                            "Instalación completada.",MessageBoxButtons.OK);
                        }
                        catch (Exception)
                        {

                            throw;
                        }
                        
                    }
                }else if (modoInstalacion == 1)
                {
                    var res = MessageBox.Show("Se va a proceder a la instalación silenciosa de las aplicaciones seleccionadas", "Instalación silenciosa", MessageBoxButtons.YesNo);
                    if (res == DialogResult.Yes)
                    {
                        //buscamos si hay aplicaciones que no soportan el modo silencioso
                        bool nosilenciosas = false;
                        bool silenciosas = false;
                        foreach (DataGridViewRow row in dataGridView1.Rows)
                        {
                            DataGridViewCheckBoxCell checkBox = (row.Cells["checkBoxColumn"] as DataGridViewCheckBoxCell);
                            if ((bool)checkBox.Value == true && row.Cells["Smode"].Value.ToString() == "NO")
                            {
                                nosilenciosas = true;
                            }
                            if ((bool)checkBox.Value == true && row.Cells["Smode"].Value.ToString() == "SI")
                            {
                                silenciosas = true;
                            }
                        }
                        if (nosilenciosas && silenciosas) //instalamos primero silenciosas y después no silenciosas
                        {
                            res = MessageBox.Show("Se han encontrado aplicaciones que no permiten el modo de instalación silenciosa, si acepta, se instalarán primero las silenciosas y después las normales",
                                            "Atención", MessageBoxButtons.YesNo,MessageBoxIcon.Warning);
                            if (res == DialogResult.Yes)
                            {
                                try
                                {
                                    //primero instalamos las silenciosas

                                    foreach (DataGridViewRow row in dataGridView1.Rows)
                                    {
                                        DataGridViewCheckBoxCell checkBox = (row.Cells["checkBoxColumn"] as DataGridViewCheckBoxCell);
                                        if ((bool)checkBox.Value == true && row.Cells["Smode"].Value.ToString() == "SI")
                                        {
                                            try
                                            {
                                                MessageBox.Show("Instalando solo silenciosa");
                                            }
                                            catch (Exception ex)
                                            {

                                                res = MessageBox.Show("No se ha podido realizar la instalación de la aplicación:" + row.Cells["aplicacion"].Value.ToString() + "por el motivo:" + ex.Message,
                                                        "Error en la instalación", MessageBoxButtons.OK,MessageBoxIcon.Error);
                                            }
                                        }
                                    }
                                    //ahora instalamos las normales
                                    foreach (DataGridViewRow row in dataGridView1.Rows)
                                    {
                                        DataGridViewCheckBoxCell checkBox = (row.Cells["checkBoxColumn"] as DataGridViewCheckBoxCell);
                                        if ((bool)checkBox.Value == true && row.Cells["Smode"].Value.ToString() == "NO")
                                        {
                                            try
                                            {
                                                Process p = new Process();
                                                p.StartInfo.FileName = row.Cells["path"].Value.ToString();
                                                p.Start();
                                                p.WaitForExit();
                                            }
                                            catch (Exception ex)
                                            {

                                                res = MessageBox.Show("No se ha podido realizar la instalación de la aplicación:" + row.Cells["aplicacion"].Value.ToString() + "por el motivo:" + ex.Message,
                                                        "Error en la instalación", MessageBoxButtons.OK,MessageBoxIcon.Error);
                                            }
                                        }
                                    }
                                    MessageBox.Show("Se ha realizado la instalación. Si ha recibido algún mensaje de error, revise configuración en concreto de dicha aplicación",
                                                    "Instalación completada.", MessageBoxButtons.OK);
                                }
                                catch (Exception)
                                {

                                    throw;
                                }
                                
                            }
                        }
                        else if (!nosilenciosas && silenciosas)//instalamos solo las no silenciosas ya que son las únicas marcadas
                        {
                            try
                            {
                                foreach (DataGridViewRow row in dataGridView1.Rows)
                                {
                                    DataGridViewCheckBoxCell checkBox = (row.Cells["checkBoxColumn"] as DataGridViewCheckBoxCell);
                                    if ((bool)checkBox.Value == true)
                                    {
                                        try
                                        {
                                            Process p = new Process();
                                            MessageBox.Show(row.Cells["path"].Value.ToString() + " " + row.Cells["Param"].Value.ToString());
                                            p.StartInfo.FileName = row.Cells["path"].Value.ToString();
                                            p.StartInfo.Arguments = row.Cells["Param"].Value.ToString();
                                            p.Start();
                                            p.WaitForExit();
                                        }
                                        catch (Exception ex)
                                        {

                                            res = MessageBox.Show("No se ha podido realizar la instalación de la aplicación:" + row.Cells["aplicacion"].Value.ToString() + "por el motivo:" + ex.Message,
                                                        "Error en la instalación", MessageBoxButtons.OK,MessageBoxIcon.Error);
                                        }
                                    }
                                }
                                MessageBox.Show("Se ha realizado la instalación. Si ha recibido algún mensaje de error, revise configuración en concreto de dicha aplicación",
                                                "Instalación completada.", MessageBoxButtons.OK);
                            }
                            catch (Exception)
                            {

                                throw;
                            }


                        }
                        else
                        {
                            res = MessageBox.Show("Se han encontrado aplicaciones seleccionadas sin opción de instalación silenciosa. Se procederá a una instalación normal.",
                                                                        "Atención", MessageBoxButtons.YesNo,MessageBoxIcon.Warning);
                            if (res == DialogResult.Yes)
                            {
                                try
                                {
                                    foreach(DataGridViewRow row in dataGridView1.Rows)
                                    {
                                        DataGridViewCheckBoxCell checkBox = (row.Cells["checkBoxColumn"] as DataGridViewCheckBoxCell);
                                        if ((bool)checkBox.Value == true && row.Cells["Smode"].Value.ToString() == "NO")
                                        {
                                            try
                                            {
                                                Process p = new Process();
                                                p.StartInfo.FileName = row.Cells["path"].Value.ToString();
                                                p.Start();
                                                p.WaitForExit();
                                            }
                                            catch (Exception ex)
                                            {

                                                res = MessageBox.Show("No se ha podido realizar la instalación de la aplicación:" + row.Cells["aplicacion"].Value.ToString() + "por el motivo:" + ex.Message,
                                                        "Error en la instalación", MessageBoxButtons.OK,MessageBoxIcon.Error);
                                            }
                                        }
                                    }
                                    MessageBox.Show("Se ha realizado la instalación. Si ha recibido algún mensaje de error, revise configuración en concreto de dicha aplicación",
                                                    "Instalación completada.", MessageBoxButtons.OK);
                                }
                                catch (Exception)
                                {

                                    throw;
                                }
                            }
                        }

                    }
                }
            }
        }
        

        //METODO PARA COMPROBAR SI ESTAMOS CON PERMISOS DE ADMINISTRADOR
        public static bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        //EVENTO QUE SE DISPARA AL ENTRAR EL RATON EN EL LOGO
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            pictureBox1.Image = (Bitmap)Properties.Resources._2;
        }

        //EVENTO QUE SE DIAPARA AL SALIR EL RATON DEL LOGO
        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            pictureBox1.Image = (Bitmap)Properties.Resources._1;
        }

        //EVENTO DEL CLICK DE LA CELDA DEL CHECKBOX, UNA VEZ DETECTADO QUE HEMOS MARCADO, RECALCULAMOS EL TAMAÑO
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            label10.Text = "0";
            if (e.RowIndex == -1)
                return;

            if (dataGridView1.Columns[e.ColumnIndex].Index == 0)
            {
                Recalcular_Tamanos();
            }    
        }

        //SIN ESTE METODO NO PODEMOS DETECTAR SI EL CHECKBOX DEL DATAGRID ESTA MARCADO O NO
        private void dataGridView1_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dataGridView1.IsCurrentCellDirty)
            {
                dataGridView1.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        //METODO QUE RELLENA LA COLUMNA SMODE Y COLOREA LA GRID EN CASO DE QUE LA APP SEA NO SILENCIOSA
        private void Colorea_Grid()
        {
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells["Param"].Value is null)
                {
                    row.Cells["Smode"].Style.BackColor = Color.Red;
                    row.Cells["Smode"].Value = "NO";
                }
                else
                {
                    row.Cells["Smode"].Style.BackColor = Color.Green;
                    row.Cells["Smode"].Value = "SI";
                }
            }
        }
  
        //BOTON MARCAR SILENCIOSAS
        private void button4_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                DataGridViewCheckBoxCell checkbox = (row.Cells["checkBoxColumn"] as DataGridViewCheckBoxCell);
                if (row.Cells["Smode"].Value.ToString() == "SI")
                {
                    checkbox.Value = checkbox.Value == null ? true : true;
                }

            }
            Recalcular_Tamanos();
        }
        
        //CHECK MARCAR SILENCIOSAS
        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            
            if (checkBox2.Checked)
            {

                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    DataGridViewCheckBoxCell checkbox = (row.Cells["checkBoxColumn"] as DataGridViewCheckBoxCell);
                    if (row.Cells["Smode"].Value.ToString() == "SI")
                    {
                        checkbox.Value = checkbox.Value == null ? true : true;
                    }

                }

            }
            else
            {
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    DataGridViewCheckBoxCell checkbox = (row.Cells["checkBoxColumn"] as DataGridViewCheckBoxCell);
                    if (row.Cells["Smode"].Value.ToString() == "SI")
                    {
                        checkbox.Value = checkbox.Value == null ? false : false;
                    }

                }
            }
            Recalcular_Tamanos();
        }

        //BOTON MARCAR NO SILENCIOSAS
        private void button3_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                DataGridViewCheckBoxCell checkbox = (row.Cells["checkBoxColumn"] as DataGridViewCheckBoxCell);
                if (row.Cells["Smode"].Value.ToString() == "NO")
                {
                    checkbox.Value = checkbox.Value == null ? true : true;
                }

            }
            Recalcular_Tamanos();
        }

        //CHECK MARCAR NO SILENCIOSAS
        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox3.Checked)
            {

                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    DataGridViewCheckBoxCell checkbox = (row.Cells["checkBoxColumn"] as DataGridViewCheckBoxCell);
                    if (row.Cells["Smode"].Value.ToString() == "NO")
                    {
                        checkbox.Value = checkbox.Value == null ? true : true;
                    }

                }

            }
            else
            {
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    DataGridViewCheckBoxCell checkbox = (row.Cells["checkBoxColumn"] as DataGridViewCheckBoxCell);
                    if (row.Cells["Smode"].Value.ToString() == "NO")
                    {
                        checkbox.Value = checkbox.Value == null ? false : false;
                    }

                }
            }
            Recalcular_Tamanos();
        }




        //METODO QUE RECALCULA EL TAMAÑO CADA VEZ QUE SE PULSA UN CHECKBOX O SE MARCAN MEDIANTE LOS BOTONES
        private void Recalcular_Tamanos()
        {
            double suma = 0;
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                DataGridViewCheckBoxCell checkbox = (row.Cells["checkBoxColumn"] as DataGridViewCheckBoxCell);
                if ((bool)checkbox.Value == true)
                {
                    try
                    {
                        suma = suma + double.Parse(row.Cells["size"].Value.ToString());
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("ERROR: no se puede calcular el tamaño acumulado de aplicaciones porque:" + ex.Message,"Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
                    }
                    
                }
            }
            label10.Text = suma.ToString();
        }

        //COMPROBAR REGISTRO PARA MARCAR INICIAR CON WINDOWS EN CASO DE QUE EXISTA LA ENTRADA
        private void Comprueba_Registro()
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", false);
            if (rk.GetValue(Application.ProductName) != null)
                checkBox1.Checked = true;
                
        }

        //AÑADO REGISTRY KEY PARA QUE SE EJECUTE AL INICIAR WINDOWS
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (checkBox1.Checked)
                //MessageBox.Show(System.Reflection.Assembly.GetEntryAssembly().Location);
                rk.SetValue(Application.ProductName, System.Reflection.Assembly.GetEntryAssembly().Location);
            else
                rk.DeleteValue(Application.ProductName, false);
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 0 && e.RowIndex != -1)
            {
                //Redibuja();
            }
        }

        private void Redibuja()
        {
            bool todas_silenciosas = true;
            bool todas_nosilenciosas = true;
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                DataGridViewCheckBoxCell checkbox = (row.Cells["checkBoxColumn"] as DataGridViewCheckBoxCell);
                if ((bool)checkbox.Value ==false && row.Cells["Smode"].Value.ToString() == "NO")
                {
                    todas_silenciosas = false;
                }

                if ((bool)checkbox.Value == false && row.Cells["Smode"].Value.ToString() == "SI")
                {
                    todas_nosilenciosas = false;
                }

            }
            if (todas_silenciosas)
            {
                checkBox2.Checked = true;
            }
            else
            {
                checkBox2.Checked = false;
            }
            if (todas_nosilenciosas)
            {
                checkBox3.Checked = true;
            }
            else
            {
                checkBox3.Checked = false;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            var res = MessageBox.Show("¿Seguro que desea salir?", "Salir", MessageBoxButtons.YesNo);
            if (res != DialogResult.Yes)
            {
              e.Cancel = true;
            }
        }
    }
}