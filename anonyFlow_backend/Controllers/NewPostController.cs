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
    public class PostData {
        string post_user_token;
        string post_content;
        string post_location;
        string post_date;

        public PostData(string post_user_token, string post_content, string post_location) {
            this.post_user_token = post_user_token;
            this.post_content = post_content;
            this.post_location = post_location;
            this.post_date = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
            return;
        }
    }

    [Route("api/[controller]")]
    public class NewPostController : Controller
    {
        // POST api/newpost
        [HttpPost]
        public Response Post([FromBody]dynamic post)
        {
            var response = new List<Response>();

            string post_content = post.post_content;
            int post_user_id = post.post_user_id;
            int post_topic_id = post.post_topic_id;
            string post_country = post.post_geolocations.post_country;
            string post_locality = post.post_geolocations.post_locality;
            string post_by_geo = post.post_by_geo;
                
            Console.WriteLine(post);

            SqlConnectionStringBuilder builder = WebApiConfig.Connection();

            try
            {
                string sql = "INSERT INTO posts " +
                    "(post_user_id," +
                    "post_content," +
                    "post_date," +
                    "post_dislikes," +
                    "post_likes," +
                    "post_topic_id," +
                    "post_locality," +
                    "post_country)" +
                    " VALUES " +
                    "(@post_user_id," +
                    "@post_content," +
                    "@post_date," +
                    "1," +
                    "1," +
                    "@post_topic_id," +
                    "@post_locality," +
                    "@post_country)".ToString();
                
                Console.WriteLine(sql);
                using (SqlConnection conn = new SqlConnection(builder.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@post_user_id", post_user_id);
                        cmd.Parameters.AddWithValue("@post_content", post_content);
                        cmd.Parameters.AddWithValue("@post_date", DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss").ToString());
                        cmd.Parameters.AddWithValue("@post_topic_id", post_topic_id);

                        if(post_by_geo == "none") {
                            cmd.Parameters.AddWithValue("@post_locality", DBNull.Value); 
                            cmd.Parameters.AddWithValue("@post_country", DBNull.Value); 
                        } else {
                            cmd.Parameters.AddWithValue("@post_locality", post_locality);
                            cmd.Parameters.AddWithValue("@post_country", post_country);
                        }

                        cmd.ExecuteNonQuery();

                        return new Response(true, "");
                    }
                }
            }
            catch (SqlException ex)
            {
                string msg = "Insert Error:";
                msg += ex.Message;
                return new Response(false, true, msg);
            }

        }
    }
}
