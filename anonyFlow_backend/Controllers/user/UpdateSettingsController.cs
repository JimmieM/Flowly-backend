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
    [Route("api/user/[controller]")]
    public class UpdateSettingsController : Controller
    {
        [HttpPost]
        public Response Post([FromBody]dynamic obj) {
            int user_id = obj.user_id;
            string column = obj.column;
            string value = obj.value;

            SqlConnectionStringBuilder builder = WebApiConfig.Connection();

            try
            {
                string sql = "UPDATE users SET @column = @value WHERE user_id = @user_id";

                Console.WriteLine(sql);
                using (SqlConnection conn = new SqlConnection(builder.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@user_id", user_id);
                        cmd.Parameters.AddWithValue("@column", column);
                        cmd.Parameters.AddWithValue("@value", value);

                        cmd.ExecuteNonQuery();

                        return new Response(true, "");
                    }
                }
            }
            catch (SqlException ex)
            {
                string msg = "Insert Error:";
                msg += ex.Message;
                return new Response(false, msg);
            }
           
        }
    }
}
