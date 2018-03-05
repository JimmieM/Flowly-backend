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
    public class ValidationResponse {
        public bool success { get; set; }
        public bool failure { get; set; }

        public string message { get; set; }

        public ValidationResponse(bool success, bool failure, string message) {
            this.success = success;
            this.failure = failure;
            this.message = message;
        }

        public ValidationResponse(bool success, string message)
        {
            this.success = success;
            this.message = message;
        }
    }

    /*
     * - middleware class to use on controllers.
     * 
     * Each controller where: 
     * 1. User ID has to be controlled.
     * 2. Token has to be controlled.
    */
    public class PostValidationClass
    {
        public int user_id;
        public string token;
        public ValidationResponse authenticate(int user_id, string token)
        {
            this.user_id = user_id;
            this.token = token;

            ValidationResponse tokenAuth = this.tokenAuthentication();
            ValidationResponse userAuth = this.userAuthentication();

            if(tokenAuth.success && userAuth.success) {
                return new ValidationResponse(true, "");    
            } else {
                if(tokenAuth.failure) {
                    return new ValidationResponse(false, tokenAuth.message);
                } else if(userAuth.failure) {
                    return new ValidationResponse(false, userAuth.message);
                } else {
                    return new ValidationResponse(false, "");
                }
            }
        }

        private ValidationResponse userAuthentication() {
            try
            {
                // include connection..
                SqlConnectionStringBuilder builder = WebApiConfig.Connection();

                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();
                    StringBuilder sb = new StringBuilder();

                    sb.Append("SELECT " +
                     " [users].[user_banned] " +
                      "FROM [dbo].[users] " +
                      "WHERE [user_id] = " + this.user_id + 
                      " AND [user_banned] = 0"
                     );


                    String sql = sb.ToString();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if(reader.HasRows) {
                                return new ValidationResponse(true, "");
                            } 

                            return new ValidationResponse(false, true, "You're banned");
                        }
                    }
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
                return new ValidationResponse(false, true, e.ToString());
            }
        }

        private ValidationResponse tokenAuthentication() {
            if(this.token == "aadsa--djja33_#11@abd") {
                return new ValidationResponse(true, "");
            }
            return new ValidationResponse(false, true, "Failed to authenticate request");
        }
    }
}
