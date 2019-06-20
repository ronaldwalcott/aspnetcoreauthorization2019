using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Areas.Identity.Data;
using WebApplication2.Areas.UserManagement.Models;

namespace WebApplication2.Areas.UserManagement.Controllers
{
    [Area("UserManagement")]
    public class UserEditController : Controller
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<WebApplication2User> userManager;

        public UserEditController(UserManager<WebApplication2User> userManager, RoleManager<IdentityRole> roleManager)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
        }

        public async Task<ActionResult> Index()
        {
            List<UserListViewModel> viewModel = new List<UserListViewModel>();

            List<WebApplication2User> users = userManager.Users.ToList();

            foreach (var user in users)
            {
                IList<string> userRolesList = new List<string>();
                userRolesList = await userManager.GetRolesAsync(user);
                string roles = null;
                bool isLockedOut = await userManager.IsLockedOutAsync(user);

                foreach (var role in userRolesList)
                {
                    if (roles is null)
                    {
                        roles = role;
                    }
                    else
                    {
                        roles = roles + ", " + role;
                    }

                }

                viewModel.Add
                    (new UserListViewModel()
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        Name = user.Surname + ", " + user.FirstName + " " + user.MiddleName,
                        LockoutEnabled = isLockedOut,
                        RoleNames = roles
                    }
                    );
            }
            return View(viewModel);
        }

        public async Task<ActionResult> Edit(string id)
        {
            UserEditViewModel viewModel = new UserEditViewModel();
            if (!String.IsNullOrEmpty(id))
            {
                WebApplication2User user = await userManager.FindByIdAsync(id);
                if (user != null)
                {
                    viewModel.Id = user.Id;
                    viewModel.UserName = user.UserName;
                    viewModel.Surname = user.Surname;
                    viewModel.FirstName = user.FirstName;
                    viewModel.MiddleName = user.MiddleName;
                    viewModel.Email = user.Email;
                    viewModel.EmployeeNumber = user.EmployeeNumber;
                    viewModel.LockoutEnabled = await userManager.IsLockedOutAsync(user);

                    //add all rolenames default to false
                    viewModel.UserRoles = roleManager.Roles.Select(r => new UserRolesViewModel
                    {
                        Id = r.Id,
                        RoleName = r.Name,
                        HasRole = false

                    }).ToList();


                    //get all user's roles
                    IList<string> userRolesList = new List<string>();
                    userRolesList = await userManager.GetRolesAsync(user);

                    //populate HasRole boolean field in the view model if the user has the role because we want to display all roles
                    //marking the ones which the user currently has
                    foreach (var userrole in viewModel.UserRoles)
                    {
                        if (userRolesList.Contains(userrole.RoleName))
                        {
                            userrole.HasRole = true;
                        }
                    }

                    //foreach (var role in userRolesList)
                    //{
                    //    foreach (var userrole in viewModel.UserRoles)
                    //    {
                    //        if (role == userrole.RoleName)
                    //        {
                    //            userrole.HasRole = true;
                    //        }
                    //    }
                    //}

                    return View(viewModel);
                }


            }

            return RedirectToAction("Index");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(string id, UserEditViewModel viewModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    WebApplication2User user = await userManager.FindByIdAsync(id);
                    if (user != null)
                    {
                        user.Surname = viewModel.Surname;
                        user.FirstName = viewModel.FirstName;
                        user.MiddleName = viewModel.MiddleName;
                        user.Email = viewModel.Email;
                        user.EmployeeNumber = viewModel.EmployeeNumber;

                        IdentityResult userResult = await userManager.UpdateAsync(user);
                        if (userResult.Succeeded)
                        {
                            //changes to roles are not tracked therefore every role has to be checked for updates
                            foreach (var role in viewModel.UserRoles)
                            {
                                if (await userManager.IsInRoleAsync(user, role.RoleName) & !role.HasRole)
                                {
                                    IdentityResult roleResult = await userManager.RemoveFromRoleAsync(user, role.RoleName);
                                    if (!roleResult.Succeeded)
                                    {
                                        AddErrors(roleResult);
                                    }
                                }
                                else if (role.HasRole & !(await userManager.IsInRoleAsync(user, role.RoleName)))
                                {
                                    IdentityResult roleResult = await userManager.AddToRoleAsync(user, role.RoleName);
                                    if (!roleResult.Succeeded)
                                    {
                                        AddErrors(roleResult);
                                    }
                                }
                            }
                        }
                        else
                        {
                            AddErrors(userResult);
                            return View(viewModel);
                        }

                    }
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return View(viewModel);
            }
        }

        public async Task<ActionResult> Delete(string id)
        {
            string fullname = string.Empty;
            if (!String.IsNullOrEmpty(id))
            {
                WebApplication2User user = await userManager.FindByIdAsync(id);
                if (user != null)
                {
                    fullname = user.Surname + ", " + user.FirstName + " " + user.MiddleName;
                }
                else
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            return View("Delete", fullname);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(string id, bool notused)
        {
            try
            {
                if (!String.IsNullOrEmpty(id))
                {
                    WebApplication2User user = await userManager.FindByIdAsync(id);
                    if (user != null)
                    {
                        string fullname = string.Empty;
                        fullname = user.Surname + ", " + user.FirstName + " " + user.MiddleName;

                        IdentityResult userResult = await userManager.DeleteAsync(user);

                        if (userResult.Succeeded)
                        {
                            return RedirectToAction("Index");
                        }
                        else
                        {
                            AddErrors(userResult);
                            return View("Delete", fullname);
                        }
                    }
                }
                
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
        //if some weird error is occurring write logs and return to the index view
                return RedirectToAction("Index");
            }
        }

        public async Task<ActionResult> Lockout(string id)
        {
            UserLockoutViewModel viewModel = new UserLockoutViewModel();
            if (!String.IsNullOrEmpty(id))
            {
                WebApplication2User user = await userManager.FindByIdAsync(id);
                
                if (user != null)
                {
                    bool isLockedOut = await userManager.IsLockedOutAsync(user);
                    if (isLockedOut)
                    {
                        return RedirectToAction("Index");
                    }

                    viewModel.Id = user.Id;
                    viewModel.UserName = user.UserName;
                    viewModel.Surname = user.Surname;
                    viewModel.FirstName = user.FirstName;
                    viewModel.MiddleName = user.MiddleName;
                    viewModel.Email = user.Email;
                    viewModel.EmployeeNumber = user.EmployeeNumber;

                    return View(viewModel);
                }
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Lockout(string id, UserLockoutViewModel viewModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    WebApplication2User user = await userManager.FindByIdAsync(id);
                    if (user != null)
                    {
                        bool isLockedOut = await userManager.IsLockedOutAsync(user);
                        if (isLockedOut)
                        {
                            return RedirectToAction("Index");
                        }

                        IdentityResult userResult = await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
                        if (userResult.Succeeded)
                        {
                            IdentityResult lockoutResult = await userManager.SetLockoutEnabledAsync(user, true);
                            if (lockoutResult.Succeeded)
                            {
                                return RedirectToAction(nameof(Index));
                            }
                        }
                        else
                        {
                            AddErrors(userResult);
                            return View(viewModel);
                        }

                    }
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "There was an error");
                return View(viewModel);
            }
        }

        public async Task<ActionResult> UnLock(string id)
        {
            UserLockoutViewModel viewModel = new UserLockoutViewModel();
            if (!String.IsNullOrEmpty(id))
            {
                WebApplication2User user = await userManager.FindByIdAsync(id);
                if (user != null)
                {
                    bool isLockedOut = await userManager.IsLockedOutAsync(user);
                    if (!isLockedOut)
                    {
                        return RedirectToAction("Index");
                    }

                    viewModel.Id = user.Id;
                    viewModel.UserName = user.UserName;
                    viewModel.Surname = user.Surname;
                    viewModel.FirstName = user.FirstName;
                    viewModel.MiddleName = user.MiddleName;
                    viewModel.Email = user.Email;
                    viewModel.EmployeeNumber = user.EmployeeNumber;

                    return View(viewModel);
                }
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UnLock(string id, UserLockoutViewModel viewModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    WebApplication2User user = await userManager.FindByIdAsync(id);
                    if (user != null)
                    {
                        bool isLockedOut = await userManager.IsLockedOutAsync(user);
                        if (!isLockedOut)
                        {
                            return RedirectToAction("Index");
                        }

                        IdentityResult userResult = await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddDays(-1));
                        if (userResult.Succeeded)
                        {
                            IdentityResult resetResult = await userManager.ResetAccessFailedCountAsync(user);
                            IdentityResult lockoutResult = await userManager.SetLockoutEnabledAsync(user, false);
                            if (lockoutResult.Succeeded)
                            {
                                return RedirectToAction(nameof(Index));
                            }
                        }
                        else
                        {
                            AddErrors(userResult);
                            return View(viewModel);
                        }

                    }
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "There was an error");
                return View(viewModel);
            }
        }

        public async Task<ActionResult> ResetPassword(string id)
        {
            UserPasswordResetViewModel viewModel = new UserPasswordResetViewModel();
            if (!String.IsNullOrEmpty(id))
            {
                WebApplication2User user = await userManager.FindByIdAsync(id);
                if (user != null)
                {
                    //if user is locked out user has to be unlocked before resetting password
                    bool isLockedOut = await userManager.IsLockedOutAsync(user);
                    if (isLockedOut)
                    {
                        return RedirectToAction("Index");
                    }

                    viewModel.Id = user.Id;
                    viewModel.UserName = user.UserName;
                    viewModel.Surname = user.Surname;
                    viewModel.FirstName = user.FirstName;
                    viewModel.MiddleName = user.MiddleName;
                    viewModel.Email = user.Email;
                    viewModel.EmployeeNumber = user.EmployeeNumber;

                    return View(viewModel);
                }
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(string id, UserPasswordResetViewModel viewModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    WebApplication2User user = await userManager.FindByIdAsync(id);
                    if (user != null)
                    {
                        IdentityResult removeResult = await userManager.RemovePasswordAsync(user);
                        if (removeResult.Succeeded)
                        {
                            IdentityResult addResult = await userManager.AddPasswordAsync(user, viewModel.Password);
                            if (addResult.Succeeded)
                            {
                                return RedirectToAction(nameof(Index));
                            }
                        }

                    }
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "There was an error");
                return View(viewModel);
            }
        }


        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

    }
}