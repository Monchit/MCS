using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Web;
using System.Web.Mvc;
using WebCommonFunction;
using MCS.Models;
using System.Transactions;
using System.IO;

namespace MCS.Controllers
{
    public class HomeController : Controller
    {
        private MoldControlEntities db = new MoldControlEntities();
        private TNCSecurity secure = new TNCSecurity();
        private TNCConversion convert = new TNCConversion();

        public ActionResult Index()
        {
            return View();
        }

        [Chk_Authen]
        public ActionResult Main()
        {
            ViewBag.SearchMoldType = db.tm_mold_type.Where(w => w.active == true).OrderBy(o => o.mold_type_name);
            return View();
        }

        [Chk_Authen]
        public ActionResult AddMold()
        {
            ViewBag.MoldType = from a in db.tm_mold_type
                                     where a.active == true
                                     orderby a.mold_type_name ascending
                                     select a;
            //ViewBag.SelectMoldType = db.tm_mold_type.Where(w => w.active == true).OrderBy(o => o.mold_type_name);
            return View();
        }

        [Chk_Authen]
        public ActionResult AddLedger(string mold_code)
        {
            var mold = (from a in db.td_mold
                        where a.mold_code == mold_code
                        select a).FirstOrDefault();

            ViewBag.TM_Inspection = db.tm_inspection.Where(w => w.active == true);
            ViewBag.TD_Issue = db.td_issue.Where(w => w.mold_code == mold.mold_code);

            return View(mold);
        }

        [Chk_Authen]
        public ActionResult NewMold()
        {
            return View();
        }

        [HttpGet]
        public ActionResult _ShowInspection(string mold_code, string cavity)
        {
            var query = from a in db.td_inspection
                        where a.mold_code == mold_code && a.cavity_no == cavity
                        select a;
            return PartialView(query);
        }

        public ActionResult _FormLedger(string mold_code)
        {
            ViewBag.mold_code = mold_code;
            return PartialView();
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

        [Chk_Authen]
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

        public ActionResult Login()
        {
            string username = Request.Form["Username"].ToString();
            string pass = Request.Form["Password"].ToString();
            var chklogin = secure.Login(username, pass, false);//set false to true for Real

            if (chklogin != null)
            {
                //if ((chklogin.group_id == 135 || chklogin.group_id == 1) || (chklogin.group_id == 3)) // system or Store
                //{
                    Session["MCS_Authen"] = chklogin.emp_code; // Employee ID
                    Session["MCS_Name"] = chklogin.emp_fname + " " + chklogin.emp_lname;    // Name & Lastname
                    Session["MCS_Pos"] = chklogin.emp_position;

                    return RedirectToAction("Main", "Home");
                //}
                //else
                //{
                //    TempData["noty"] = "You are not authorized to access this system !!!";
                //    return RedirectToAction("Index", "Home");
                //}
            }
            else
            {
                TempData["noty"] = "Username and/or Password is incorrect !!!";
                return RedirectToAction("Index", "Home");
            }
        }

        public ActionResult Logout()
        {
            Session.Remove("MCS_Authen");
            Session.Remove("MCS_Name");
            Session.Remove("MCS_Pos");
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public ActionResult CreateMold()
        {
            try
            {
                string moldcode = Request.Form["moldcode"];

                td_mold mold = new td_mold();
                mold.mold_code = moldcode;
                mold.mold_type = byte.Parse(Request.Form["moldtype"].ToString());
                mold.item_code = Request.Form["itemcode"] != null ? Request.Form["itemcode"].ToString() : null;
                mold.location = Request.Form["location"];
                mold.unit = Request.Form["unit"];
                mold.mold_diameter = Request.Form["molddiameter"] != null ? Request.Form["molddiameter"].ToString() : null;
                mold.create_dt = DateTime.Now;
                mold.create_by = Session["MCS_Authen"].ToString();

                db.td_mold.Add(mold);
                db.SaveChanges();

                return RedirectToAction("AddLedger", "Home", new { mold_code = moldcode });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex;
                return View("AddMold");
            }
        }

        [HttpPost]
        public ActionResult CreateLedger(HttpPostedFileBase piccode, HttpPostedFileBase picmold)
        {
            string moldcode = Request.Form["moldcode"];
            using (TransactionScope tran = new TransactionScope())
            {
                using (var dbin = new MoldControlEntities())
                {
                    dbin.Database.Connection.Open();

                    try
                    {
                        DateTime dt = DateTime.Now;
                        var cav = Request.Form["cavityno"];

                        if (!dbin.td_ledger.Any(w => w.mold_code == moldcode && w.cavity_no == cav))
                        {
                            td_ledger ledger = new td_ledger();
                            ledger.mold_code = moldcode;
                            ledger.cavity_no = cav;
                            ledger.qty = int.Parse(Request.Form["qty"].ToString());
                            ledger.receive_date = convert.DateDisplayToDB(Request.Form["adddate"]);
                            ledger.invoice_no = Request.Form["invoiceno"];
                            ledger.description = Request.Form["description"];
                            ledger.source = Request.Form["source"];
                            ledger.result = Request.Form["result"];
                            ledger.price = !string.IsNullOrEmpty(Request.Form["price"].ToString()) ? int.Parse(Request.Form["price"].ToString()) : 0;
                            ledger.check_by = Session["MCS_Authen"].ToString();
                            ledger.create_dt = dt;
                            ledger.status_id = 1;

                            // Add data to DB TD_File //
                            string subPath = "~/UploadFiles/" + moldcode + "/" + cav + "/";
                            if (!Directory.Exists(Server.MapPath(subPath)))
                                Directory.CreateDirectory(Server.MapPath(subPath));
                            
                            if (piccode != null && piccode.ContentLength > 0)
                            {
                                if (piccode.ContentType.Contains("image"))
                                {
                                    var extension = Path.GetExtension(piccode.FileName);
                                    var fileName = "moldcode" + extension;
                                    var path = Path.Combine(Server.MapPath(subPath), fileName);
                                    piccode.SaveAs(path);
                                    ledger.pic_code = subPath + fileName;
                                }
                            }

                            if (picmold != null && picmold.ContentLength > 0)
                            {
                                if (picmold.ContentType.Contains("image"))
                                {
                                    var extension = Path.GetExtension(picmold.FileName);
                                    var fileName = "mold" + extension;
                                    var path = Path.Combine(Server.MapPath(subPath), fileName);
                                    picmold.SaveAs(path);
                                    ledger.pic_mold = subPath + fileName;
                                }
                            }

                            dbin.td_ledger.Add(ledger);

                            //string[] key = Request.Form.GetValues("insKey");
                            //string[] result = Request.Form.GetValues("insResult");
                            //string[] desc = Request.Form.GetValues("insDesc");

                            //for (int i = 0; i < key.Length; i++)
                            //{
                            //    td_inspection insp = new td_inspection();
                            //    insp.mold_code = moldcode;
                            //    insp.cavity_no = cav;
                            //    insp.inspection_id = byte.Parse(key[i]);
                            //    insp.result = result[i] == "OK" ? true : false;
                            //    insp.description = desc[i];
                            //    dbin.td_inspection.Add(insp);
                            //}

                            dbin.SaveChanges();
                            tran.Complete();
                        }

                    }
                    catch (Exception)
                    {
                        tran.Dispose();
                        TempData["ErrorMessage"] = "";
                    }
                    finally
                    {
                        dbin.Database.Connection.Close();
                        dbin.Dispose();
                        tran.Dispose();
                    }
                }
            }

            return RedirectToAction("AddLedger", new { mold_code = moldcode });
        }

        [HttpPost]
        public ActionResult CreateInspection()
        {
            string moldcode = Request.Form["hdmoldcode"];
            using (var dbin = new MoldControlEntities())
            {
                dbin.Database.Connection.Open();
                try
                {
                    var cav = Request.Form["hdcavityno"];
                    string[] key = Request.Form.GetValues("inspKey");
                    string[] result = Request.Form.GetValues("inspResult");
                    string[] desc = Request.Form.GetValues("inspDesc");
                    
                    for (int i = 0; i < key.Length; i++)
                    {
                        td_inspection insp = new td_inspection();
                        insp.mold_code = moldcode;
                        insp.cavity_no = cav;
                        insp.inspection_id = byte.Parse(key[i]);
                        insp.result = result[i] == "OK" ? true : false;
                        insp.description = desc[i];
                        dbin.td_inspection.Add(insp);
                    }
                    dbin.SaveChanges();
                }
                catch (Exception)
                {
                    TempData["ErrorMessage"] = "";
                }
                finally
                {
                    dbin.Database.Connection.Close();
                    dbin.Dispose();
                }
            }
            return RedirectToAction("AddLedger", new { mold_code = moldcode });
        }

        [HttpPost]
        public JsonResult GetMoldType()
        {
            try
            {
                var result = db.tm_mold_type
                    .Select(r => new { DisplayText = r.mold_type_name, Value = r.mold_type });
                return Json(new { Result = "OK", Options = result });
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
        }

        //---------------------------------//

        [Chk_Authen]
        public ActionResult MoldType()
        {
            return View();
        }

        [HttpPost]
        public JsonResult MoldTypeList(int jtStartIndex = 0, int jtPageSize = 0, string jtSorting = null)
        {
            try
            {
                var query = db.tm_mold_type;

                //Get data from database
                int TotalRecord = query.Count();

                // Paging
                var output = query
                    .Select(s => new
                    {
                        s.mold_type,
                        s.mold_type_name,
                        s.active
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
        public JsonResult CreateMoldType()
        {
            try
            {
                var formData = Request.Form["mold_type_name"];
                var dbData = db.tm_mold_type.Where(w => w.mold_type_name == formData).FirstOrDefault();
                if (dbData == null)
                {
                    tm_mold_type data = new tm_mold_type();
                    data.mold_type_name = formData;
                    data.active = true;

                    db.tm_mold_type.Add(data);
                }

                db.SaveChanges();

                return Json(new { Result = "OK", Record = db.tm_mold_type.OrderByDescending(o => o.mold_type).FirstOrDefault() });
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult UpdateMoldType()
        {
            try
            {
                var formData = byte.Parse(Request.Form["mold_type"].ToString());
                var data = db.tm_mold_type.Find(formData);
                data.mold_type_name = Request.Form["mold_type_name"];
                data.active = Request.Form["active"] == "1" ? true : false;
                db.SaveChanges();

                return Json(new { Result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult DeleteMoldType()
        {
            try
            {
                var formData = byte.Parse(Request.Form["mold_type"].ToString());
                var data = db.tm_mold_type.Find(formData);
                db.tm_mold_type.Remove(data);
                db.SaveChanges();

                return Json(new { Result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
        }

        //---------------------------------//

        [Chk_Authen]
        public ActionResult Inspection()
        {
            return View();
        }

        [HttpPost]
        public JsonResult InspList(int jtStartIndex = 0, int jtPageSize = 0, string jtSorting = null)
        {
            try
            {
                var query = db.tm_inspection;

                //Get data from database
                int TotalRecord = query.Count();

                // Paging
                var output = query
                    .Select(s => new
                    {
                        s.inspection_id,
                        s.inspection_name,
                        s.active
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
        public JsonResult CreateInsp()
        {
            try
            {
                var formData = Request.Form["inspection_name"];
                var dbData = db.tm_inspection.Where(w => w.inspection_name == formData).FirstOrDefault();
                if (dbData == null)
                {
                    tm_inspection data = new tm_inspection();
                    data.inspection_name = Request.Form["inspection_name"];
                    data.active = true;

                    db.tm_inspection.Add(data);
                }

                db.SaveChanges();

                return Json(new { Result = "OK", Record = db.tm_mold_type.OrderByDescending(o => o.mold_type).FirstOrDefault() });
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult UpdateInsp()
        {
            try
            {
                var formData = byte.Parse(Request.Form["inspection_id"]);
                var data = db.tm_inspection.Find(formData);
                data.inspection_name = Request.Form["inspection_name"];
                data.active = Request.Form["active"] == "1" ? true : false;
                db.SaveChanges();

                return Json(new { Result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult DeleteInsp()
        {
            try
            {
                var formData = byte.Parse(Request.Form["inspection_id"]);
                var data = db.tm_inspection.Find(formData);
                db.tm_inspection.Remove(data);
                db.SaveChanges();

                return Json(new { Result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
        }

        //---------------------------------//

        public ActionResult Remedy()
        {
            return View();
        }
    }
}
