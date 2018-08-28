﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BackEnd.DataBase;
using BackEnd.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models.PublicAPI.Requests;
using Models;
using Models.PublicAPI.Requests.Account;
using Microsoft.Azure.KeyVault.Models;
using Models.PublicAPI.Responses;
using Models.People;
using BackEnd.Extensions;
using Microsoft.AspNetCore.Authorization;
using Models.PublicAPI.Responses.People;

namespace BackEnd.Controllers.Users
{
    [Produces("application/json")]
    [Route("api/account")]
    public class AccountController : AuthorizeController
    {
        private readonly IMapper mapper;
        private readonly IEmailSender emailSender;
        private readonly IUserRegisterTokens registerTokens;

        public AccountController(IMapper mapper, 
            UserManager<User> userManager, 
            IEmailSender emailSender,
            IUserRegisterTokens registerTokens) : base(userManager)
        {
            this.mapper = mapper;
            this.emailSender = emailSender;
            this.registerTokens = registerTokens;
        }
        [AllowAnonymous]
        [HttpGet]
        [Route("{id}/{*emailToken}")]
        public async Task<ResponseBase> Get(string id, string emailToken)
        {
            var user = await userManager.FindByIdAsync(id);
            var result = await userManager.ConfirmEmailAsync(user, emailToken);
            if (result.Succeeded)
            {
                return ResponseStatusCode.OK;
            }
            else
            {
                return ResponseStatusCode.InvalidToken;
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ResponseBase> Post([FromBody]AccountCreateRequest account)
        {
            if (!await registerTokens.IsCorrectToken(account.Email, account.AccessToken))
                return ResponseStatusCode.IncorrectAccessToken;
            User user;
            user = mapper.Map<User>(account);
            var result = await userManager.CreateAsync(user, account.Password);
            if (result.Succeeded)
                await registerTokens.RemoveToken(account.Email);

            return ResponseStatusCode.OK;
        }

        [HttpPut]
        public async Task<UserView> EditUser([FromBody]AccountEditRequest editRequest)
        {
            var currentUser = await GetCurrentUser();
            mapper.Map(editRequest, currentUser);
            await userManager.UpdateAsync(currentUser);
            return mapper.Map<UserView>(currentUser);
        }
    }
}