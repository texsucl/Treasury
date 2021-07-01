using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SSO.Web.ViewModels
{
    public class LoginModel
    {
        [Display(Name = "網域帳號")]
        [Required(ErrorMessage = "請輸入網域帳號")]
        public string UserId { get; set; }

        [Display(Name = "密碼")]
        [Required(ErrorMessage = "請輸入密碼")]
        public string Password { get; set; }
    }
}