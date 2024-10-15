using Game_MVC.Dtos;
using Game_MVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Game_MVC.Controllers
{
    [Authorize]
    public class UserAccountController : Controller
    {
        private readonly AuthService _authService;

        public UserAccountController(AuthService authService)
        {
            _authService = authService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var user = await _authService.GetUserInfoAsync(userId);
                return View(user);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return RedirectToAction("Login", "Auth");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var user = await _authService.GetUserInfoAsync(userId);
                var updateUserDto = new UpdateUserDto
                {
                    Username = user.Username,
                    Email = user.Email
                };
                return View(updateUserDto);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return RedirectToAction("Login", "Auth");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UpdateUserDto updateUserDto)
        {
            if (!ModelState.IsValid)
            {
                return View(updateUserDto);
            }

            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var updatedUser = await _authService.UpdateUserInfoAsync(userId, updateUserDto);
                TempData["SuccessMessage"] = "Your account has been updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return RedirectToAction("Login", "Auth");
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while updating your account. Please try again.");
                return View(updateUserDto);
            }
        }
    }
}
