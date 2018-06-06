using LostStuffs.Models;
using Microsoft.AspNet.Identity;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Microsoft.AspNet.Identity.Owin;
using System.Collections.Generic;
using System.Linq;

namespace LostStuffs.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [RoutePrefix("api/users")]
  
    public class AccountApiController : ApiController
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private ApplicationDbContext db = new ApplicationDbContext();

        

        public ApplicationUserManager UserManager
        {
            get
            {            
                return _userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager;
            }
            private set
            {
                _signInManager = value;
            }
        }



        /// <summary>
        /// Get all users
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("get-all")]
        public IHttpActionResult GetAllUsers()
        {
            var users = db.Users.Select(u => new { Id = u.Id, UserName = u.UserName, Email = u.Email });
            return Ok(users);
        }

        /// <summary>
        /// Create user
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Route("create")]
        public async Task<IHttpActionResult> Post(RegisterViewModel model)
        {

            if (!ModelState.IsValid)
            {
                return Content(HttpStatusCode.BadRequest, "Incorrect data!");
            }

            var user = new ApplicationUser { UserName = model.UserName, Email = model.Email };
            IdentityResult result = await UserManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                return Content(HttpStatusCode.OK, "User added successfully!");
            }
            return Content(HttpStatusCode.InternalServerError, "Error!");
        }

        /// <summary>
        /// Update user. Requires authentication go to /users/login - Username:Admin, Password: Password12*, grant_type:password
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("update")]
        [Authorize]
        public async Task<IHttpActionResult> Update(UpdateViewModel model)
        {

            if (!ModelState.IsValid)
            {
                return Content(HttpStatusCode.BadRequest, "Incorrect data!");
            }

            ApplicationUser user = UserManager.FindById(User.Identity.GetUserId());
            user.UserName = model.UserName;
            user.Email = model.Email;

            IdentityResult result = await UserManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return Content(HttpStatusCode.OK, "User added successfully!");
            }

            return InternalServerError();
        }
        /// <summary>
        /// Change password. Requires authentication go to /users/login - Username:Admin, Password: Password12*, grant_type:password
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut]
        [Authorize]
        [Route("change-password")]
        public async Task<IHttpActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
           
                return Ok("Password has been changed " + User.Identity.Name);
            
        }

        /// <summary>
        /// Delete user. Requires authentication go to /users/login - Username:Admin, Password: Password12*, grant_type:password
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Route("delete")]
        [HttpPost]
        [Authorize]
        public async Task<IHttpActionResult> Delete(DeleteViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Content(HttpStatusCode.BadRequest, "Incorrect username!");
            }

            var user = UserManager.FindByName(model.UserName); 
            IdentityResult result = await UserManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                return Content(HttpStatusCode.OK, "User deleted successfully!");
            }
            return Content(HttpStatusCode.InternalServerError, "Error!");
        }

        /// <summary>
        /// Login
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Route("login")]
        public IHttpActionResult Login(LoginViewModel model)
        {
            return Ok();
        }
    }
}
