using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace anonyFlow_backend.Controllers.user
{
    [Route("api/contacts/[controller]")]
    public class AnswerContactController : Controller
    {
        [HttpPost]
        public Response Post([FromBody]dynamic obj)
        {
            int user_id = obj.user_id; // your id
            string username = obj.username; // your username
            int contact_user_id = obj.contact_user_id; // the requestee ID
            int accept = obj.answer;

            SqlConnectionStringBuilder builder = WebApiConfig.Connection();

            try
            {
                string sql;
                if(accept == 1) {
                    sql = "UPDATE contacts SET contact_accepted = 1 WHERE contact_with_user_id = @user_id AND contact_user_id = @contact_user_id";
                } else {
                    sql = "DELETE FROM contacts WHERE contact_with_user_id = @user_id AND contact_user_id = @contact_user_id";
                }

                using (SqlConnection conn = new SqlConnection(builder.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        if(accept == 1) {
                            cmd.Parameters.AddWithValue("@user_id", user_id);
                            cmd.Parameters.AddWithValue("@contact_user_id", contact_user_id);
                        }

                        // message, open page on app
                        PushNotificationObject push_object = new PushNotificationObject(username + " has accepted your contact request!", "profile");

                        NewNotificationClass newNotification = new NewNotificationClass(contact_user_id, push_object);

                        Response push = newNotification.createPush();

              
                        cmd.ExecuteNonQuery();

                        return new Response(true, false, "");
                    }
                }
            }
            catch (SqlException ex)
            {
                string msg = "Update: ";
                msg += ex.Message;
                return new Response(false, true, msg);
            }
        }
    }
}
