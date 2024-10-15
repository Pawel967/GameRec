using Game_MVC.Dtos;
using Game_MVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Game_MVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AuthService _authService;

        public AdminController(AuthService authService)
        {
            _authService = authService;
        }

        public async Task<IActionResult> UserList()
        {
            var users = await _authService.GetAllUsersAsync();
            return View(users);
        }

        public async Task<IActionResult> EditUser(Guid id)
        {
            var user = await _authService.GetUserInfoAsync(id);
            var updateUserDto = new UpdateUserDto
            {
                Username = user.Username,
                Email = user.Email
            };
            return View(updateUserDto);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(Guid id, UpdateUserDto updateUserDto)
        {
            if (!ModelState.IsValid)
            {
                return View(updateUserDto);
            }

            try
            {
                var updatedUser = await _authService.UpdateUserInfoAsync(id, updateUserDto);
                TempData["SuccessMessage"] = "User account has been updated successfully.";
                return RedirectToAction(nameof(UserList));
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while updating the user account. Please try again.");
                return View(updateUserDto);
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            try
            {
                var result = await _authService.DeleteUserAsync(id);
                if (result)
                {
                    TempData["SuccessMessage"] = "User account has been deleted successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "User not found. The account may have already been deleted.";
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error deleting user: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while deleting the user account. Please try again.";
            }
            return RedirectToAction(nameof(UserList));
        }
    }
}
