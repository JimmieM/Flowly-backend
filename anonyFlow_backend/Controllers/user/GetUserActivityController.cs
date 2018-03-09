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
    public class UserActivityResponse {
        public bool success { get; set; }
        public bool error { get; set; }
        public string message { get; set; }

        public List<UserActivity> post_activity { get; set; }
        public List<UserActivity> comment_activity { get; set; }

        public UserActivityResponse(
            bool success,
            List<UserActivity> post_activity,
            List<UserActivity> comment_activity)
        {
            this.success = success;
            this.post_activity = post_activity;
            this.comment_activity = comment_activity;
            return;
        }

        public UserActivityResponse(bool success, bool error, string message) {
            this.success = success;
            this.error = error;
            this.message = message;
        }
    }

    public class UserActivity {
        public bool error { get; set; }
        public string error_message { get; set; }

        public int post_id { get; set; }
        public string date { get; set; }
        public int comment_id { get; set; }
        public string post_content { get; set; }
        public string comment_content { get; set; }


        // failure definition
        public UserActivity(bool error, string error_message)
        {
            this.error = error;
            this.error_message = error_message;
            return;
        }

        // created a post
        public UserActivity(int post_id, string date, string post_content) {
            this.post_id = post_id;
            this.date = date;
            this.post_content = post_content;
            return;
        }

        // created a comment
        public UserActivity(int post_id, string date, int comment_id, string comment_content) {
            this.post_id = post_id;
            this.date = date;
            this.comment_id = comment_id;
            this.comment_content = comment_content;
            return;
        }
    
    }

    [Route("api/user/[controller]")]
    public class GetUserActivityController : Controller
    {
        SqlConnectionStringBuilder builder = WebApiConfig.Connection();
        Modules modules = new Modules();

        public int user_id;

        [HttpPost]
        public UserActivityResponse Post([FromBody]dynamic obj)
        {
            this.user_id = obj.user_id;


            try {
                List<UserActivity> comments = this.getComments();
                List<UserActivity> posts = this.getPosts();

                return new UserActivityResponse(true, posts, comments);
            } catch(SqlException e) {
                
                return new UserActivityResponse(false, true, e.ToString());
            }
        }

        public List<UserActivity> getComments() {

            List<UserActivity> comment_activity = new List<UserActivity>();
            try
            {
                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();
                    StringBuilder sb = new StringBuilder();

                    sb.Append("SELECT TOP (50) comment_post_id, comment_date, comment_id, comment_content FROM comments WHERE comment_user_id = " + this.user_id);
                    sb.Append(" ORDER BY comment_date DESC");

                    String sql = sb.ToString();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    var date = modules.betweenDates(reader.GetString(1).ToString());
                                    comment_activity.Add(
                                        new UserActivity(
                                            reader.GetInt32(0),
                                            date,
                                            reader.GetInt32(2),
                                            reader.GetString(3)));
                                }
                            }
                            else
                            {
                                return comment_activity;
                            }

                            return comment_activity;
                        }
                    }
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine("SELECT ERROR: " + e.ToString());
                comment_activity.Add(
                     new UserActivity(
                         true,
                        e.ToString()));

                return comment_activity;
            }
        }

        public List<UserActivity> getPosts() {
            
            List<UserActivity> post_activity = new List<UserActivity>();

            try
            {
                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();
                    StringBuilder sb = new StringBuilder();

                    sb.Append("SELECT TOP (50) post_id, post_date, post_content FROM posts WHERE post_user_id = " + this.user_id);
                    sb.Append(" ORDER BY post_date DESC");

                    String sql = sb.ToString();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows) {
                                while(reader.Read()) {
                                    
                                    var date = modules.betweenDates(reader.GetString(1).ToString());
                                    post_activity.Add(
                                        new UserActivity(
                                            reader.GetInt32(0),
                                            date,
                                            reader.GetString(2)));
                                }
                            }
                            else
                            {
                                return post_activity;
                            }

                            return post_activity;
                        }
                    }
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine("SELECT ERROR: " + e.ToString());
                post_activity.Add(
                     new UserActivity(
                         true,
                        e.ToString()));

                return post_activity;

            }
        }
    }
}
