﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DevIO.Api.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Co campo {0} é obrigatório")]
        [EmailAddress(ErrorMessage = "O campo {0} está em formato inválido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        [StringLength(8, ErrorMessage = "O campo {0} precisa ter entre {2} e {1} caracteres.", MinimumLength = 4)]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "As senhas não conferem.")]
        public string ConfirmPassword { get; set; }
    }

    public class LoginUserViewModel
    {
        [Required(ErrorMessage ="O campo {0} é obrigatório")]
        [EmailAddress(ErrorMessage = "O campo {0} está em formato inválido")]
        public string Email { get; set; }

        public string Password { get; set; }
    }

    public class LoginResponseViewModel
    {
        public string AccessToken { get; set; }
        public long ExpiresIn { get; set; }
        public UserTokenViewModel UserToken { get; set; }
    }

    public class UserTokenViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public IEnumerable<ClaimViewModel> Claims { get; set; }
    }


    public class ClaimViewModel
    {
        public string Value { get; set; }
        public string Type { get; set; }
    }
}
