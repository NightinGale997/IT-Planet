﻿using PlanetIT.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace PlanetIT.Domain.Models.Request
{
    public class AccountRequest
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required]
        public Role Role { get; set; }
    }
}
