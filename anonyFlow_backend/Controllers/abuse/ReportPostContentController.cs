using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;

/*
 * Class developed for users to report a post or comment. Where comment is bound to post.
 * 
 * - A method for filtering objectionable content - implement in flowcontroller
 * - A mechanism for users to flag objectionable content - implement self
 * - A mechanism for users to block abusive users - implement in ReportUserController and self.
 * - The developer must act on objectionable content reports within 24 hours by removing the content and ejecting the user who provided the offending content 
 * 
 */

namespace anonyFlow_backend.Controllers.abuse
{




    [Route("api/abuse/[controller]")]
    public class ReportPostContentController : Controller
    {
        [HttpPost]
        public Response Post([FromBody]dynamic obj)
        {
            var user_id = obj.user_id; // 1
            var table_id = obj.table_id; // 1 
            var table = obj.table; // 'post'
            // a report of post id 1 has been filed.



        }




    }
}
