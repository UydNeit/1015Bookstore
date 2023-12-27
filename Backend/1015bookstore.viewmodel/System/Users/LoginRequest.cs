﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _1015bookstore.viewmodel.System.Users
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Please enter your Username and Pasword!")]
        public string username { get; set; }
        [Required(ErrorMessage = "Please enter your Username and Pasword!")]
        public string password { get; set; }
    }
}
