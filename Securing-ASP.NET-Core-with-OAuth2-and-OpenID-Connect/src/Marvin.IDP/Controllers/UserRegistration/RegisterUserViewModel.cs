using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Marvin.IDP.Controllers.UserRegistration
{
    public class RegisterUserViewModel
    {
        // credentials
        [MaxLength(100)]
        public string Username { get; set; }

        [MaxLength(100)]
        public string Password { get; set; }

        // claims
        [Required]
        [MaxLength(100)]
        public string Firstname { get; set; }

        [Required]
        [MaxLength(100)]
        public string Lastname { get; set; }

        [Required]
        [MaxLength(150)]
        public string Email { get; set; }

        [Required]
        [MaxLength(200)]
        public string Address { get; set; }

        [Required]
        [MaxLength(2)]
        public string Country { get; set; }

        public SelectList CountryCodes { get; set; } = new SelectList(new []
        {
            new {Id="BE", Value="Belgium"},
            new {Id="US", Value="United States of America"},
            new {Id="IN", Value="India"}}, "Id", "Value"
        );

        public string ReturnUrl { get; set; }
    }
}
