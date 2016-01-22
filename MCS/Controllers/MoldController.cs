using MCS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Web;
using System.Web.Mvc;

namespace MCS.Controllers
{
    public class MoldController : Controller
    {
        private MoldControlEntities db = new MoldControlEntities();

        [Chk_Authen]
        public ActionResult Index()
        {
            ViewBag.SearchMoldType = db.tm_mold_type.Where(w => w.active == true).OrderBy(o => o.mold_type_name);
            return View();
        }

        [HttpPost]
        public JsonResult MoldList(string mold_code, string item_code, string unit, string location, string mold_diameter, byte? mold_type, int jtStartIndex = 0, int jtPageSize = 0, string jtSorting = null)
        {
            try
            {
                //byte lv = byte.Parse(Session["MCS_ULv"].ToString());
                //int org = int.Parse(Session["MCS_Org"].ToString());
                var query = from a in db.td_mold select a;

                if (!string.IsNullOrEmpty(mold_code))
                {
                    query = query.Where(w => w.mold_code.ToUpper().Contains(mold_code.ToUpper()));
                }
                if (!string.IsNullOrEmpty(item_code))
                {
                    query = query.Where(w => w.item_code.ToUpper().Contains(item_code.ToUpper()));
                }
                if (!string.IsNullOrEmpty(unit))
                {
                    query = query.Where(w => w.unit == unit);
                }
                if (!string.IsNullOrEmpty(location))
                {
                    query = query.Where(w => w.location == location);
                }
                if (!string.IsNullOrEmpty(mold_diameter))
                {
                    query = query.Where(w => w.mold_diameter.ToUpper().Contains(mold_diameter.ToUpper()));
                }
                if (mold_type != null)
                {
                    query = query.Where(w => w.mold_type == mold_type);
                }

                //Get data from database
                int TotalRecord = query.Count();

                // Paging
                var output = query
                    .Select(s => new
                    {
                        s.mold_code,
                        s.item_code,
                        s.mold_type,
                        s.unit,
                        s.location,
                        s.mold_diameter
                    }).OrderBy(jtSorting).Skip(jtStartIndex).Take(jtPageSize);

                //Return result to jTable
                return Json(new { Result = "OK", Records = output, TotalRecordCount = TotalRecord });
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
        }
        
        [HttpPost]
        public JsonResult UpdateMold()
        {
            try
            {
                var mold_code = Request.Form["mold_code"];
                var data = db.td_mold.Find(mold_code);
                if (data != null)
                {
                    data.item_code = Request.Form["item_code"];
                    data.mold_type = byte.Parse(Request.Form["mold_type"].ToString());
                    data.unit = Request.Form["unit"];
                    data.location = Request.Form["location"];
                    data.mold_diameter = Request.Form["mold_diameter"];
                    data.update_dt = DateTime.Now;
                    data.update_by = Session["MCS_Authen"].ToString();

                    db.SaveChanges();
                }

                return Json(new { Result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult CreateMold()
        {
            try
            {
                var mold_code = Request.Form["mold_code"];
                var data = db.td_mold.Find(mold_code);
                if (data == null)
                {
                    td_mold mold = new td_mold();
                    mold.mold_code = mold_code;
                    mold.item_code = Request.Form["item_code"] != null ? Request.Form["item_code"].ToString() : null;
                    mold.mold_type = byte.Parse(Request.Form["mold_type"].ToString());
                    mold.unit = Request.Form["unit"];
                    mold.location = Request.Form["location"];
                    mold.mold_diameter = Request.Form["mold_diameter"] != null ? Request.Form["mold_diameter"].ToString() : null;
                    mold.create_dt = DateTime.Now;
                    mold.create_by = Session["MCS_Authen"].ToString();
                    
                    db.td_mold.Add(mold);

                    db.SaveChanges();
                }

                return Json(new { Result = "OK", Record = db.td_mold.OrderByDescending(o => o.create_dt).FirstOrDefault() });
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
        }

    }
}
