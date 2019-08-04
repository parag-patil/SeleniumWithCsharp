using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SeleniumLibrary.SeleniumBO;
namespace SeleniumWindowsApp
{
    public partial class Form1 : Form
    {
        const string url = "https://www.nseindia.com/products/content/derivatives/equities/historical_fo.htm";
        DataTable data;
        BindingSource bs = new BindingSource();
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void BtnGetData_Click(object sender, EventArgs e)
        {
            SeleniumBO sbo = new SeleniumBO(url);

            int days = 90;
            data = new DataTable();
            DateTime newFromDate = dtFromDate.Value;
            DateTime newToDate = dtToDate.Value;
            DataTable tmpTable = null;
            while (days <= (newToDate - newFromDate).TotalDays)
            {
                newToDate = newFromDate.AddDays(days);
                tmpTable = sbo.ExecuteSelenium(txtSymbol.Text, newFromDate, newToDate, cBoxOptionType.SelectedItem.ToString(),cBoxInstrumentType.SelectedItem.ToString());
                if (tmpTable != null)
                {
                    data.Merge(tmpTable);
                }
                newFromDate = newToDate.AddDays(1);
                newToDate = dtToDate.Value;
            }
            if((newToDate - newFromDate).TotalDays < days)
            {
                tmpTable = sbo.ExecuteSelenium(txtSymbol.Text, newFromDate, newToDate,cBoxOptionType.SelectedItem.ToString(), cBoxInstrumentType.SelectedItem.ToString());
                if(tmpTable!=null)
                {
                    data.Merge(tmpTable);
                }
            }

            DataRow[] dr = data.Select("No._of_contracts=0 OR Symbol=''");
            foreach(DataRow row in dr)
            {
                data.Rows.Remove(row);
            }

            bs.DataSource = data;
            advancedDataGridView1.DataSource = bs;
            advancedDataGridView1.AutoGenerateColumns = true;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            string connectionString = "Server = SAI\\SQLEXPRESS; Database = ShareMarket; Trusted_Connection = true; MultipleActiveResultSets = true";
            data.Rows.RemoveAt(data.Rows.Count-1);
            using(var bulkCopy = new SqlBulkCopy(connectionString))
            {
                foreach(DataColumn col in data.Columns)
                {
                    bulkCopy.ColumnMappings.Add(col.ColumnName, col.ColumnName);
                }
                bulkCopy.BulkCopyTimeout = 600;
                bulkCopy.DestinationTableName = "OptionChainGreeks";
                bulkCopy.WriteToServer(data);
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            SeleniumBO bo = new SeleniumBO("");
            var r = bo.ImpliedVolatility("CE", 489.65, 440, 0.1, 0.073972602739726, 0, 92.8, 0.3);
            MessageBox.Show("IV- Actual IV = 125.79%, Calculated IV = " + r);
            var d = bo.OptionDelta("CE", 489.65, 440,  0.1, 0.073972602739726, 125.79/100, 0);
            MessageBox.Show("Delta - Actual = 0.69, Calculated = " + d);
            var t = bo.OptionTheta("CE", 489.65, 440,  0.1, 0.073972602739726, 125.79/100, 0);
            MessageBox.Show("Theta - Actual = -1.16, Calculated = " + t);
            var g = bo.OptionGamma(489.65, 440,  0.1, 0.073972602739726, 125.79/100, 0);
            MessageBox.Show("Gamma - Actual = 0.0021, Calculated = " + g);
            var v = bo.OptionVega(489.65, 440,  0.1, 0.073972602739726, 125.79/100,0);
            MessageBox.Show("Vega - Actual = 0.467, Calculated = " + v);
        }

        private void AdvancedDataGridView1_SortStringChanged(object sender, EventArgs e)
        {
            bs.Sort = advancedDataGridView1.SortString;
        }

        private void AdvancedDataGridView1_FilterStringChanged(object sender, EventArgs e)
        {
            bs.Filter = advancedDataGridView1.FilterString;
        }
    }
}
