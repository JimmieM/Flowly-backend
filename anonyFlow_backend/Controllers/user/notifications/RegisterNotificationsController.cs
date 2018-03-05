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
    [Route("api/user/notifications/[controller]")]
    public class RegisterNotificationsController : Controller
    {

        [HttpPost]
        public Response Post([FromBody]dynamic obj)
        {
            int user_id = obj.user_id;
            string platform = obj.platform;
            string device_token = obj.device_token;

            SqlConnectionStringBuilder builder = WebApiConfig.Connection();

            try
            {
                string sql = "UPDATE users SET user_platform = @platform, user_device_token = @device_token WHERE user_id = @user_id";

                Console.WriteLine(sql);
                using (SqlConnection conn = new SqlConnection(builder.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@platform", platform);
                        cmd.Parameters.AddWithValue("@device_token", device_token);
                        cmd.Parameters.AddWithValue("@user_id", user_id);

                        cmd.ExecuteNonQuery();

                        return new Response(true, "");
                    }
                }
            }
            catch (SqlException ex)
            {
                string msg = "Update Error:";
                msg += ex.Message;
                return new Response(false,true,msg);
            }


        }

    }
}
