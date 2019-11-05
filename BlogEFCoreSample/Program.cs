using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace BlogEFCoreSample
{
    class Program
    {
        static void Main(string[] args)
        {
            // context
            using var context = CreateContextSqlServer();
            //using var context = CreateContextSqlite();
            //using var context = CreateContextInMemory();

            // db
            CreateAndSeedDb(context);

            // query
            /**
             * -- Query generated for Sql Server
             * SELECT TOP(1) [p].[Id], [p].[PostDate]
             * FROM [Posts] AS [p]
             * WHERE (DATEPART(year, [p].[PostDate]) = 2019) AND (DATEPART(month, [p].[PostDate]) = 11)
             * 
             * -- Query generated for Sqlite
             * SELECT "p"."Id", "p"."PostDate"
             * FROM "Posts" AS "p"
             */
            var post = context.Posts.ToList().FirstOrDefault(p => p.PostDate.Year == 2019 && p.PostDate.Month == 11);

            Console.WriteLine(post);
        }

        static readonly ILoggerFactory loggerFactory = LoggerFactory.Create(builder => { builder.AddConsole(); });

        static BlogDbContext CreateContextSqlServer()
        {
            var options = new DbContextOptionsBuilder<BlogDbContext>()
                 .UseLoggerFactory(loggerFactory)
                 .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=BlogEFCoreSample;Trusted_Connection=True;MultipleActiveResultSets=true").Options;

            return new BlogDbContext(options);
        }

        static BlogDbContext CreateContextSqlite()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<BlogDbContext>()
                .UseLoggerFactory(loggerFactory)
                .UseSqlite(connection).Options;

            return new BlogDbContext(options);
        }

        static BlogDbContext CreateContextInMemory()
        {
            var options =  new DbContextOptionsBuilder<BlogDbContext>()
                .UseLoggerFactory(loggerFactory)
                .UseInMemoryDatabase("BlogEFCoreSample").Options;

            return new BlogDbContext(options);
        }

        static void CreateAndSeedDb(BlogDbContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var post = new Post { PostDate = DateTimeOffset.UtcNow };
            context.Posts.Add(post);
            context.SaveChanges();
        }
    }

    class BlogDbContext : DbContext
    {
        public BlogDbContext(DbContextOptions<BlogDbContext> options) : base(options)
        {

        }

        public DbSet<Post> Posts { get; set; }
    }

    class Post
    {
        public int Id { get; set; }
        public DateTimeOffset PostDate { get; set; }

        public override string ToString()
        {
            return $"{Id} {PostDate}";
        }
    }
}
