using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Qazi.Common;
using Qazi.GUI.CommonDialogs;
using Qazi.BinaryFileIOManager;
using Qazi.EncodingSchemeManager;
using Qazi.DataPreprocessing;

namespace TrainingDataEncodingManager
{
    public partial class Form1 : Form
    {
        DataTable _DataTableToEncode;
        Dictionary<string, string> _EncodingDictionary;
        DataTable _EncodedTable;

        public Form1()
        {
            InitializeComponent();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            textBox1.Text = openFileDialog1.FileName;
            DataSet ds = new DataSet();
            ds.ReadXml(openFileDialog1.FileName);
            DataTableSelectorWnd dtwnd = new DataTableSelectorWnd("Select DataTable to Encode", ds);
            dtwnd.ShowDialog();
            _DataTableToEncode = ds.Tables[dtwnd.TableName];
            dataGridView1.DataSource = _DataTableToEncode;
            lblRecordCount.Text = _DataTableToEncode.Rows.Count.ToString();
            foreach (DataColumn col in _DataTableToEncode.Columns)
            {
                listBox1.Items.Add(col.ColumnName);
            }
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            _EncodedTable.WriteXml(saveFileDialog1.FileName);
            MessageBox.Show("Encoded DataTable Saved...");
        }

        private void btnOpenTrainingData_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog2.ShowDialog();
            BinaryFileSerializationManager fileManager = new BinaryFileSerializationManager();
            _EncodingDictionary = (Dictionary<string, string>)fileManager.Open(openFileDialog2.FileName);
            DataTable dt = EncodingFormateConvertor.HashToDataTable(_EncodingDictionary);
            dataGridView2.DataSource = dt;
            lblHashCodeRecordCount.Text = dt.Rows.Count.ToString();
        }
        

        private void openFileDialog2_FileOk(object sender, CancelEventArgs e)
        {
            textBox2.Text = openFileDialog2.FileName;
           
        }

        private void button2_Click(object sender, EventArgs e)
        {
            List<string> lstAttributes = new List<string>();
            foreach (object item in listBox2.Items)
            {
                lstAttributes.Add(item.ToString());
            }
            DataTableEncodingManager encodingManager = new DataTableEncodingManager(_EncodingDictionary, lstAttributes, _DataTableToEncode, checkBox1.Checked, checkBox2.Checked);
            encodingManager.EncodingCompleted += new WorkerCompletedEventHandler(encodingManager_EncodingCompleted);
            encodingManager.EncodingProgress += new WorkerProgressUpdateEventHandler(encodingManager_EncodingProgress);
            encodingManager.Run();
        }

        void encodingManager_EncodingProgress(object sender, WorkerProgressEventArg e)
        {
            progressBar1.Value = (int)e.ProgressPercentage;
            Application.DoEvents();
        }

        void encodingManager_EncodingCompleted(object sender, WorkerCompletedEventArg e)
        {
            _EncodedTable =(DataTable) e.Result;
            dataGridView3.DataSource = _EncodedTable;
            lblEncodedTableRecordCount.Text = _EncodedTable.Rows.Count.ToString();
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            listBox2.Items.Add(listBox1.SelectedItem);
        }

        private void listBox2_DoubleClick(object sender, EventArgs e)
        {
            listBox2.Items.Remove(listBox2.SelectedItem);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            saveFileDialog1.ShowDialog(this);
        }
    }
}