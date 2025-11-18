using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizWeb_TrioForce.DTOs;
using QuizWeb_TrioForce.Services.Interfaces;
using QuizWeb_TrioForce.ViewModels;
using System.Globalization;

namespace QuizWeb_TrioForce.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ILogger<ProfileController> _logger;
        private readonly IUserService _userService;

        public ProfileController(ILogger<ProfileController> logger, IUserService userService)
        {
            _logger = logger;
            _userService = userService;
        }


        // GET: ProfileController
        public async Task<IActionResult> Index()
        {
            if (!User.Identity?.IsAuthenticated == true)
            {
                return Unauthorized();
            }
            var username = User.Identity?.Name!;
            var user = await _userService.GetProfileAsync(username);
            if (user == null)
            {
                return NotFound();
            }

            var viewModel = new UserEditViewModel
            {
                Username = user.UserName!,
                Birthday = user.BirthDay.ToString("dd/MM/yyyy"),
                Email = user.Email!,
                Fullname = user.FullName,
                Sex = user.Sex,
            };
            return View(viewModel);
        }



        // POST: ProfileController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateField([FromBody] UpdateFieldRequest request)
        {
            try
            {

                if (!User.Identity?.IsAuthenticated == true)
                {
                    return Json(new { success = false, message = "Unauthorized" });
                }

                var user = await _userService.GetProfileAsync(User.Identity?.Name!);

                if (user == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                switch (request.FieldName)
                {
                    case "FullName":
                        user.FullName = request.Value;
                        break;
                    case "Email":
                        user.Email = request.Value;
                        user.NormalizedEmail = request.Value?.Trim().ToUpper();
                        break;
                    case "BirthDay":
                        if (DateOnly.TryParseExact(request.Value, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                        {
                            user.BirthDay = date;
                        }
                        else
                        {
                            return Json(new { success = false, message = "Invalid date format" });
                        }
                        break;
                    case "Sex":
                        if (bool.TryParse(request.Value,out var sex))
                        {
                            user.Sex = sex;
                        }else 
                        {
                            return Json(new { success = false, message = "Invalid sex field" });
                        }
                        break;  

                    default:
                        return Json(new { success = false, message = "Invalid field" });
                }


                await _userService.UpdateProfileAsync(user);
                return Json(new { success = true, value = request.Value });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating field");
                return Json(new { success = false, message = "An error occurred while updating the field." });
            }
        }
    }
}
