using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace anonyFlow_backend.Controllers
{
    [Route("api/[controller]")]
    public class RemoveContentController : Controller
    {

        public int user_id;
        public string content_table;
        public int content_id;

        public RemoveContentController(int user_id, string content_table, int content_id) {
            this.user_id = user_id;
            this.content_id = content_id;
            this.content_table = content_table;
        }

        public RemoveContentController(string content_table, int content_id)
        {
            this.content_id = content_id;
            this.content_table = content_table;
        }

        public Response removeContent() {
            SqlConnectionStringBuilder builder = WebApiConfig.Connection();

            try
            {
                string sql = "DELETE FROM " + this.content_table + "s WHERE " + this.content_table + "_id" + " = " + this.content_id; 

                Console.WriteLine(sql);

                using (SqlConnection conn = new SqlConnection(builder.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.ExecuteNonQuery();

                        return new Response(true, "");
                    }
                }
            }
            catch (SqlException ex)
            {
                string msg = "Deletion error: ";
                msg += ex.Message;
                return new Response(false, msg);
            }
        }

        /*
         * if 
        */
        private bool isOriginPoster()
        { return false; }

        [HttpPost]
        public Response Post([FromBody]dynamic obj)
        {
            this.user_id = obj.user_id;
            this.content_id = obj.content_id;
            this.content_table = obj.content_table;

            bool can_delete = this.isOriginPoster();

            if(can_delete) {
                
                try {
                    this.removeContent();    
                } catch(Exception e) {
                    
                }
            }

            return new Response(false, false, "Missing implementation");

        }

    }
}
