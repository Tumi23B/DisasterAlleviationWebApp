using Disaster_Alleviation_Web_App.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Disaster_Alleviation_Web_App.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserManagementController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserManagementController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: /UserManagement/
        public async Task<IActionResult> Index()
        {
            var users = _userManager.Users.ToList();
            var userRoles = new List<UserWithRoleViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoles.Add(new UserWithRoleViewModel
                {
                    User = user,
                    Roles = roles
                });
            }

            return View(userRoles); // Make sure your Index.cshtml uses: @model IEnumerable<UserWithRoleViewModel>
        }

        // POST: /UserManagement/ChangeRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeRole(string userId, string role)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(role))
                return BadRequest("User ID and role must be provided.");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound("User not found.");

            // Prevent admin from removing own role
            if (user.UserName == User.Identity.Name && role != "Admin")
            {
                TempData["ErrorMessage"] = "You cannot remove your own admin role.";
                return RedirectToAction("Index");
            }

            // Remove old roles
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Count > 0)
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
            }

            // Create role if it doesn't exist
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }

            // Add new role
            await _userManager.AddToRoleAsync(user, role);

            TempData["SuccessMessage"] = $"Role for {user.FullName} updated to {role}.";
            return RedirectToAction("Index");
        }
    }
}
