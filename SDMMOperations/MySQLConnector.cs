using MySql.Data.MySqlClient;
using System.Data;
using Microsoft.Extensions.Configuration;
using DocumentFormat.OpenXml.Drawing.Spreadsheet;
using System.Transactions;
using System.Xml.Linq;
using System.Timers;

namespace SDMMOperations
{
    public class MySQLConnector
    {
        protected string? connectionString;
        
        public MySQLConnector()
        {
            connectionString = GetConnectionString();
        }

        public static string? GetConnectionString()
        {
            var config = LoadConfiguration();
            return config.GetConnectionString("DefaultConnection");
        }

        public static IConfigurationRoot LoadConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            return builder.Build();
        }

        public string NonQuery(string sql, Dictionary<string, string> parameters)
        {
            string result = "";
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                MySqlCommand myCommand = new MySqlCommand(sql, connection);
                foreach(var key in parameters.Keys)
                {
                    myCommand.Parameters.AddWithValue(key, parameters[key]);
                }

                try
                {
                    connection.Open();
                    myCommand.ExecuteNonQuery();
                    result = myCommand.LastInsertedId.ToString();
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                finally
                {
                    if (connection.State == ConnectionState.Open)
                    {
                        connection.Close();
                    }
                }
            }
            return result;
        }

        public List<Dictionary<string, string>> Query(string sql, Dictionary<string, string> parameters = null)
        {
            List<Dictionary<string, string>> result = new List<Dictionary<string, string>>();

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                MySqlCommand myCommand = new MySqlCommand(sql, connection);

                if (parameters != null)
                {
                    foreach (var key in parameters.Keys)
                    {
                        myCommand.Parameters.AddWithValue(key, parameters[key]);
                    }
                }

                try
                {
                    connection.Open();
                    myCommand.ExecuteNonQuery();

                    using (var dr = myCommand.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            Dictionary<string, string> line = new Dictionary<string, string>();
                            for (int i = 0; i < dr.FieldCount; i++)
                            {
                                Console.Write($"{dr.GetName(i)}: {dr[i]}  ");
                                line.Add(dr.GetName(i), dr[i].ToString());
                            }
                            result.Add(line);
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                finally
                {
                    if (connection.State == ConnectionState.Open)
                    {
                        connection.Close();
                    }
                }
            }

            return result;
        }

        


    }
}
