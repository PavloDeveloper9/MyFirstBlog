﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MyFirstBlog.Models
{
    public class BlogContext(DbContextOptions<BlogContext> options) : IdentityDbContext<User>(options)
    {
        public DbSet<BlogPost> BlogPosts {  get; set; }
    }
}
