﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;

// Generic DataHandler Class
public class DataHandler<T>
{
    // Ensure the key length is appropriate for AES-128 encryption
    private static readonly byte[] EncryptionKey = new byte[] 
    {
        0x01, 0x23, 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef,
        0xfe, 0xdc, 0xba, 0x98, 0x76, 0x54, 0x32, 0x10
    };

    public void SaveObjectData(string filePath, T obj)
    {
        // Serialize to XML
        XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
        using (StringWriter textWriter = new StringWriter())
        {
            xmlSerializer.Serialize(textWriter, obj);
            string xmlString = textWriter.ToString();

            // Encrypt the XML string
            string encryptedString = EncryptString(xmlString);

            // Save the encrypted string to file
            File.WriteAllText(filePath, encryptedString);
        }
    }

    public T ReadObjectData(string filePath)
    {
        // Read the encrypted string from file
        string encryptedString = File.ReadAllText(filePath);

        // Decrypt the string
        string xmlString = DecryptString(encryptedString);

        // Deserialize from XML
        XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
        using (StringReader textReader = new StringReader(xmlString))
        {
            return (T)xmlSerializer.Deserialize(textReader);
        }
    }

    private string EncryptString(string plainText)
    {
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = EncryptionKey;
            aesAlg.GenerateIV();
            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            using (MemoryStream msEncrypt = new MemoryStream())
            {
                msEncrypt.Write(aesAlg.IV, 0, aesAlg.IV.Length);
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(plainText);
                    }
                }

                return Convert.ToBase64String(msEncrypt.ToArray());
            }
        }
    }

    private string DecryptString(string cipherText)
    {
        byte[] fullCipher = Convert.FromBase64String(cipherText);

        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = EncryptionKey;
            byte[] iv = new byte[aesAlg.BlockSize / 8];
            Array.Copy(fullCipher, 0, iv, 0, iv.Length);
            aesAlg.IV = iv;

            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            using (MemoryStream msDecrypt = new MemoryStream(fullCipher, iv.Length, fullCipher.Length - iv.Length))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {
                        return srDecrypt.ReadToEnd();
                    }
                }
            }
        }
    }
}

// Database Connection Parameters Class
public class DbConnectionParams
{
    // Ensure properties are initialized
    public string ServerName { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

// DatabaseManager Class
public class DatabaseManager : IDisposable
{
    private string connectionString;
    private SqlConnection connection;

    public DatabaseManager(string configFilePath)
    {
        DataHandler<DbConnectionParams> dataHandler = new DataHandler<DbConnectionParams>();
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

// Program Class
public class Program
{
    public static void Main(string[] args)
    {
        string filePath = "dbConnectionParams.STD";

        // Save the connection parameters to a file
        DbConnectionParams connectionParams = new DbConnectionParams
        {
            ServerName = "MONO",
            DatabaseName = "data",
            UserName = "sa",
            Password = "nada123"
        };

        DataHandler<DbConnectionParams> dataHandler = new DataHandler<DbConnectionParams>();
        dataHandler.SaveObjectData(filePath, connectionParams);

        using (var dbManager = new DatabaseManager(filePath))
        {
            try
            {
                // Example: Select statement for Departments
                string selectDepartmentsQuery = "SELECT * FROM Department";
                DataTable departmentsResults = dbManager.ExecuteSelect(selectDepartmentsQuery, null);
                PrintDataTable(departmentsResults);

                // Example: Insert into Employees table
                string insertEmployeeQuery = "INSERT INTO Employees (FirstName, LastName, BirthDate, DepartmentID) " +
                                             "VALUES (@firstName, @lastName, @birthDate, @departmentID)";
                List<SqlParameter> insertEmployeeParams = new List<SqlParameter>
                {
                    new SqlParameter("@firstName", "Nada"),
                    new SqlParameter("@lastName", "Hassan"),
                    new SqlParameter("@birthDate", new DateTime(2005, 5, 15)),
                    new SqlParameter("@departmentID", 1) // Example: Ensure this ID exists in Departments table
                };

                int rowsInserted = dbManager.ExecuteInsert(insertEmployeeQuery, insertEmployeeParams);
                Console.WriteLine($"Rows Inserted: {rowsInserted}");

                // Example: Update Employees table
                string updateEmployeeQuery = "UPDATE Employees SET FirstName = @firstName WHERE EmployeeID = @employeeID";
                List<SqlParameter> updateEmployeeParams = new List<SqlParameter>
                {
                    new SqlParameter("@firstName", "Shams"),
                    new SqlParameter("@employeeID", 1)
                };
                int rowsUpdated = dbManager.ExecuteUpdate(updateEmployeeQuery, updateEmployeeParams);
                Console.WriteLine($"Rows Updated: {rowsUpdated}");

                // Example: Delete from Employees table
                string deleteEmployeeQuery = "DELETE FROM Employees WHERE EmployeeID = @employeeID";
                List<SqlParameter> deleteEmployeeParams = new List<SqlParameter>
                {
                    new SqlParameter("@employeeID", 2)
                };
                int rowsDeleted = dbManager.ExecuteDelete(deleteEmployeeQuery, deleteEmployeeParams);
                Console.WriteLine($"Rows Deleted: {rowsDeleted}");

                // Get server date
                DateTime serverDate = dbManager.GetServerDate();
                Console.WriteLine($"SQL Server Date: {serverDate}");
            }
            catch (Exception ex)
            {
                                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    private static void PrintDataTable(DataTable table)
    {
        foreach (DataColumn column in table.Columns)
        {
            Console.Write($"{column.ColumnName}\t");
        }
        Console.WriteLine();

        foreach (DataRow row in table.Rows)
        {
            foreach (var item in row.ItemArray)
            {
                Console.Write($"{item}\t");
            }
            Console.WriteLine();
        }
    }
}
