using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using SimpleCrud.Data;
using SimpleCrud.Models;
using SimpleCrud.Models.Domain;
using System.Security.Claims;

namespace SimpleCrud.Controllers;

public class AuthController : Controller
{
    private readonly AppDbContext _context;

    public AuthController(AppDbContext context)
    {
        _context = context;
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
        User? user = await _context.Users.FindAsync(model.Username);

        if (user is null || !user.Password.Equals(model.Password))
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
}
