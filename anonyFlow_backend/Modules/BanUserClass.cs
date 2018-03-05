using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace anonyFlow_backend.Controllers
{
    public class BanUserClass
    {
        public int user_id;
        public BanUserClass(int user_id)
        {
            this.user_id = user_id;
        }

        public Response BanUser() {
            SqlConnectionStringBuilder builder = WebApiConfig.Connection();

            try
            {
                string sql = "UPDATE users SET user_banned = 1 WHERE user_id = @user_id";

                using (SqlConnection conn = new SqlConnection(builder.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@user_id", this.user_id);
       
                        cmd.ExecuteNonQuery();

                        return new Response(true, "");
                    }
                }
            }
            catch (SqlException ex)
            {
                string msg = "Update error: ";
                msg += ex.Message;
                return new Response(false, true, msg);
            }
        }
    }
}
