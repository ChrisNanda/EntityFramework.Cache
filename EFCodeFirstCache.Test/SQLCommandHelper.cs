using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Configuration;

namespace EFCodeFirstCache.Test
{
    public static class SQLCommandHelper
    {
        public static void ExecuteNonQuery(string queryText)
        {
            using (SqlConnection connection = 
                new SqlConnection(ConfigurationManager.ConnectionStrings["ProductConnection"].ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandType = System.Data.CommandType.Text;
                    command.CommandText = queryText;
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
