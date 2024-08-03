﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyFirstBlog.Models;

namespace MyFirstBlog.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlogController : ControllerBase
    {
        private readonly BlogContext _blogContext;
        private readonly UserManager<User> _userManager;

        public BlogController(BlogContext context, UserManager<User> userManager)
        {
            _blogContext = context;
            _userManager = userManager;            
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BlogPost>>> GetBlogPosts(int pageNum = 1, int pageSize = 3) 
        {
            var pagedBlogPosts = await _blogContext.BlogPosts
                .Skip((pageNum - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalPosts = await _blogContext.BlogPosts.CountAsync();

            var response = new PagedModel<BlogPost>
            {
                Data = pagedBlogPosts,
                PageNumber = pageNum,
                PageSize = pageSize,
                TotalCount = totalPosts
            };
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BlogPost>> GetBlogPost(Guid id) 
        {
            var blogPost = await _blogContext.BlogPosts.FindAsync(id);

            if (blogPost == null) 
            {
                return NotFound();
            }

            return blogPost;
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<BlogPost>> CreateBlogPost(PostBlog postBlog) 
        {
            var user = await _userManager.GetUserAsync(User);
            var newPost = new BlogPost { Title = postBlog.Title, Content = postBlog.Content, CreatedDate = postBlog.CreatedAt, User = user, IdUser = user.Id };
            //blogPost.IdUser = user.Id;
            //blogPost.User = user;
            //blogPost.CreatedDate = DateTime.UtcNow;

            //_blogContext.BlogPosts.Add(blogPost);
            //await _blogContext.SaveChangesAsync();

            //return CreatedAtAction(nameof(GetBlogPost), new {id = blogPost.Id}, blogPost);

            try
            {
                _blogContext.BlogPosts.Add(newPost);
                await _blogContext.SaveChangesAsync();

                return CreatedAtAction(nameof(GetBlogPost), new { id = newPost.Id }, newPost);
            }
            catch (Exception ex)
            {
                
                return StatusCode(500, ex);
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateBlogPost(Guid blogPostId, BlogPost updatePostBlog) 
        {
            if(blogPostId != updatePostBlog.Id) 
            {
                return BadRequest();
            }
            
            var user = await _userManager.GetUserAsync(User);
            var blogPost = await _blogContext.BlogPosts.FindAsync(blogPostId);

            if (blogPost == null) { return NotFound(); }

            if (blogPost.IdUser != user.Id) { return Forbid(); }

            blogPost.Title = updatePostBlog.Title;
            blogPost.Content = updatePostBlog.Content;

            _blogContext.Entry(blogPost).State = EntityState.Modified;

            try
            {
                await _blogContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException) 
            {
                if (!BlogPostExists(blogPostId)) { return NotFound(); }
                else { throw; }
            }

            return NoContent();

        }

        private bool BlogPostExists(Guid blogId) 
        {
            return _blogContext.BlogPosts.Any(b => b.Id == blogId);
        }

        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> DeleteBlogPost(Guid postId)
        {
            var user = await _userManager.GetUserAsync(User);
            var blogPost = await _blogContext.BlogPosts.FindAsync(postId);

            if(blogPost == null) { return NotFound(); }

            if (blogPost.IdUser != user.Id) { return Forbid(); }

            _blogContext.BlogPosts.Remove(blogPost);
            await _blogContext.SaveChangesAsync();

            return NoContent();
        }
    }
}
