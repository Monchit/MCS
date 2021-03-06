//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MCS.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class td_ledger
    {
        public td_ledger()
        {
            this.td_inspection = new HashSet<td_inspection>();
        }
    
        public string mold_code { get; set; }
        public string cavity_no { get; set; }
        public System.DateTime receive_date { get; set; }
        public int qty { get; set; }
        public string description { get; set; }
        public string source { get; set; }
        public string result { get; set; }
        public string invoice_no { get; set; }
        public Nullable<int> price { get; set; }
        public string pic_code { get; set; }
        public string pic_mold { get; set; }
        public string check_by { get; set; }
        public System.DateTime create_dt { get; set; }
        public byte status_id { get; set; }
    
        public virtual ICollection<td_inspection> td_inspection { get; set; }
        public virtual td_mold td_mold { get; set; }
    }
}
