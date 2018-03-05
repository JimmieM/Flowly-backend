using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace anonyFlow_backend.Controllers
{
    [Route("api/beta_test/[controller]")]
    public class GeoLocationQAController : Controller
    {
        [HttpPost]
        public Response Post([FromBody]dynamic obj)
        {
            int is_correct = obj.geoloc_is_correct;
            string comments = obj.geoloc_comments;
            string user_identifier = obj.geoloc_user_identifier;

            Console.WriteLine(obj);

            SqlConnectionStringBuilder builder = WebApiConfig.Connection();

            try
            {
                string sql = "INSERT INTO beta_geolocation (geo_loc_is_correct, geo_loc_comments, geo_loc_user_identifier) VALUES (@is_correct, @comments, @user_identifier)";

                using (SqlConnection conn = new SqlConnection(builder.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@is_correct", is_correct);
                        cmd.Parameters.AddWithValue("@comments", comments);
                        cmd.Parameters.AddWithValue("@user_identifier", user_identifier);

                        cmd.ExecuteNonQuery();

                        return new Response(true, "");
                    }
                }
            }
            catch (SqlException ex)
            {
                string msg = "Insert error: ";
                msg += ex.Message;
                return new Response(false, true, msg);
            }
        }
    }
}
