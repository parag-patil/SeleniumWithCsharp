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

            DataRow[] dr = data.Select("No._of_contracts=0 OR Symbol=''");
            foreach(DataRow row in dr)
            {
                data.Rows.Remove(row);
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
                bulkCopy.DestinationTableName = "dbo.RBLBANK";
                bulkCopy.WriteToServer(data);
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            SeleniumBO bo = new SeleniumBO("");
            var r = bo.ImpliedVolatility("CE", "489.65", 440, 10, 0.073972602739726, 0, 92.8, 30);
            MessageBox.Show("IV is " + r);
            var d = bo.OptionDelta("CE", 489.65, 440, 0.073972602739726, 10, 125.79, 0);
            MessageBox.Show("Delta is " + d);
            var t = bo.OptionTheta("CE", 489.65, 440, 0.073972602739726, 10, 125.79, 0);
            MessageBox.Show("Theta is " + t);
            var g = bo.OptionGamma(489.65, 440, 0.073972602739726, 10,125.79, 0);
            MessageBox.Show("Gamma is " + g);
            var v = bo.OptionVega(489.65, 440, 0.073972602739726, 10,125.79,0);
            MessageBox.Show("Vega is " + v);
        }
    }
}
