using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.Contracts;

/*
 * Class developed for users to report a post or comment. Where comment is bound to post.
 * 
 * - A method for filtering objectionable content - implement in flowcontroller
 * - A mechanism for users to flag objectionable content - implement self
 * - A mechanism for users to block abusive users - implement in ReportUserController and self.
 * - The developer must act on objectionable content reports within 24 hours by removing the content and ejecting the user who provided the offending content 
 * 
 */

namespace anonyFlow_backend.Controllers
{

    public class ReportAbuseByPost {
        public int reportee_user_id; // the user reporting
        public int reported_user_id; // the user of abuse
        public string report_table;
        public int report_table_id;
        public string report_reason;

        public ReportAbuseByPost(
            int reportee_user_id, 
            int reported_user_id, 
            string report_table, 
            int report_table_id,
            string report_reason
        ) {
            this.reportee_user_id = reportee_user_id;
            this.reported_user_id = reported_user_id;
            this.report_table = report_table;
            this.report_table_id = report_table_id;
            this.report_reason = report_reason;
        }

        public Response report() {
            SqlConnectionStringBuilder builder = WebApiConfig.Connection();

            try
            {
                string sql = "INSERT INTO reports (report_by_user_id, report_table, report_table_id, report_user_id, report_date, report_reason) VALUES (@report_by_user_id, @report_table, @report_table_id, @report_user_id, @report_date, @report_reason)";

                Console.WriteLine(sql);
                using (SqlConnection conn = new SqlConnection(builder.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@report_by_user_id", this.reportee_user_id);
                        cmd.Parameters.AddWithValue("@report_table", this.report_table);
                        cmd.Parameters.AddWithValue("@report_table_id", this.report_table_id);
                        cmd.Parameters.AddWithValue("@report_date", DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
                        cmd.Parameters.AddWithValue("@report_user_id", this.reported_user_id);
                        cmd.Parameters.AddWithValue("@report_reason", this.report_reason);

                        cmd.ExecuteNonQuery();

                        return new Response(true, "");
                    }
                }
            }
            catch (SqlException ex)
            {
                string msg = "Insert Error: ";
                msg += ex.Message;
                throw new Exception(msg.ToString());
                //return new Response(false, msg);
            }

        }
    }

    [Route("api/abuse/[controller]")]
    public class ReportPostContentController : Controller
    {

        private int maximum_reports = 15;

        [HttpPost]
        public Response Post([FromBody]dynamic obj)
        {
            Contract.Ensures(Contract.Result<Response>() != null);
            int user_id = obj.user_id; // 1
            int user_reported_id = obj.user_content_creator_id;
            int table_id = obj.table_id; // 1 
            string table = obj.table; // 'post'
            string reason = obj.reason;
            // a report of post id 1 has been filed.

            Console.WriteLine(obj);

            try {
                // File the report into system
                ReportAbuseByPost new_report = new ReportAbuseByPost(user_id, user_reported_id, table, table_id, reason);
                Response report = new_report.report();

                Console.WriteLine(report.success);
            } catch(Exception e) {
                Console.WriteLine(e);   
            }

            // if its an account
            if(user_reported_id != 0) {
                // check if the user is going to be banned based on past reports.
                // call collectReportsClass
                CollectReportsClass collection = new CollectReportsClass(table, table_id, user_id);
                CollectedReportsObject returnable = collection.returnReports();

                if (returnable.success)
                {
                    var returnable_reports_received = returnable.reports_received;

                    // ban the user
                    if (returnable_reports_received >= this.maximum_reports)
                    {
                        BanUserClass ban_blass = new BanUserClass(user_reported_id);

                        Response ban = ban_blass.BanUser();
                    }
                }
            }

            return new Response(true, false, "");
        }
    }
}
