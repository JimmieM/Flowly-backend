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
    public class User {
        public bool success { get; set; }
        public int user_id { get; set; }
        public string message { get; set; }


        public bool didSucceed() {
            return success;
        }

        public User(bool success, string message)
        {
            this.success = success;
            this.message = message;
            return;
        }
        public User(bool success, int user_id, string message) {
            this.success = success;
            this.user_id = user_id;
            this.message = message;
            return;
        }
    }
    public class GetUserByPostIdClass
    {
        public int post_id;
        public User user;

        public GetUserByPostIdClass(int post_id) {
            this.post_id = post_id;
        }

        /*
         * @returns User instance 
         */
        public User getUser() {
            try
            {
                // include connection..
                SqlConnectionStringBuilder builder = WebApiConfig.Connection();

                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();
                    StringBuilder sb = new StringBuilder();

                    sb.Append("SELECT " +
                     " [posts].[post_user_id] " +
                      "FROM [dbo].[posts] " +
                              "WHERE [post_id] = " + this.post_id
                     );


                    String sql = sb.ToString();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (!reader.HasRows)
                            {
                                return new User(false, "Could not find post");
                            }
                            else
                            {
                                int user_id;
                                while (reader.Read())
                                {
                                    user_id = reader.GetInt32(0);

                                    if(user_id == 0) {
                                        user = new User(false, "User has no account");
                                    } else {
                                        user = new User(true, user_id, "");
                                    }
                                }

                                return user;
                            }
                        }
                    }
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
                return user = new User(false, e.ToString());
            }
        }


    }
}
