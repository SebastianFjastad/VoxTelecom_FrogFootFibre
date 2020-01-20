using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using FrogFoot.Entities;
using FrogFoot.Models;

namespace FrogFoot.Areas.Home.Models
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class ExternalLoginListViewModel
    {
        public string ReturnUrl { get; set; }
    }

    public class SendCodeViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
        public string ReturnUrl { get; set; }
        public bool RememberMe { get; set; }
    }

    public class VerifyCodeViewModel
    {
        [Required]
        public string Provider { get; set; }

        [Required]
        [Display(Name = "Code")]
        public string Code { get; set; }
        public string ReturnUrl { get; set; }

        [Display(Name = "Remember this browser?")]
        public bool RememberBrowser { get; set; }

        public bool RememberMe { get; set; }
    }

    public class ForgotViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Email")]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        private List<ISP> _isPs;

        public RegisterViewModel()
        {
            Locations = new List<Location>();
            Estates = new List<Estate>();
            ISPs = new List<ISP>();
            SelectedISPIds = new List<int?>();
        }

        public List<Location> Locations { get; set; }
        public List<Estate> Estates { get; set; }
        public List<ISP> ISPs
        {
            get { return _isPs.OrderBy(i => i.Name).ToList(); }
            set { _isPs = value; }
        }
        public List<int?> SelectedISPIds { get; set; }

        #region User
        [Required]
        [Display(Name = "First name")]
        public string FirstName { get; set; }
        [Required]
        [Display(Name = "Last name")]
        public string LastName { get; set; }
        [Required]
        [Display(Name = "Email")]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
        [Required]
        [DataType(DataType.PhoneNumber)]
        [Display(Name = "Cell number")]
        public string PhoneNumber { get; set; }
        [DataType(DataType.PhoneNumber)]
        public string Landline { get; set; }
        [Required]
        [Display(Name = "Street address")]
        public string Address { get; set; }
        [Required]
        [Display(Name = "Latitude")]
        public double Latitude { get; set; }
        [Required]
        [Display(Name = "Longitude")]
        public double Longitude { get; set; }
        public string SuburbName { get; set; }
        public string EstateName { get; set; }
        [Required]
        [Display(Name = "Suburb")]
        public int? LocationId { get; set; }
        public int? EstateId { get; set; }
        #endregion

        #region Alt Contact
        public bool HasAltContact { get; set; }
        [Display(Name = "First name")]
        public string AltFirstName { get; set; }
        [Display(Name = "Last name")]
        public string AltLastName { get; set; }
        [Display(Name = "Email")]
        [EmailAddress]
        public string AltEmail { get; set; }
        [Display(Name = "Cell no.")]
        [DataType(DataType.PhoneNumber)]
        public string AltCellNo { get; set; }
        [Display(Name = "Landline")]
        [DataType(DataType.PhoneNumber)]
        public string AltLandline { get; set; }
        #endregion
    }

    public class AdminRegisterViewModel
    {
        #region User
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
        [DisplayName("First name")]
        public string FirstName { get; set; }
        [DisplayName("Last name")]
        public string LastName { get; set; }
        [DisplayName("Cell no.")]
        public string PhoneNumber { get; set; }
        [DisplayName("Landline")]
        public string Landline { get; set; }

        public int? LocationId { get; set; }
        public int? EstateId { get; set; }
        public string Address { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public UserType Type { get; set; }
        public int? ISPId { get; set; }
        [DisplayName("Is Champ")]
        public bool IsChamp { get; set; }
        public bool IsUserTypeAdmin { get; set; }
        [DisplayName("Manages Precinct instead of Estate")]
        public bool UsePrecinctCodeForChamp { get; set; }
        #endregion

        #region Alt Contact
        public bool HasAltContact { get; set; }
        [Display(Name = "First name")]
        public string AltFirstName { get; set; }
        [Display(Name = "Last name")]
        public string AltLastName { get; set; }
        [Display(Name = "Email")]
        public string AltEmail { get; set; }
        [Display(Name = "Cell no.")]
        public string AltCellNo { get; set; }
        [Display(Name = "Landline")]
        public string AltLandline { get; set; }
        #endregion
    }

    public class ResetPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        //[StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}
