﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SimpleCrud.Data;
using SimpleCrud.Models;
using SimpleCrud.Models.Domain;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace SimpleCrud.Controllers;

public class AuthController : Controller
{
    private static readonly Regex _regex = new("^[a-zA-Z0-9]*$");
    private readonly AppDbContext _context;
    private readonly IOptions<IdentityOptions> _identityOptions;

    public AuthController(AppDbContext context, IOptions<IdentityOptions> identityOptions)
    {
        _context = context;
        _identityOptions = identityOptions;
    }

    [HttpGet]
    public IActionResult Login()
    {
        ClaimsPrincipal userClaims = HttpContext.User;

        if (userClaims is not null && userClaims.Identity is not null && userClaims.Identity.IsAuthenticated)
        {
            return RedirectToAction("Index", "Home");
        }

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (model is null || model.Username is null)
        {
            return View();
        }

        User? user = await _context.Users.FindAsync(model.Username);

        if (user is null || !BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
        {
            ViewData["ValidationError"] = "Bad login information.";
            return View();
        }

        List<Claim> claims = new()
        {
            new Claim(ClaimTypes.NameIdentifier, model.Username)
        };

        ClaimsIdentity identity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        AuthenticationProperties properties = new()
        {
            AllowRefresh = true,
            IsPersistent = model.KeepLoggedIn
        };

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity), properties);

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Register()
    {
        ClaimsPrincipal userClaims = HttpContext.User;

        if (userClaims is not null && userClaims.Identity is not null && userClaims.Identity.IsAuthenticated)
        {
            return RedirectToAction("Index", "Home");
        }

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (model is null || string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Password) || string.IsNullOrEmpty(model.ConfirmedPassword))
        {
            ViewData["ValidationError"] = "Missing one or more input.";
            return View();
        }

        if (model.Username.Length < 5 || !_regex.IsMatch(model.Username))
        {
            ViewData["ValidationError"] = "Username has to be at least 5 characters long and contain only letter or digits.";
            return View();
        }

        if (!model.Password.Equals(model.ConfirmedPassword))
        {
            ViewData["ValidationError"] = "Passwords do not match.";
            return View();
        }

        bool isUsernameTaken = await _context.Users.AnyAsync(user => user.Username == model.Username);
        if (isUsernameTaken)
        {
            ViewData["ValidationError"] = "Username is taken.";
            return View();
        }

        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);
        User user = new()
        {
            Username = model.Username,
            Password = hashedPassword
        };

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        return RedirectToAction("Login");
    }
}
