using DocumentFormat.OpenXml.Office.Word;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDMMOperations
{
    internal class MySQLConnector
    {
        string connectionString = "Server=(localdb)\\DocManagerDB;Database=master;Trusted_Connection=True;";
        
        public MySQLConnector()
        {
            
        }

        public string NonQuery(string sql)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand myCommand = new SqlCommand(sql, connection);
                try
                {
                    connection.Open();
                    myCommand.ExecuteNonQuery();
                }
                catch (System.Exception ex)
                {
                    return ex.ToString();
                }
                finally
                {
                    if (connection.State == ConnectionState.Open)
                    {
                        connection.Close();
                    }
                }
            }
            return "Ok";
        }

    }
}
