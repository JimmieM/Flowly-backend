using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace anonyFlow_backend.Controllers
{
    // returnable object - contains information about requested user
    public class Profile
    {
        public int user_id { get; set; } // redundant variable. Often already declared in app
        public string username { get; set; } // redundant variable. Often already declared in app

        public int met_by_post_id { get; set; } // where you ViewUserProfile.{this.user_id} and Profile instance met.
        public string contacts_since { get; set; }

        public int user_thanks { get; set; } // how many "thanks" the user has gotten. By posts or comments.

        public Profile() {}

        public Profile(
            int user_id,
            string username,
            int met_by_post_id,
            string contacts_since,
            int user_thanks) 
        {
            this.user_id = user_id;
            this.username = username;
            this.met_by_post_id = met_by_post_id;
            this.contacts_since = contacts_since;
            this.user_thanks = user_thanks;
        }


    }

    [Route("api/user/[controller]")]
    public class ViewUserProfileController : Controller
    {
        public int user_id;
        public int profile_user_id;

        Modules modules = new Modules();
        SqlConnectionStringBuilder builder = WebApiConfig.Connection();

        [HttpPost]
        public Response Post([FromBody]dynamic obj)
        {
            this.user_id = obj.user_id;
            this.profile_user_id = obj.profile_user_id;

            return this.getProfile();
        }

        public Response getProfile() {
            try
            {
                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();

                    StringBuilder sb = new StringBuilder();

                    sb.Append("SELECT " +
                              "[users].[user_name], " +
                              "[users].[user_thanks], " +
                              "[contacts].[contact_since], " +
                              "[contacts].[contact_met_by_post_id] " +
                              "FROM " +
                              "[users], " +
                              "[contacts] " +
                              "WHERE " +
                              "[users].[user_id] = " + this.profile_user_id + " " +
                              "AND (contact_user_id = " + this.profile_user_id + " AND contact_with_user_id = " + this.user_id + ") " +
                              "OR " +
                              "(contact_user_id = " + this.user_id + " AND contact_with_user_id = " + this.profile_user_id + ")");

                    String sql = sb.ToString();
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                Profile p = new Profile();
                                while (reader.Read())
                                {
                                    string date = modules.betweenDates(reader.GetString(2));
                                    p = new Profile(
                                        this.profile_user_id,
                                        reader.GetString(0),
                                        reader.GetInt32(1),
                                        date,
                                        reader.GetInt32(3));
                                }

                                return new Response(true, "", p);
                            }
                            else
                            {
                                return new Response(false, "User does not exist!");
                            }
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
    }
}
