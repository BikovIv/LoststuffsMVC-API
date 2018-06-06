using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using LostStuffs.Entities;
using LostStuffs.Models;
using LostStuffs.DataAccess;
using LostStuffs.Models.CommentModels;
using System.Security.Claims;

namespace LostStuffs.Controllers
{

    [Authorize]
    [RoutePrefix("api/comment")]
    public class CommentsApiController : ApiController
    {
        CommentsRepository repository = new CommentsRepository();
        private ApplicationDbContext db = new ApplicationDbContext();

        LostStuffsRepository repo = new LostStuffsRepository();

        /// <summary>
        /// Get all comments for item. id=item id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // GET: api/CommentsApi
        [AllowAnonymous]
        [Route("get")]
        public IHttpActionResult GetComments(int id)  //LostStuff id !
        {
            try
            {
                List<Comment> allCommetns = new List<Comment>();
                allCommetns.AddRange(repository.GetAll().Where(x => x.LostStuffId == id));

                List<object> result = new List<object>();

                var lostStuffName = repo.GetById(id).Name;

                foreach (var comment in allCommetns)
                {
                    result.Add(new
                    {
                        comment.Id,
                        comment.UserName,
                        comment.UserId,
                        comment.LostStuffId,
                        comment.Content,
                        lostStuffName
                    }
                    );
                }
                return Ok(result);
            }
            catch (Exception)
            {
               
                return BadRequest("Invalid id!");
            }
          
        }

        /// <summary>
        /// Add comment. id = item id. Requires authentication go to /users/login - Username:Admin, Password: Password12*, grant_type:password
        /// </summary>
        /// <param name="id"></param>
        /// <param name="comment"></param>
        /// <returns></returns>
        // POST: api/CommentsApi
        [ResponseType(typeof(Comment))]
        [HttpPost]
        [Route("post")]
        [Authorize]
        public IHttpActionResult PostComment(int id, Comment comment)
        {
            var userData = GetCurrentUser(db);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            comment.LostStuffId = id;
            comment.CreatedAt = DateTime.Now;
            comment.UpdatedAt = DateTime.Now;
            comment.UserId = userData.Id;
            comment.UserName = userData.UserName;         
            repository.Add(comment);

            return Ok(comment);
        }

        /// <summary>
        /// Edit comment. id = comment id. Requires authentication go to /users/login - Username:Admin, Password: Password12*, grant_type:password
        /// </summary>
        /// <param name="id"></param>
        /// <param name="commentRequest"></param>
        /// <returns></returns>
        // PUT: api/CommentsApi/5
        [ResponseType(typeof(void))]
        [Route("edit")]
        public IHttpActionResult PutComment(int id, CommentRequestModel commentRequest)
        {
            Comment comment;
            var userData = GetCurrentUser(db);
          
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                comment = repository.GetById(id);
            }
            catch (Exception)
            {

                throw;
            }

            if (userData.Id.Equals(comment.UserId))
            {

                if (id != comment.Id)
                {
                    return BadRequest();
                }
                //
                commentRequest.Id = comment.Id;
                commentRequest.LostStuffId = comment.LostStuffId;
                //

                comment.UpdatedAt = DateTime.Now;
                comment.Content = commentRequest.Content;

                repository.Update(comment);

                return Ok("The comment was edited " + 
                    new { comment.Id,
                        comment.Content,
                        comment.LostStuffId,
                        comment.UserId,
                        comment.UserName
                    });
            }
            else
            {
                return BadRequest("You do not have permissions to edit!");
            }
        }

        /// <summary>
        /// Delete comment. id = comment id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // DELETE: api/CommentsApi/5
        [ResponseType(typeof(Comment))]
        [Route("delete")]
        public IHttpActionResult DeleteComment(int id)
        {
            Comment comment = repository.GetById(id);
            if (comment == null)
            {
                return NotFound();
            }

            repository.Delete(comment);

            return Ok("The comment is deleted");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool CommentExists(int id)
        {
            return db.Comments.Count(e => e.Id == id) > 0;
        }


        private ApplicationUser GetCurrentUser(ApplicationDbContext context)
        {
            var identity = User.Identity as ClaimsIdentity;
            Claim identityClaim = identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

            return context.Users.FirstOrDefault(u => u.Id == identityClaim.Value);
        }
    }
}