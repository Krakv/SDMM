using MySql.Data.MySqlClient;
using System.Data;
using Microsoft.Extensions.Configuration;
using System.Net.Sockets;

namespace SDMMOperations
{
    public class MySQLConnector
    {
        protected string? connectionString;
        CancellationTokenSource cancellationTokenSource;
        
        public MySQLConnector()
        {
            connectionString = GetConnectionString();
            cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            
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
        

        public async Task<string> NonQueryAsync(string sql, Dictionary<string, string> parameters)
        {

            string result = "";
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                using (MySqlCommand myCommand = new MySqlCommand(sql, connection))
                {
                    foreach (var key in parameters.Keys)
                    {
                        myCommand.Parameters.AddWithValue(key, parameters[key]);
                    }

                    try
                    {
                        await connection.OpenAsync();
                        await myCommand.ExecuteNonQueryAsync();
                        result = myCommand.LastInsertedId.ToString();
                    }
                    catch (TaskCanceledException)
                    {
                        throw new TaskCanceledException();
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
            }
            return result;
        }

        public async Task<List<Dictionary<string, string>>> QueryAsync(string sql, Dictionary<string, string>? parameters = null)
        {

            List<Dictionary<string, string>> result = new List<Dictionary<string, string>>();
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                using (MySqlCommand myCommand = new MySqlCommand(sql, connection))
                {
                    if (parameters != null)
                    {
                        foreach (var key in parameters.Keys)
                        {
                            myCommand.Parameters.AddWithValue(key, parameters[key]);
                        }
                    }

                    try
                    {
                        await connection.OpenAsync();
                        await myCommand.ExecuteNonQueryAsync();

                        using (var dr = myCommand.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                Dictionary<string, string> line = new Dictionary<string, string>();
                                for (int i = 0; i < dr.FieldCount; i++)
                                {
                                    Console.Write($"{dr.GetName(i)}: {dr[i]}  ");
                                    line.Add(dr.GetName(i), dr[i].ToString() ?? "");
                                }
                                result.Add(line);
                            }
                        }
                    }
                    catch (System.Exception ex) when (ex is not MySqlException) 
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
            }

            return result;
        }

        


    }
}
