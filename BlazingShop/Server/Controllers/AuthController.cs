﻿using BlazingShop.Server.Models;
using BlazingShop.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazingShop.Server.Controllers
{
   // For accesing the endpoints like..api/auth/login or …api/auth/register
   [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        // 19-26 Constructor Injection of the Identity Interfaces for signing-in and creating new users etc.
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        
        public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }


        [HttpPost]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var user = await _userManager.FindByNameAsync(request.UserName);
            if (user == null) return BadRequest("User does not exist");
            var singInResult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            if (!singInResult.Succeeded) return BadRequest("Invalid password");
            await _signInManager.SignInAsync(user, request.RememberMe);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterRequest parameters)
        {
            var user = new ApplicationUser();
            user.UserName = parameters.UserName;
            user.FullName = parameters.FullName;
            user.Email = parameters.Email;
            user.Address = parameters.Address;
            user.PhoneNumber = parameters.PhoneNumber;
            var result = await _userManager.CreateAsync(user, parameters.Password);
            if (!result.Succeeded) return BadRequest(result.Errors.FirstOrDefault()?.Description);
            if (user.UserName.StartsWith("admin"))
            {
                await _userManager.AddToRoleAsync(user, "Admin");
                
                return await Login(new LoginRequest
                {
                    UserName = parameters.UserName,
                    Password = parameters.Password

                });
            }

            await _userManager.AddToRoleAsync(user, "User");

            return await Login(new LoginRequest
            {
                UserName = parameters.UserName,
                Password = parameters.Password

            });
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok();
        }

        [HttpGet]
        public CurrentUser CurrentUserInfo()
        {

            return new CurrentUser
            {
                IsAuthenticated = User.Identity.IsAuthenticated,
                UserName = User.Identity.Name,
              
                Claims = User.Claims
                .ToDictionary(c => c.Type, c => c.Value)
                
            };
        }
    }
}
