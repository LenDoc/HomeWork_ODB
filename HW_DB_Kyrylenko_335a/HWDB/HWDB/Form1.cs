using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HWDB
{
    public partial class Form1 : Form
    {
        private static string dbCommand = "";
        private BindingSource bindingSrc;
        private static string dbPath = Application.StartupPath + "\\" + "DB.db;";
        private static string conString = "Data Source=" + dbPath + "Version=3;New=False;Compress=True;";
        private static SQLiteConnection connection = new SQLiteConnection(conString);
        private static SQLiteCommand command = new SQLiteCommand("", connection);

        private static string sql;

        public Form1()
        {
            InitializeComponent();
            this.textBoxStationID.Enabled = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            openConnection();

            updateDataBiding();
            closeConnection();
        }
        private void openConnection()
        {
            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
               MessageBox.Show("The connection is" + connection.State.ToString());
            }
        }
        private void closeConnection()
        {
            if (connection.State == ConnectionState.Open)
            {
                connection.Close();
               MessageBox.Show("The connection is" + connection.State.ToString());
            }
        }



        private void displayPosition()
        {
            positionLabel.Text = "Position: " + Convert.ToString(bindingSrc.Position + 1) +
                "/" + bindingSrc.Count.ToString();
        }
        private void updateDataBiding(SQLiteCommand cmd = null)
        {
            try
            {
                TextBox tb;
                foreach (Control c in groupBox1.Controls)
                {
                    if (c.GetType() == typeof(TextBox))
                    {
                        tb = (TextBox)c;
                        tb.DataBindings.Clear();
                        tb.Text = "";
                    }
                }

                dbCommand = "SELECT";
                sql = "SELECT * FROM Station ORDER BY StationID ASC; ";

                if (cmd == null)
                {
                    command.CommandText = sql;
                }
                else
                {
                    command = cmd;
                }

                SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
                DataSet dataSt = new DataSet();
                adapter.Fill(dataSt, "Station");

                bindingSrc = new BindingSource();
                bindingSrc.DataSource = dataSt.Tables["Station"];

                textBoxStationID.DataBindings.Add("Text", bindingSrc, "StationID");
                textBoxStationName.DataBindings.Add("Text", bindingSrc, "StationName");

                dataGridView1.Enabled = true;
                dataGridView1.DataSource = bindingSrc;

                dataGridView1.AutoResizeColumns((DataGridViewAutoSizeColumnsMode)DataGridViewAutoSizeColumnsMode.AllCells);
                dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

                displayPosition();
            }
            catch (Exception ex)
            {

                MessageBox.Show("Data Binding Error: " + ex.Message.ToString(),
                    "Error Message : ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void buttonExit_Click_1(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void buttonBefore1_Click(object sender, EventArgs e)
        {
            bindingSrc.MoveFirst();
            displayPosition();

        }

        private void buttonBefore_Click(object sender, EventArgs e)
        {
            bindingSrc.MovePrevious();
            displayPosition();
        }

        private void buttonNext_Click(object sender, EventArgs e)
        {
            bindingSrc.MoveNext();
            displayPosition();
        }

        private void buttonNext1_Click(object sender, EventArgs e)
        {
            bindingSrc.MoveLast();
            displayPosition();
        }

        private void dataGridView1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            try
            {
                displayPosition();
            }
            catch (Exception)
            {

            }
        }

        private void buttonInsert_Click(object sender, EventArgs e)
        {
            try
            {
                if (buttonInsert.Text == "Insert")
                {
                    buttonInsert.Text = "Cancel";
                    positionLabel.Text = "Position: 0/0";
                    dataGridView1.ClearSelection();
                    dataGridView1.Enabled = false;

                }
                else
                {
                    buttonInsert.Text = "Insert";
                    updateDataBiding();
                    return;
                }
                TextBox txt;
                foreach (Control c in groupBox1.Controls)
                {
                    if (c.GetType() == typeof(TextBox))
                    {
                        txt = (TextBox)c;
                        txt.DataBindings.Clear();
                        txt.Text = "";
                        if (txt.Name.Equals("textBoxStationName"))
                        {
                            if (txt.CanFocus)
                            {
                                txt.Focus();
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        private void buttonRefresh_Click(object sender, EventArgs e)
        {
            if (buttonInsert.Text.Equals("Cancel"))
            {
                return;
            }
            updateDataBiding();
        }

        private void addCmdParametrs()
        {
            command.Parameters.Clear();
            command.CommandText = sql;

            command.Parameters.AddWithValue("StationName", textBoxStationName.Text.Trim());
            if (dbCommand.ToUpper() == "UPDATE")
            {
                command.Parameters.AddWithValue("StationID", textBoxStationID.Text.Trim());

            }

        }

        private void buttonSave_Click(object sender, EventArgs e)
        {

            if (string.IsNullOrEmpty(textBoxStationName.Text.Trim()))
            {
                MessageBox.Show("Please fill field!", "Insert new station",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;

            }
            openConnection();
            try {
                if (buttonInsert.Text == "Insert")
                {
                    if (textBoxStationID.Text.Trim() == "" ||
                        string.IsNullOrEmpty(textBoxStationID.Text.Trim()))
                    {
                        MessageBox.Show("Please select an item");
                        return;
                    }

                    if (MessageBox.Show("StationID: " + textBoxStationID.Text.Trim() +
                        " -- Do you want to update the selected recors?",
                        "UPDATE",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2) == DialogResult.No)
                    {
                        return; //if user press no button - exit
                    }

                    dbCommand = "UPDATE";
                    sql = "UPDATE Station SET StationName = @StationName WHERE StationID = @StationID";
                    addCmdParametrs();
                }
                else if (buttonInsert.Text.Equals("Cancel"))
                {
                    DialogResult result;
                    result = MessageBox.Show("Do you want to insert a new station record? (Y/N)",
                        "INSERT: ",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question
                        );

                    if (result == DialogResult.Yes)
                    {
                        dbCommand = "INSERT";
                        sql = "INSERT INTO Station(StationName) VALUES(@StationName)";
                        addCmdParametrs();
                    }
                    else
                    {
                        return;
                    }
                }

                int executeResult = command.ExecuteNonQuery();
                if (executeResult == -1)
                {
                    MessageBox.Show("Data was not saved!", "Fail to save data.",
                        MessageBoxButtons.OK, MessageBoxIcon.Stop
                        );
                }
                else
                {
                    MessageBox.Show("Your SQL " + dbCommand + " QUERY has been executed successfully!",
                        "SAVE",
                        MessageBoxButtons.OK, MessageBoxIcon.Information

                        );
                    updateDataBiding();
                    buttonInsert.Text = "Insert";
                }



            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message.ToString(), "Save data: ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                dbCommand = "";
                closeConnection();

            }
        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            if (buttonInsert.Text == "Cancel")
            {
                return;
            }
            if (textBoxStationID.Text.Trim() == "" || string.IsNullOrEmpty(textBoxStationID.Text.Trim()))
            {
                MessageBox.Show("Please select an item from the list!",
                    "Delete Data: ",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            openConnection();
            try
            {
                if (MessageBox.Show("StationID: " + textBoxStationID.Text.Trim() +
                      " -- Do you want to delete the selected recors?",
                      "DELETE: ",
                      MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                      MessageBoxDefaultButton.Button2) == DialogResult.No)
                {
                    return; //if user press no button - exit
                }

                dbCommand = "DELETE";
                sql = "DELETE FROM Station WHERE StationID = @StationID";
                command.Parameters.Clear();
                command.CommandText = sql;
                command.Parameters.AddWithValue("StationID", textBoxStationID.Text.Trim());
                int executeResult = command.ExecuteNonQuery();
                if(executeResult == 1)
                {
                    MessageBox.Show("Your SQL  " + dbCommand + " QUERY has been executed successfully!",

                     "Delete: ",
                     MessageBoxButtons.OK, MessageBoxIcon.Information
                     );
                    updateDataBiding();
                }
 

            } catch(Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message.ToString(),
                    "Error Message: ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error
                    );
            }
            finally
            {
                dbCommand = "";
                closeConnection();
            }
        }
    }
}
