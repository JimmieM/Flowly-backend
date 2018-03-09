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
    public class CreateContactController : Controller
    {

        public int user_id;
        public string token;
        public int user_contact_id; // the id of the contact you're adding
        public int met_by_post_id;

        public Response alreadyContact(int your_id, int contact_id) {
            try
            {
                // include connection..
                SqlConnectionStringBuilder builder = WebApiConfig.Connection();

                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();
                    StringBuilder sb = new StringBuilder();

                    sb.Append("SELECT contact_id FROM[dbo].[contacts] " +
                              "WHERE " +
                              "(contact_user_id = " + your_id + " AND contact_with_user_id = " + contact_id + ") " +
                              "OR " +
                              "(contact_user_id = " + your_id + " AND contact_with_user_id = " + contact_id + ")");

                    String sql = sb.ToString();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            Console.WriteLine(reader.HasRows);
                            return new Response(reader.HasRows, "");
                        }
                    }
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
                return new Response(false, e.ToString());
            }
            
        }

        public Response requestConnection() {
            

            if (user_id == user_contact_id)
            {
                return new Response(false, "You can not add yourself.");
            }

            if (alreadyContact(user_id, user_contact_id).success)
            {
                return new Response(false, "You're already connected with this user");
            }


            List<Response> response = new List<Response>();

            SqlConnectionStringBuilder builder = WebApiConfig.Connection();

            try
            {
                string sql = "INSERT INTO contacts (contact_user_id, contact_with_user_id, contact_accepted, contact_met_by_post_id, contact_since) VALUES (@user_id, @user_contact_id, 0, @met_by_post_id, @contact_since)";

                Console.WriteLine(sql);
                using (SqlConnection conn = new SqlConnection(builder.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@user_id", user_id);
                        cmd.Parameters.AddWithValue("@user_contact_id", user_contact_id);
                        cmd.Parameters.AddWithValue("@met_by_post_id", met_by_post_id);
                        cmd.Parameters.AddWithValue("@contact_since", DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));


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

        [HttpPost]
        public Response Post([FromBody]dynamic obj)
        {
            this.user_id = obj.user_id;
            this.token = obj.token;
            this.user_contact_id = obj.user_contact_id; // the id of the contact you're adding
            this.met_by_post_id = obj.met_by_post_id; // a post ID

            //PostValidationClass postVal = new PostValidationClass();
            //ValidationResponse auth = postVal.authenticate(user_id, token);
            //if(!auth.success) {
            //    return new Response(false, auth.message);
            //}

            Response request = this.requestConnection();

            if(request.success) {
                PushNotificationObject push_object = new PushNotificationObject("You have a new contact request");

                NewNotificationClass newNotification = new NewNotificationClass(user_contact_id, push_object);

                Response push = newNotification.createPush();

                return new Response(true, false, "");
            }

            return new Response(false, true, request.message);
        }

    }
}
