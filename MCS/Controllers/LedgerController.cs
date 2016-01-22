using MCS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Web;
using System.Web.Mvc;

namespace MCS.Controllers
{
    public class LedgerController : Controller
    {
        private MoldControlEntities db = new MoldControlEntities();

        [Chk_Authen]
        public ActionResult Index(string mold_code)
        {
            var mold = (from a in db.td_mold
                        where a.mold_code == mold_code
                        select a).FirstOrDefault();
            return View(mold);
        }

        [HttpPost]
        public JsonResult LedgerList(string mold_code, int jtStartIndex = 0, int jtPageSize = 0, string jtSorting = null)
        {
            try
            {
                var query = from a in db.td_ledger
                            where a.mold_code == mold_code
                            select a;

                //Get data from database
                int TotalRecord = query.Count();

                // Paging
                var output = query
                    .Select(s => new
                    {
                        s.mold_code,
                        s.cavity_no,
                        s.receive_date,
                        s.qty,
                        s.description,
                        s.source,
                        s.result,
                        s.invoice_no,
                        s.price,
                        s.pic_code,
                        s.pic_mold,
                        s.status_id
                    }).OrderBy(jtSorting).Skip(jtStartIndex).Take(jtPageSize);

                //Return result to jTable
                return Json(new { Result = "OK", Records = output, TotalRecordCount = TotalRecord });
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
        }

    }
}
