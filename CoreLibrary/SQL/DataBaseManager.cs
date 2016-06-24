using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Core
{
    class DataBaseManager
    {
        private SqlConnection getConnection() {

            string connetionString = null;
            SqlConnection con;
            connetionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=nayatell;Integrated Security=True;Pooling=False";
            con = new SqlConnection(connetionString);
            return con;
                
        }

        public String getVendorNameByMac(string mac)
        {
            if (mac.Equals("")) {
                return null;
            }
            String prifixs = mac.Replace(":","").Substring(0, 6);
            String sql = "select vendor_name from vendors where prefix = '" + prifixs+"'";
            SqlConnection connection = getConnection();
            String vendorName = null;
            try
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sql, connection);
                SqlDataReader dataReader = command.ExecuteReader();
                while (dataReader.Read())
                {
                    //  MessageBox.Show(dataReader.GetValue(0) + " - " + dataReader.GetValue(1) + " - " + dataReader.GetValue(2));
                    vendorName = dataReader.GetString(0);
                }
                dataReader.Close();
                command.Dispose();
                connection.Close();
                return vendorName;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: sql:",ex);
                return vendorName;

            }
        }

    }
}
