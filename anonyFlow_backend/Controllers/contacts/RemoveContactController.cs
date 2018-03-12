using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace anonyFlow_backend.Controllers
{
    [Route("api/contacts/[controller]")]
    public class RemoveContactController : Controller
    {
        [HttpPost]
        public Response Post([FromBody]dynamic obj)
        {
            var user_id = obj.user_id; // yours
            var contact_user_id = obj.contact_user_id; // to be deleted

            SqlConnectionStringBuilder builder = WebApiConfig.Connection();

            try
            {
                string sql = "DELETE FROM contacts " +
                    "WHERE " +
                    "(contact_user_id = " + user_id + " AND contact_with_user_id = " + contact_user_id + ") " +
                    "OR " +
                    "(contact_user_id = " + contact_user_id + " AND contact_with_user_id = " + user_id + ")";

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
                string msg = "Insert Error:";
                msg += ex.Message;
                return new Response(false, msg);
            }
        }
    }
}
