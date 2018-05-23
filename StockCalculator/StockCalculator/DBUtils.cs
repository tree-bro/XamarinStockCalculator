using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace StockCalculator
{
    public class DBUtils
    {
        private static SqliteConnection connection;
        private static SqliteCommand command;
        private static SqliteDataReader reader;

        public static void initDBConnection(string connectionString)
        {
            if(connection == null)
            {
                try
                {
                    connection = new SqliteConnection(connectionString);
                    connection.Open();
                }
                catch(Exception ex)
                {

                }
            }
        }

        public static void closeDBConnection()
        {
            try
            {
                if(reader !=null && !reader.IsClosed)
                {
                    reader.Close();
                }

                if(command != null)
                {
                    command.Cancel();
                }

                if(connection != null)
                {
                    connection.Close();
                }
                
            }
            catch (Exception ex)
            {

            }
        }

        public static void executeCommand(string queryString)
        {
            if(connection != null)
            {
                try
                {
                    command = connection.CreateCommand();
                    command.CommandText = queryString;
                    command.ExecuteNonQuery();
                }
                catch(Exception ex)
                {

                }
                
            }
        }
    }
}
