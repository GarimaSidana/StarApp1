using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StarApp1.Models
{
    public class AllowanceDashboardViewModel
    {
        public string Name  { get; set; }
        [Key]
        public int SAPid { get; set; }
        public int Hours { get; set; }
        public int LeaveHours { get; set; }
        public int AfternoonShiftDays { get; set; }
        public int NightShiftDays { get; set; }
        public int TotalDays { get; set; }
        public int TransportAllowance { get; set; }
        public int TotalAllowance   { get; set; }
        public string ApprovalStatus { get; set; }

        
        public string ProjectId { get; set; }
        [NotMapped]
        public List<SelectListItem> Project { get; set; }
        
        public DateTime PeriodStart { get; set; }

        [NotMapped]
        public List<SelectListItem> Period{ get; set; }

        public IFormFile browseFile { get; set; }
         
        
    }
}
