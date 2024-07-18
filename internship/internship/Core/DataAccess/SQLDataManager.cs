using internship.Core.FileAccess;
using internship.Core.Models;
using System.Data;
using System.Data.SqlClient;

namespace internship.Core.DataAccess
{
    internal class SQLDataManager
    {
        private string connectionString;
        private SqlConnection connection;

        public SQLDataManager(string configFilePath)
        {
            FileManager<DbConnectionParams> dataHandler = new FileManager<DbConnectionParams>();
            DbConnectionParams connectionParams = dataHandler.ReadObjectData(configFilePath);

            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder
            {
                DataSource = connectionParams.ServerName,
                InitialCatalog = connectionParams.DatabaseName,
                UserID = connectionParams.UserName,
                Password = connectionParams.Password
            };
            this.connectionString = builder.ConnectionString;
            this.connection = new SqlConnection(this.connectionString);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                connection?.Dispose();
                connection = null;
            }
        }

        private bool BindParameters(SqlCommand command, List<SqlParameter> parameters)
        {
            try
            {
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters.ToArray());
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error binding parameters: {ex.Message}");
                return false;
            }
        }

        public DataTable ExecuteSelect(string sql, List<SqlParameter> parameters)
        {
            DataTable dataTable = new DataTable();
            try
            {
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    if (BindParameters(command, parameters))
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            adapter.Fill(dataTable);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing SELECT query: {ex.Message}");
            }
            return dataTable;
        }

        public int ExecuteNonQuery(string sql, List<SqlParameter> parameters)
        {
            int rowsAffected = 0;
            try
            {
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    if (BindParameters(command, parameters))
                    {
                        connection.Open();
                        rowsAffected = command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing query: {ex.Message}");
            }
            finally
            {
                connection.Close();
            }
            return rowsAffected;
        }

        public DateTime GetServerDate()
        {
            DateTime serverDate = DateTime.MinValue;
            try
            {
                string sql = "SELECT GETDATE() AS CurrentDate";
                DataTable result = ExecuteSelect(sql, null);
                if (result.Rows.Count > 0)
                {
                    serverDate = (DateTime)result.Rows[0]["CurrentDate"];
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting server date: {ex.Message}");
            }
            return serverDate;
        }

        public int ExecuteInsert(string sql, List<SqlParameter> parameters)
        {
            return ExecuteNonQuery(sql, parameters);
        }

        public int ExecuteUpdate(string sql, List<SqlParameter> parameters)
        {
            return ExecuteNonQuery(sql, parameters);
        }

        public int ExecuteDelete(string sql, List<SqlParameter> parameters)
        {
            return ExecuteNonQuery(sql, parameters);
        }
    }
}
