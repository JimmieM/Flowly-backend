using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace anonyFlow_backend.Controllers.abuse
{

    // subset class of Response to hold all collected reports regarding a user.
    public class CollectedReportsObject : Response
    {

        public int? user_id { get; set; } // the user_id or 0 if the user wasnt logged in..
        public int reports_received { get; set; } // how many times a report has been filed agianst user.
        public List<ReportClass> reports { get; set; }

        // with user_id
        public CollectedReportsObject(Response response, int user_id, int reports_recieved, List<ReportClass> reports)
            : base (response.success, response.error, response.message) {
            this.user_id = user_id;
            this.reports_received = reports_received;
            this.reports = reports;
        }

        // without a user id
        public CollectedReportsObject(Response response, int reports_recieved, List<ReportClass> reports)
            : base (response.success, response.error, response.message) 
        {
            this.reports_received = reports_received;
            this.reports = reports;
        }

        // if zero results or an error.
        public CollectedReportsObject(Response response)
        : base(response.success, response.error, response.message)
        {}
    }

    public class CollectReportsClass
    {
        Modules modules = new Modules();
        SqlConnectionStringBuilder builder = WebApiConfig.Connection();
        StringBuilder sb;

        public int? user_id; // should be able to be nullable.
        public string table; // Can not be nullable
        public int id; // can not be nullable


        // will return amount of reports filed of the user, if post/comment is 
        // created by a user.

        // Collect reports based on either a user, or based on a post/comment.



        // return all report in collected reports, then scenario ON report a post -> decide to ban user, if user is loged in.
        public CollectReportsClass(string table, int id, int user_id)
        {
            this.table = table;
            this.id = id;
            this.user_id = user_id;
        }

        // return all reports in Collected reports, then scenario if Filter -> then filter if x amount of reports.
        public CollectReportsClass(string table, int id)
        {
            this.table = table;
            this.id = id;
        }

        // ?
        public CollectReportsClass(int user_id)
        {
            this.user_id = user_id;
        }

        // return based on the instance constructor of class
        public CollectedReportsObject returnReports() {
            // store each report in class instance of ReportClass
            var reports = new List<ReportClass>();

            Response resp; // response instance

            try
            {
                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();
                    sb = new StringBuilder();

                    sb.Append("SELECT [report_id], [report_by_user_id], [report_table], [report_table_id], [report_user_id] FROM [dbo].[reports]");
                        
                    // select based on only a post/comment ID
                    if(this.user_id == null) {
                        // select each report where this user_id has been reported.
                        sb.Append(" WHERE report_user_id = " + this.user_id);
                    } else {
                        // else, select where applied post/comment ID has been reported.
                        sb.Append(" WHERE report_table = '" + this.table + "' AND report_table_id = " + this.id);
                    }

                    String sql = sb.ToString();

                    Console.WriteLine(sql);

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (!reader.HasRows)
                            {
                                // no errors received, but we didnt get any results.
                                resp = new Response(false, false, "No results");
                                return new CollectedReportsObject(resp);
                            }
                            else
                            {
                                int reports_received_counter = 0;


                                while (reader.Read())
                                {
                                    // use correct ReportClass constructor
                                    if (this.user_id == null)
                                    {
                                        // get amount of times this post/comment id has been reported.
                                        // increment value.
                                        reports_received_counter++;

                                    } else {
                                        
                                        reports_received_counter++;
                                        // else add all reports pointed to assigned user id
                                        reports.Add(
                                         new ReportClass(
                                             reader.GetInt32(4),
                                             reader.GetString(2).ToString(),
                                             reader.GetInt32(3)
                                         ));
                                    }
                      
                                }

                                // the operation was succesfull.
                                // success = true, error = false, message = none to give.
                                resp = new Response(true, false, "");

                                // return ReportClass List with 1 object, with first contructor of ReportClass
                                if(this.user_id == null) {

                                    // add object to report list

                                    // 1 object is only needed, since we're counting the same post/comment being reported for abuse.
                                    reports.Add(
                                        new ReportClass(
                                            this.table,
                                            this.id,
                                            reports_received_counter
                                        )
                                    );

                                    return new CollectedReportsObject(resp, reports_received_counter, reports);
                                }

                                // else the operation was to collect all reports pointed to user id
                                resp = new Response(true, false, "");
                                return new CollectedReportsObject(resp, this.user_id.GetValueOrDefault(), reports_received_counter, reports);
                            }
                        }
                    }
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());

                // return errors.
                resp = new Response(false, true, e.ToString());
                return new CollectedReportsObject(resp);
            }

        }
    }
}
