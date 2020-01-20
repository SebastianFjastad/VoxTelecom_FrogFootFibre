using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using FrogFoot.Entities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace FrogFoot.Areas.Home.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class User : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<User> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }

        public User()
        {
            ClientISPContacts = new HashSet<ClientISPContact>();
            ClientContactMethods = new HashSet<ClientContactMethod>();
        }

        #region User
        public bool IsDeleted { get; set; }
        public int? ISPId { get; set; }
        public ISP ISP { get; set; }
        [Display(Name = "Is user type admin")]
        public bool IsUserTypeAdmin { get; set; }
        [DisplayName("First name")]
        [Required]
        public string FirstName { get; set; }
        [Required]
        [DisplayName("Last name")]
        public string LastName { get; set; }
        public string Landline { get; set; }
        public DateTime? CreatedDate { get; set; }
        [DisplayName("Is Champ")]
        public bool IsChamp { get; set; }
        [DisplayName("Opt out of ISP communications")]
        public bool ISPCommsOptOutStatus { get; set; }
        [DisplayName("Opt out of Frogfoot communications")]
        public bool FFCommsOptOutStatus { get; set; }
        [DisplayName("Manages Precinct instead of Estate")]
        public bool UsePrecinctCodeForChamp { get; set; }
        [DisplayName("Log in count")]
        public int LogInCount { get; set; }
        public DateTime? LastLogin { get; set; }
        public ICollection<Order> Orders { get; set; }
        public virtual ICollection<ClientISPContact> ClientISPContacts { get; set; }
        public ICollection<ClientContactMethod> ClientContactMethods { get; set; }
        public virtual ICollection<ISPClientContact> ISPClientContacts { get; set; }
        #endregion

        #region Location Details
        [DisplayName("Address")]
        public string Address { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public bool UserCreatedWithOrder { get; set; }
        public int? LocationId { get; set; }
        public Location Location { get; set; }
        public int? EstateId { get; set; }
        public Estate Estate { get; set; }
        public int? ZoneId { get; set; }
        public Zone Zone { get; set; }
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

        #region HouseOwner
        [Display(Name = "House owner")]
        public bool IsOwner { get; set; }
        [Display(Name = "Name")]
        public string OwnerName { get; set; }
        [Display(Name = "Email")]
        public string OwnerEmail { get; set; }
        [Display(Name = "Phone no.")]
        public string OwnerPhoneNumber { get; set; }
        #endregion

        public bool Temp { get; set; }
        
        public string SuburbString { get; set; }
        public string TempPassword { get; set; }
        public string EstateString { get; set; } 
    }
}