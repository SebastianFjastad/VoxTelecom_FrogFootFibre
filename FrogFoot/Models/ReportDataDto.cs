using System;
using System.Collections.Generic;
using FrogFoot.Entities;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;

namespace FrogFoot.Models
{
    public class ReportDataDto
    {
        public string CreatedDate { get; set; }
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Landline { get; set; }
        public string IsOwner { get; set; }
        public int OrderId { get; set; }
        public string ProductName { get; set; }
        public LineSpeed LineSpeed { get; set; }
        public decimal? FFSetupCost { get; set; }
        public decimal? FFMonthlyRevenue { get; set; }
        public string UpSpeed { get; set; }
        public string IsCapped { get; set; }
        public string CreatedByRole { get; set; }
        public decimal? MonthlyCost { get; set; }
        public decimal? SetupCost { get; set; }
        public string ISPName { get; set; }
        public string Precinct { get; set; }
        public string Suburb { get; set; }
        public string Estate { get; set; }
        public string Zone { get; set; }
        public string Address { get; set; }
        public string FirstDateOfFibre { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string UserCreatedBy { get; set; }
        public string OrderedOn { get; set; }
        public int LogInCount { get; set; }
        public string LastLogInDate { get; set; }
        public OrderStatus? Status { get; set; }
        public List<Status> StatusList { get; set; }
    }
}