using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace anonyFlow_backend.Controllers
{
    public class comment
    {
        public int comment_id { get; set; }
        public int comment_user_id { get; set; }
        public string comment_content { get; set; }
        public int comment_likes { get; set; }
        public int comment_dislikes { get; set; }
        public string comment_date { get; set; }

        public comment(int comment_id, int comment_user_id, string comment_content, string comment_date, int comment_likes, int comment_dislikes) {
            this.comment_id = comment_id;
            this.comment_user_id = comment_user_id;
            this.comment_content = comment_content;
            this.comment_date = comment_date;

            this.comment_likes = comment_likes;
            this.comment_dislikes = comment_dislikes;
            return;
        }
    }

    public class flow
    {
        public int post_id { get; set; }
        public int post_user_id { get; set; }

        public string post_content { get; set; }
        public string post_date { get; set; }

        public int post_likes { get; set; } 
        public int post_dislikes { get; set; } 

        public int post_topic_id { get; set; } 

        public string post_locality { get; set; }
        public string post_country { get; set; }

        // define by each comment connect to current post
        public List<comment> post_comments { get; set; } // from comments table

        public flow
        (
            int post_id,
            int post_user_id,
            string post_content,
            string post_date,
            int post_dislikes,
            int post_likes,
            int post_topic_id,
            string post_locality,
            string post_country,
            List<comment> post_comments
        )
        {
            this.post_id = post_id;
            this.post_user_id = post_user_id;
            this.post_content = post_content;
            this.post_date = post_date;
            this.post_dislikes = post_dislikes;
            this.post_likes = post_likes;
            this.post_topic_id = post_topic_id;
            this.post_locality = post_locality;
            this.post_country = post_country;
            this.post_comments = post_comments;
            return;
        }
    }

    [Route("api/[controller]")]
    public class FlowController : Controller
    {
        Modules modules = new Modules();
        SqlConnectionStringBuilder builder = WebApiConfig.Connection();
        StringBuilder sb;

        public Response getFlow(string flow_type, string posts_by) {

            Console.WriteLine("Params:" + flow_type + " " + posts_by);

            var flow = new List<flow>();

            try
            {
                // include connection..

                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();
                    sb = new StringBuilder();

                    sb.Append("SELECT TOP (50)" +
                     " [posts].[post_id]," +
                     " [posts].[post_user_id]," +
                      " [posts].[post_content]," +
                      " [posts].[post_date]," +
                      " [posts].[post_dislikes]," +
                      " [posts].[post_likes]," +
                      " [posts].[post_topic_id]," +
                              " [posts].[post_locality]," +
                              " [posts].[post_country]" +
                      " FROM [dbo].[posts]"
                     );

                    sb.Append(" WHERE [post_topic_id] = " + topic_id);

                    if (flow_type != null) {

                        if (flow_type == "popular") {
                            sb.Append(" AND [posts].[post_likes] >= 20");
                        }
                        else if (flow_type == "unpopular") {
                            sb.Append(" AND [posts].[post_dislikes] >= 5");
                        }
                        else {
                            if(flow_type != "normal") {
                                return new Response(false, "Failed to initiate query");    
                            }

                        }
                    }

                    if (posts_by == "none") {
                        //sb.Append(" AND [posts].[post_country] IS NULL AND [posts].[post_locality] IS NULL");
                    }
                    else if (posts_by == "locality") {
                        sb.Append(" AND [posts].[post_locality] = '" + post_locality + "'");

                    }
                    else if (posts_by == "country") {
                        sb.Append(" AND [posts].[post_country] = '" + post_country + "'");
                    }
                    else {
                        return new Response(false, "No geolocation received!");
                    }

                    sb.Append(" ORDER BY post_date DESC");

                    String sql = sb.ToString();

                    Console.WriteLine(sql);

                    using (SqlCommand command = new SqlCommand(sql, connection)) {
                        using (SqlDataReader reader = command.ExecuteReader()) {
                            if (!reader.HasRows) {
                                return new Response(false, "Query found 0 results");
                            }
                            else {
                                while (reader.Read()) {

                                    var date = modules.betweenDates(reader.GetString(3).ToString());
                                    List<comment> Comments = GetComments(reader.GetInt32(0));
                                    flow.Add(new flow(
                                        reader.GetInt32(0),
                                        reader.GetInt32(1),
                                        reader.GetString(2).ToString(),
                                        date,
                                        reader.GetInt32(4),
                                        reader.GetInt32(5),
                                        reader.GetInt32(6),
                                        reader.GetValue(7).ToString(),
                                        reader.GetValue(8).ToString(),
                                        Comments
                                    ));
                                }
                            }
                        }
                    }
                }
            } catch (SqlException e) {
                Console.WriteLine(e.ToString());
                return new Response(false, e.ToString());
            }

            return new Response(true, "", flow);
        }

        public string username;

        public int topic_id;
        public string post_locality;
        public string post_country;

        // POST api/values
        [HttpPost]
        public Response Post([FromBody]dynamic obj) {
            Console.WriteLine(obj);

            int user_id = obj.user_id;
            var flow_type = obj.flow_type;
            var post_id = obj.post_id;
            this.topic_id = obj.topic_id;

            Console.WriteLine("Topic id:" + this.topic_id);
            var posts_by = obj.posts_by.ToString(); // country, none, city. None defines to get all posts with NULL country and locality(city)
            this.post_locality = obj.post_geolocations.locality;
            this.post_country = obj.post_geolocations.country;

   
            if(flow_type == "all") {
                Console.WriteLine("ALL");

                var normal = getFlow("normal", posts_by);
                var popular = getFlow("popular", posts_by);
                var unpopular = getFlow("unpopular", posts_by);

                Console.WriteLine("Goes");

                Console.WriteLine(normal.success);

                List<flow> x = normal.flow_response_dynamic; 
                List<flow> y = popular.flow_response_dynamic;
                List<flow> z = unpopular.flow_response_dynamic;

                return new Response(true, "", x,y,z);

               
            } else {
                var dynamic = getFlow(flow_type, posts_by);

                if(dynamic.success) {
                    return new Response(true, "", dynamic.flow_response_dynamic);
                }
            }

            return new Response(false, true, "");
        }

        public List<comment> GetComments(int postId) {
            var comments = new List<comment>();

            try {

                using (SqlConnection connection = new SqlConnection(builder.ConnectionString)) {
                    connection.Open();
                    StringBuilder sb = new StringBuilder();

                    sb.Append("SELECT " +
                      " [comments].[comment_id]," +
                      " [comments].[comment_user_id]," +
                      " [comments].[comment_content]," +
                      " [comments].[comment_date]," +
                              " [comments].[comment_likes]," +
                      " [comments].[comment_dislikes]" +
                      " FROM [dbo].[comments]" +
                      "WHERE [comments].[comment_post_id] LIKE " + postId +
                              "ORDER BY comment_date ASC"
                     );

                    String sql = sb.ToString();

                    using (SqlCommand command = new SqlCommand(sql, connection)) {
                        using (SqlDataReader reader = command.ExecuteReader()) {
                            while (reader.Read()) {
                                
                                string date = modules.betweenDates(reader.GetString(3).ToString());

                                comments.Add(new comment(
                                    reader.GetInt32(0),                                    
                                    reader.GetInt32(1),
                                    reader.GetString(2).ToString(),
                                    date,
                                    reader.GetInt32(4),
                                    reader.GetInt32(5)
                                ));
                            }
                        }
                    }
                }
            } catch (SqlException e) {
                Console.WriteLine(e.ToString());
            }

            return comments;
        }
    }
}
