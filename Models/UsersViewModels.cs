using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace BlogNew.Models
{
    public class UserViewModel
    {
        public string UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }

        [Display(Name = "Disabled")]
        public bool IsDisabled { get; set; }

        [Display(Name = "Role")]
        public string Roles { set; get; }
    }

    public class UserCreateViewModel
    {
        [Required(ErrorMessage = "Username is required.")]
        [Display(Name = "Username")]
        [RegularExpression(@"^[a-zA-Z0-9]{1,10}$",
         ErrorMessage = "Username must be up to 10 uppercase or lowercase letters or digits.")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class UserEditViewModel
    {
        public string UserId { get; set; } 

        [Required(ErrorMessage = "Username is required.")]
        [RegularExpression(@"^[a-zA-Z0-9]{1,10}$",
         ErrorMessage = "Username must be up to 10 uppercase or lowercase letters or digits.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress]
        public string Email { get; set; }

        [Display(Name = "Disabled")]
        [Required(ErrorMessage = "Disabled value required.")]
        public bool IsDisabled { get; set; }

        [Required(ErrorMessage = "At least one Role is required.")]
        public string Role {  get; set; }

        public List<SelectListItem> SelectRoles { get; set; } = new List<SelectListItem>();
    }
}