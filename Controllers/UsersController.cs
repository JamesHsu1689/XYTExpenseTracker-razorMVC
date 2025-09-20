using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ExpenseTracker.Models;

namespace ExpenseTracker.Controllers
{
    [Authorize(Policy = "RequireAdmin")] // or [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(UserManager<ApplicationUser> userManager,
                               RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: /Users
        public async Task<IActionResult> Index()
        {
            var users = _userManager.Users.ToList();
            var list = new List<UserRowVM>();
            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                list.Add(new UserRowVM
                {
                    Id = u.Id,
                    Email = u.Email ?? u.UserName ?? "(no email)",
                    Name = string.Join(" ", new[] { u.FirstName, u.LastName }
                                        .Where(s => !string.IsNullOrWhiteSpace(s))),
                    CurrentRole = roles.FirstOrDefault() ?? "(none)"
                });
            }
            return View(list);
        }

        // GET: /Users/Edit/{id}
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user is null) return NotFound();

            var current = (await _userManager.GetRolesAsync(user)).FirstOrDefault();
            return View(new EditUserRoleVM
            {
                Id = user.Id,
                Email = user.Email ?? user.UserName ?? "(no email)",
                SelectedRole = current,
                AvailableRoles = new[] { "Admin", "Staff", "User" }
            });
        }

        // POST: /Users/Edit
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserRoleVM model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user is null) return NotFound();

            var managed = new[] { "Admin", "Staff", "User" };

            // Ensure roles exist (in case seeding didn’t run)
            foreach (var r in managed)
                if (!await _roleManager.RoleExistsAsync(r))
                    await _roleManager.CreateAsync(new IdentityRole(r));

            // Remove any of the managed roles the user currently has
            var current = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, managed.Intersect(current));

            // Add the selected role (if any)
            if (!string.IsNullOrWhiteSpace(model.SelectedRole))
                await _userManager.AddToRoleAsync(user, model.SelectedRole);

            TempData["Status"] = "Role updated.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Users/Delete/{id}  (confirmation page)
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user is null) return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            ViewBag.IsSelf = (currentUserId == user.Id);

            var roles = await _userManager.GetRolesAsync(user);
            var vm = new DeleteUserVM
            {
                Id = user.Id,
                Email = user.Email ?? user.UserName ?? "(no email)",
                Name = string.Join(" ", new[] { user.FirstName, user.LastName }
                                    .Where(s => !string.IsNullOrWhiteSpace(s))),
                Roles = roles.ToList()
            };
            return View(vm);
        }

        // POST: /Users/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
            {
                TempData["Status"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }

            // Guard 1: don't delete yourself
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == user.Id)
            {
                TempData["Status"] = "You cannot delete your own account.";
                return RedirectToAction(nameof(Index));
            }

            // Guard 2: don't delete the last remaining Admin
            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                var admins = await _userManager.GetUsersInRoleAsync("Admin");
                if (admins.Count <= 1)
                {
                    TempData["Status"] = "You cannot delete the last remaining Admin.";
                    return RedirectToAction(nameof(Index));
                }
            }

            // If later you link ApplicationUser -> Person, clean that up here BEFORE deleting the user.

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                TempData["Status"] = "Delete failed: " + string.Join("; ", result.Errors.Select(e => e.Description));
                return RedirectToAction(nameof(Index));
            }

            TempData["Status"] = "User deleted.";
            return RedirectToAction(nameof(Index));
        }
    }

    // ----- View Models -----

    public class UserRowVM
    {
        public string Id { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string? Name { get; set; }
        public string CurrentRole { get; set; } = default!;
    }

    public class EditUserRoleVM
    {
        public string Id { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string? SelectedRole { get; set; }
        public IEnumerable<string> AvailableRoles { get; set; } = Array.Empty<string>();
    }

    public class DeleteUserVM
    {
        public string Id { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string? Name { get; set; }
        public List<string> Roles { get; set; } = new();
    }
}
