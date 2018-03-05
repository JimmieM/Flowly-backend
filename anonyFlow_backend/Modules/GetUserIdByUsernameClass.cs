using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Net.Http;

namespace anonyFlow_backend.Controllers
{
    public class UserId {
        public bool success { get; set; }
        public string message { get; set; }
        public int userId { get; set; }

        public UserId(bool success, string message, int userId) {
            this.success = success;
            this.message = message;
            this.userId = userId;
            return;
        }

        public UserId(bool success, int userId)
        {
            this.success = success;
            this.userId = userId;
            return;
        }

        public bool didSucceed() {
            return success;
        }
    }
        

    public class GetUserIdByUsernameClass
    {
        public string username;
        public GetUserIdByUsernameClass(string username)
        {
            this.username = username;
        }

        public UserId getId() {
            try
            {
                SqlConnectionStringBuilder builder = WebApiConfig.Connection();
                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();
                    StringBuilder sb = new StringBuilder();

                    sb.Append("SELECT " +
                     "[user_id] " +
                      "FROM [dbo].[users] " +
                              "WHERE [user_name] = '" + username + "'"
                     );

                    String sql = sb.ToString();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                int id = 0;
                                while (reader.Read())
                                {
                                    id = reader.GetInt32(0);
                                }

                                if(id == 0) {
                                    return new UserId(false, "Internal error", 0);
                                }

                                return new UserId(true, id);

                            } else {
                                return new UserId(false, "0 rows found", 0);
                            }
                        }
                    }
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine("INSERT ERROR: " + e.ToString());
                return new UserId(false, e.ToString(), 0);
            }
        }
    }
}
