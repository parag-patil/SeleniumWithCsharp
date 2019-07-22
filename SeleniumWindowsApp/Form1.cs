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
                tmpTable = sbo.ExecuteSelenium(txtSymbol.Text, newFromDate, newToDate);
                if (tmpTable != null)
                {
                    data.Merge(tmpTable);
                }
                newFromDate = newToDate.AddDays(1);
                newToDate = dtToDate.Value;
            }
            if((newToDate - newFromDate).TotalDays < days)
            {
                tmpTable = sbo.ExecuteSelenium(txtSymbol.Text, newFromDate, newToDate);
                if(tmpTable!=null)
                {
                    data.Merge(tmpTable);
                }
            }

            gvData.DataSource = data;
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
                bulkCopy.DestinationTableName = "dbo.OPTSTK_Stagging";
                bulkCopy.WriteToServer(data);
            }
        }
    }
}
