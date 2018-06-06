using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using LostStuffs.Entities;
using LostStuffs.Models;
using LostStuffs.DataAccess;
using System.Net.Http;
using System;
using System.Web;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using System.IO;
using System.Security.Claims;

namespace LostStuffs.Controllers
{
    [RoutePrefix("api/lost-stuffs")]
    public class LostStuffsAPIController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        LostStuffsRepository repository = new LostStuffsRepository();
        private ApplicationUser user = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>().FindById(HttpContext.Current.User.Identity.GetUserId());

        /// <summary>
        /// Get all items
        /// </summary>
        /// <returns></returns>
        // GET: api/LostStuffsAPI
        [Route("get-all")]
        [HttpGet]
        public IHttpActionResult Get()
        {
            List<LostStuff> allLostStuffs = new List<LostStuff>();
            allLostStuffs = repository.GetAll();

            List<object> result = new List<object>();

            foreach (var item in allLostStuffs)
            {
                result.Add(new
                {
                    id = item.Id,
                    name = item.Name,
                    decscription = item.Description,
                    userName = item.UserName,
                    price = item.Price,
                    imageName = item.ImageName,
                    imagePath = item.ImagePath,
                    phoneNumber = item.PhoneNumber
                });
            }

            return this.Ok(result);
        }

        /// <summary>
        /// Get item by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // GET: api/LostStuffsAPI/5
        [Route("get")]
        public IHttpActionResult GetLostStuff(int id)
        {
            LostStuff lostStuff = repository.GetById(id);

            if (lostStuff == null)
            {
                return this.ResponseMessage(new HttpResponseMessage(HttpStatusCode.BadRequest));
            }

            return this.Ok(new
            {
                id = lostStuff.Id,
                name = lostStuff.Name,
                description = lostStuff.Description,
                price = lostStuff.Price,
                userId = lostStuff.UserId,
                userName = lostStuff.UserName
            });
        }

        /// <summary>
        /// Create item. Requires authentication go to /users/login - Username:Admin, Password: Password12*, grant_type:password
        /// </summary>
        /// <param name="lostStuffRequest"></param>
        /// <returns></returns>
        // POST: api/LostStuffsAPI
        [Authorize]
        [ResponseType(typeof(LostStuff))]
        [HttpPost]
        [Route("post")]
        public IHttpActionResult PostLostStuff(LostStuffRequestModel lostStuffRequest)      
        {
            var userData = GetCurrentUser(db);
            
            LostStuff lostStuff = new LostStuff();

            var httpRequest = HttpContext.Current.Request;

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            lostStuff.CreatedAt = DateTime.Now;
            lostStuff.UpdatedAt = DateTime.Now;
            lostStuff.UserId = userData.Id;
            lostStuff.UserName = userData.UserName;
            lostStuff.Name = lostStuffRequest.Name;
            lostStuff.Description = lostStuffRequest.Description;
            lostStuff.Price = lostStuffRequest.Price;
            lostStuff.PhoneNumber = lostStuffRequest.PhoneNumber;
            db.LostStuffs.Add(lostStuff);
            db.SaveChanges();

            ////create directory for entity
            string directoryPath = string.Format("~/Content/UploadedFiles/" + lostStuff.Id.ToString() + "/");
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(HttpContext.Current.Server.MapPath(directoryPath));
            }

            if (httpRequest.Files.Count > 0)
            {
                foreach (string file in httpRequest.Files)
                {
                    var postedFile = httpRequest.Files[file];

                    if (file.Equals("mainImage"))
                    {
                        if (httpRequest.Files[file] != null)
                        {
                            lostStuff.ImageName = postedFile.FileName;
                            lostStuff.ImagePath = directoryPath + postedFile.FileName;
                            postedFile.SaveAs(HttpContext.Current.Server.MapPath(directoryPath + postedFile.FileName));
                            db.SaveChanges();
                        }
                        else
                        {
                            lostStuff.ImageName = "default.jpg";
                            lostStuff.ImagePath = "~/Content/UploadedFiles/default.jpg";
                            db.SaveChanges();
                        }
                    }
                    else
                    {
                        if (httpRequest.Files[file] != null)
                        {
                            postedFile.SaveAs(HttpContext.Current.Server.MapPath(directoryPath + postedFile.FileName));
                        }
                        else
                        {
                            continue;
                        }
                    }
                }
            }
            else
            {
                lostStuff.ImageName = "default.jpg";
                lostStuff.ImagePath = "~/Content/UploadedFiles/default.jpg";
                db.SaveChanges();
            }
            return Ok();
        }

        /// <summary>
        /// Edit item. Requires authentication go to /users/login - Username:Admin, Password: Password12*, grant_type:password
        /// </summary>
        /// <param name="id"></param>
        /// <param name="lostStuffRequest"></param>
        /// <returns></returns>
        // PUT: api/LostStuffsAPI/5
        [ResponseType(typeof(void))]
        [Route("put")]
        [Authorize]
        public IHttpActionResult PutLostStuff(int id, LostStuffRequestModel lostStuffRequest)
        {
            var userData = GetCurrentUser(db);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            LostStuff lostStuff;
            try
            {
                lostStuff = repository.GetById(id);
               // lostStuffRequest.Id = lostStuff.Id;
            }
            catch (Exception ex)
            {
                return this.NotFound();
            }

            //if (lostStuff.Id != lostStuffRequest.Id)
            //{
            //    return BadRequest();
            //}

            lostStuff.UpdatedAt = DateTime.Now;
            lostStuff.UserId = userData.Id;
            lostStuff.UserName = userData.UserName;
            lostStuff.Name = lostStuffRequest.Name;
            lostStuff.Description = lostStuffRequest.Description;
            lostStuff.Price = lostStuffRequest.Price;
            lostStuff.PhoneNumber = lostStuffRequest.PhoneNumber;
            repository.Update(lostStuff);

            return this.Ok(lostStuffRequest);
        }

        /// <summary>
        /// Delete item
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // DELETE: api/LostStuffsAPI/5
        [ResponseType(typeof(LostStuff))]
        [Route("delete")]
        public IHttpActionResult DeleteLostStuff(int id)
        {
            LostStuff lostStuff = repository.GetById(id);
            if (lostStuff == null)
            {
                return NotFound();
            }

            string directoryPath = string.Format("~/Content/UploadedFiles/" + lostStuff.Id.ToString() + "/");

            try
            {
                Directory.Delete(HttpContext.Current.Server.MapPath(directoryPath));
            }catch (Exception ex)
            {

            }
            repository.Delete(lostStuff);
            return Ok(lostStuff);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool LostStuffExists(int id)
        {
            return db.LostStuffs.Count(e => e.Id == id) > 0;
        }

        private ApplicationUser GetCurrentUser(ApplicationDbContext context)
        {
            var identity = User.Identity as ClaimsIdentity;
            Claim identityClaim = identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

            return context.Users.FirstOrDefault(u => u.Id == identityClaim.Value);
        }
    }
}