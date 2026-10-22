using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace AkumsPrintingApplication.PL
{
    public class Common
    {
        public DataSet ExecuteDataset(SqlCommand cmd)
        {
            SqlConnection con = new SqlConnection();
            con.ConnectionString = PCommon.StrSqlCon;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Connection = con;
            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }
            con.Open();
            DataSet ds = new DataSet();
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(ds);
            con.Close();
            return ds;
        }
        public DataTable ExecuteDatatable(SqlCommand cmd)
        {
            SqlConnection con = new SqlConnection();
            con.ConnectionString = PCommon.StrSqlCon;
            cmd.CommandType = CommandType.StoredProcedure;
            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }
            con.Open();
            DataSet ds = new DataSet();
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(ds);
            con.Close();
            return ds.Tables[0];
        }

        public void newfillcombo(ComboBox combo, string procname, string mode)
        {
            SqlConnection con = new SqlConnection();
            con.ConnectionString = PCommon.StrSqlCon;
            con.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = 30000;
            cmd.CommandText = procname;
            cmd.Connection = con;
            cmd.Parameters.AddWithValue("@TYPE", mode);
            SqlDataAdapter sa = new SqlDataAdapter(cmd);

            DataSet ds = new DataSet();
            sa.Fill(ds);
            if (ds.Tables[0].Rows.Count > 0)
            {

                combo.DataSource = ds.Tables[0];
                combo.DisplayMember = ds.Tables[0].Columns[0].ColumnName;
            }
            else
            {
                combo.Items.Clear();
            }
            sa.Dispose();
            ds.Dispose();
            con.Close();

        }

    }
}