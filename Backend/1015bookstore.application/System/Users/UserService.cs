﻿using _1015bookstore.application.Helper;
using _1015bookstore.data.EF;
using _1015bookstore.data.Entities;
using _1015bookstore.utility.Exceptions;
using _1015bookstore.viewmodel.Comon;
using _1015bookstore.viewmodel.System.Users;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace _1015bookstore.application.System.Users
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _config;
        private readonly IEmailSender _emailSender;
        private readonly _1015DbContext _context;

        public UserService(UserManager<User> userManager, SignInManager<User> signInManager, IConfiguration config, IEmailSender emailSender, _1015DbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _config = config;
            _emailSender = emailSender;
            _context = context;
        }
        public async Task<ResponseService<LoginRespone>> Authencate(LoginRequest request)
        {
            var user = await _userManager.FindByNameAsync(request.username);
            if (user == null) 
                return new ResponseService<LoginRespone>()
                {
                    CodeStatus = 400,
                    Status = false,
                    Message = "Wrong Username or Password!",
                };

            var result = await _signInManager.PasswordSignInAsync(user, request.password, true, true);
            if (!result.Succeeded) 
                return new ResponseService<LoginRespone>()
                {
                    CodeStatus = 400,
                    Status = false,
                    Message = "Wrong Username or Password!",
                };

            var roles = await _userManager.GetRolesAsync(user);
            var claims = new[]
            {
                new Claim(ClaimTypes.Email,user.Email),
                new Claim(ClaimTypes.GivenName,user.firstname),
                new Claim(ClaimTypes.Role, string.Join(";",roles))
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Tokens:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(null, null, claims, expires: DateTime.Now.AddHours(3),signingCredentials: creds);

           var loginRespone = new LoginRespone
           {
               token = new JwtSecurityTokenHandler().WriteToken(token),
               user_id = user.Id,
           };

            return new ResponseService<LoginRespone>()
            {
                CodeStatus = 200,
                Status = true,
                Message = "Success",
                Data = loginRespone
            };
        }

        public async Task<ResponseService<string>> ForgotPassword(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return new ResponseService<string>()
                {
                    CodeStatus = 400,
                    Status = false,
                    Message = $"Can not find a emai: {email}",
                };


            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            string code = CreateCode();

            var save = new CodeForgotPassword
            {
                user_id = user.Id,
                code = code,
                createdate = DateTime.Now,
                dateexpire = DateTime.Now.AddMinutes(3),
                token = token,
            };
            await _context.CodesForgotPassword.AddAsync(save);
            await _context.SaveChangesAsync();

            await _emailSender.SendEmailForgotPassword(email, code);

            return new ResponseService<string>()
            {
                CodeStatus = 200,
                Status = true,
                Message = "Success",
                Data = token
            };
        }

        private string CreateCode()
        {
            string code = "";

            Random random = new Random();
            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0; i < 6; i++)
            {
                int randomNumber = random.Next(10);
                stringBuilder.Append(randomNumber);
            }

            code = stringBuilder.ToString();

            return code;
        }

        public async Task<ResponseService> Register(RegisterRequest request)
        {
            var user_old = await _userManager.FindByNameAsync(request.username);
            if (user_old != null)
                return new ResponseService()
                {
                    CodeStatus = 400,
                    Status = false,
                    Message = "The Username is used for someone",
                };


            var check_email = await _userManager.FindByEmailAsync(request.email);
            if (check_email != null)
                return new ResponseService()
                {
                    CodeStatus = 400,
                    Status = false,
                    Message = "The Email is used for someone",
                };

            var user = new User()
            {
                Email = request.email,
                firstname = request.firstname,
                lastname = request.lastname,
                UserName = request.username,
            };
            var result = await _userManager.CreateAsync(user, request.password);
            if (result.Succeeded)
            {
                return new ResponseService()
                {
                    CodeStatus = 200,
                    Status = true,
                    Message = "Success",
                };
            }
            return new ResponseService()
            {
                CodeStatus = 500,
                Status = false,
                Message = "Can not create account",
            };
        }

        public async Task<ResponseService> CofirmCodeForgotPassword(ConfirmCodeFPRequest request)
        {
            var code_db = await _context.CodesForgotPassword.Where(x => x.token == request.token).OrderByDescending(x => x.createdate).FirstOrDefaultAsync();

            if (code_db == null)
            {
                return new ResponseService()
                {
                    CodeStatus = 400,
                    Status = false,
                    Message = "Token is wrong",
                };
            }    

            if (code_db.code != request.code)
            {
                return new ResponseService()
                {
                    CodeStatus = 400,
                    Status = false,
                    Message = "Code is wrong",
                };
            }

            code_db.check = true;
            await _context.SaveChangesAsync();

            return new ResponseService()
            {
                CodeStatus = 200,
                Status = true,
                Message = "Success",
            };
        }

        public async Task<ResponseService> ChangePasswordForgotPassword(ChangePasswordFPRequest request)
        {
            var code_db = await _context.CodesForgotPassword.Where(x => x.token == request.token).OrderByDescending(x => x.createdate).FirstOrDefaultAsync();
            if (code_db == null)
            {
                return new ResponseService()
                {
                    CodeStatus = 400,
                    Status = false,
                    Message = "Token is wrong",
                };
            }
            if (!code_db.check)
            {
                return new ResponseService()
                {
                    CodeStatus = 400,
                    Status = false,
                    Message = "Process is wrong",
                };
            }
            var user = await _context.Users.SingleOrDefaultAsync(x => x.Id == code_db.user_id);
            var resetPassResult = await _userManager.ResetPasswordAsync(user, request.token, request.password);
            if (resetPassResult.Succeeded)
            {
                return new ResponseService()
                {
                    CodeStatus = 200,
                    Status = true,
                    Message = "Success",
                };
            }
            return new ResponseService()
            {
                CodeStatus = 500,
                Status = false,
                Message = "Can not change password",
            };
        }
    
        public async Task<ResponseService> ChangePassword(ChangePasswordRequest request)
        {
            var user = await _context.Users.SingleOrDefaultAsync(x => x.Id == request.user_id);
            if (user == null)
            {
                return new ResponseService()
                {
                    CodeStatus = 400,
                    Status = false,
                    Message = $"Can not find a user with id: {request.user_id}",
                };
            }
            var resetPassResult = await _userManager.ChangePasswordAsync(user, request.oldPassword, request.newPassword);
            if (resetPassResult.Succeeded)
            {
                return new ResponseService()
                {
                    CodeStatus = 200,
                    Status = true,
                    Message = "Success",
                };
            }
            return new ResponseService()
            {
                CodeStatus = 400,
                Status = false,
                Message = "The old password is wrong!",
            };
        }

        public async Task<ResponseService<UserViewModel>> GetUserById(Guid id)
        {
            var user = await _context.Users.SingleOrDefaultAsync(x => x.Id == id);
            if (user == null)
            {
                return new ResponseService<UserViewModel>()
                {
                    CodeStatus = 400,
                    Status = false,
                    Message = $"Can not find a user with id: {id}",
                };
            }

            return new ResponseService<UserViewModel>
            {
                CodeStatus = 200,
                Status = true,
                Message = "Success",
                Data = new UserViewModel
                {
                    id = id,
                    firstname = user.firstname,
                    lastname = user.lastname,
                    dob = user.dob,
                    sex = user.sex,
                    phonenumber = user.PhoneNumber,
                    email = user.Email,
                }
            };
        }

        public async Task<ResponseService> UpdateInforUser(UserUpdateRequest request)
        {
            var user = await _context.Users.SingleOrDefaultAsync(x => x.Id == request.id);
            if (user == null)
            {
                return new ResponseService()
                {
                    CodeStatus = 400,
                    Status = false,
                    Message = $"Can not find a user with id: {request.id}",
                };
            }

            user.firstname = request.firstname;
            user.lastname = request.lastname;
            user.dob = request.dob;
            user.sex = request.sex;
            user.PhoneNumber = request.phonenumber;

            if (await _context.SaveChangesAsync() > 0)
            {
                return new ResponseService()
                {
                    CodeStatus = 200,
                    Status = true,
                    Message = "Success",
                };
            }
            return new ResponseService()
            {
                CodeStatus = 500,
                Status = false,
                Message = "Can not change information",
            };
        }
    }
}
