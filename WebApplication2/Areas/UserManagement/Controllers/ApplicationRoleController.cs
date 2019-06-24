using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Areas.UserManagement.Constants;
using WebApplication2.Areas.UserManagement.Models;

namespace WebApplication2.Areas.UserManagement.Controllers
{
    [Area("UserManagement")]
    [Authorize(Policy = PolicyNames.AdministratorPolicy)]
    public class ApplicationRoleController : Controller
    {
       
        private readonly RoleManager<IdentityRole> roleManager;

        public ApplicationRoleController(RoleManager<IdentityRole> roleManager)
        {
            this.roleManager = roleManager;
        }

        public ActionResult Index()
        {
            List<ApplicationRolesViewModel> viewModel = new List<ApplicationRolesViewModel>();
            viewModel = roleManager.Roles.Select(r => new ApplicationRolesViewModel
            {
                Id = r.Id,
                RoleName = r.Name
            }).ToList();

            return View(viewModel);
        }

        public ActionResult Create()
        {
            ApplicationRolesViewModel viewModel = new ApplicationRolesViewModel();
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(ApplicationRolesViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                IdentityRole identityRole = new IdentityRole();
                identityRole.Name = viewModel.RoleName;
                IdentityResult roleResult = await roleManager.CreateAsync(identityRole);

                if (roleResult.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                AddErrors(roleResult);
            }
            return View(viewModel);
        }

        public async Task<ActionResult> Edit(string id)
        {
            ApplicationRolesViewModel viewModel = new ApplicationRolesViewModel();
            if (!String.IsNullOrEmpty(id))
            {
                IdentityRole identityRole = await roleManager.FindByIdAsync(id);
                if (identityRole != null)
                {
                    viewModel.Id = identityRole.Id;
                    viewModel.RoleName = identityRole.Name;
                    return View(viewModel);
                }
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(string id, ApplicationRolesViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                if (!String.IsNullOrEmpty(id))
                {
                    IdentityRole identityRole = await roleManager.FindByIdAsync(id);
                    if (identityRole != null)
                    {
                        identityRole.Name = viewModel.RoleName;
                        IdentityResult roleResult = await roleManager.UpdateAsync(identityRole);

                        if (roleResult.Succeeded)
                        {
                            return RedirectToAction("Index");
                        }
                        AddErrors(roleResult);
                    }
                }
            }
            return View();
        }

        public async Task<ActionResult> Delete(string id)
        {
            string roleName = string.Empty;
            if (!String.IsNullOrEmpty(id))
            {
                IdentityRole identityRole = await roleManager.FindByIdAsync(id);
                if (identityRole != null)
                {
                    roleName = identityRole.Name;
                }
            }
            return View("Delete", roleName);
        }


        //Not really how deleting roles should probably be handled - to be reviewed
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(string id, bool notused)
        {
            try
            {
                if (!String.IsNullOrEmpty(id))
                {
                    IdentityRole identityRole = await roleManager.FindByIdAsync(id);
                    if (identityRole != null)
                    {
                        IdentityResult roleResult = await roleManager.DeleteAsync(identityRole);

                        if (roleResult.Succeeded)
                        {
                            return RedirectToAction("Index");
                        }

                    }
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index");
            }
        }


        public async Task<ActionResult> ManageClaim(string id)
        {
            RoleClaimsViewModel viewModel = new RoleClaimsViewModel();
            if (!String.IsNullOrEmpty(id))
            {
                IdentityRole identityRole = await roleManager.FindByIdAsync(id);
                if (identityRole != null)
                {
                    //Get claims associated with the role
                    IList<Claim> roleClaimList = await roleManager.GetClaimsAsync(identityRole);
                    List<string> roleClaimTypeList = new List<string>();
                    foreach (var roleClaim in roleClaimList)
                    {
                        roleClaimTypeList.Add(roleClaim.Type);
                    }

                    viewModel.Id = identityRole.Id;
                    viewModel.RoleName = identityRole.Name;
                    viewModel.RoleClaims = new List<ClaimsViewModel>();

                    foreach (var claimName in ClaimNames.ClaimName)
                    {
                        viewModel.RoleClaims.Add(new ClaimsViewModel() { ClaimName = claimName, HasClaim = roleClaimTypeList.Contains(claimName) });
                    }
                    return View("ManageClaim", viewModel);
                }
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ManageClaim(string id, RoleClaimsViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                if (!String.IsNullOrEmpty(id))
                {
                    IdentityRole identityRole = await roleManager.FindByIdAsync(id);
                    if (identityRole != null)
                    {
                        //Get claims associated with the role
                        IList<Claim> roleClaimList = await roleManager.GetClaimsAsync(identityRole);
                        //Extract the claim type
                        List<string> roleClaimTypeList = new List<string>();
                        foreach (var roleClaim in roleClaimList)
                        {
                            roleClaimTypeList.Add(roleClaim.Type);
                        }

                        foreach (var roleClaim in viewModel.RoleClaims)
                        {
                            //create a new claim with the claim name
                            Claim claim = new Claim(roleClaim.ClaimName, "");
                            //get the associated claim from the role's claim list
                            Claim associatedClaim = roleClaimList.Where(x => x.Type == roleClaim.ClaimName).FirstOrDefault();

                            if (roleClaim.HasClaim && !roleClaimTypeList.Contains(roleClaim.ClaimName))
                            {
                                IdentityResult claimResult = await roleManager.AddClaimAsync(identityRole, claim);
                                if (!claimResult.Succeeded)
                                {
                                    //TODO log details and display some sort of error
                                }
                            }
                            else if (!roleClaim.HasClaim && roleClaimTypeList.Contains(roleClaim.ClaimName))
                            {
                                IdentityResult claimResult = await roleManager.RemoveClaimAsync(identityRole, associatedClaim);
                                if (!claimResult.Succeeded)
                                {
                                    //TODO log details and display some sort of error
                                }
                            }
                        }
                    }
                    return RedirectToAction("Index");
                }

            }


            return View(viewModel);
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