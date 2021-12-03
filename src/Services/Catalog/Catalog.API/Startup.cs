using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Mongo.Migration.Documents;
using Mongo.Migration.Documents.Attributes;
using Mongo.Migration.Migrations.Database;
using Mongo.Migration.Startup;
using Mongo.Migration.Startup.DotNetCore;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Catalog.API.Entities;
using Catalog.API.Data;
using Catalog.API.Repositories;

namespace Catalog.API
{
    public class Startup
    {
        private MongoClient _client;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Catalog.API", Version = "v1" });
            });
            _client = new MongoClient(Configuration["ConnectionStrings:Mongo"]);
            services.AddSingleton<IMongoClient>(_client);
            services.AddMigration(
                new MongoMigrationSettings
            {
                ConnectionString = Configuration["ConnectionStrings:Mongo"],
                Database = "CatalogDB",
                VersionFieldName = "TestVersionName" // Optional
            });
            services.AddScoped<ICatalogContext, CatalogContext>();
            services.AddScoped<IProductRepository, ProductRepository>();

        }
        public class M100_AddNewCar : DatabaseMigration
        {
            public M100_AddNewCar()
                : base("1.0.0")
            {
            }

            public override void Up(IMongoDatabase db)
            {
                var collection = db.GetCollection<Product>("Products");
                collection.InsertMany(new[]
                     { 
                    new Product
                    {
                        Name = "Asus Laptop",
                        Category= "Computers",
                        Summary= "Summary",
                        Description= "Description",
                        ImageFile= "ImageFile",
                        Price= 54.93m
                    },
                    new Product
                    {
                        Name = "HP Laptop",
                        Category= "Computers",
                        Summary= "Summary",
                        Description= "Description",
                        ImageFile= "ImageFile",
                        Price= 88.93m
                    }
                    });
            }

            public override void Down(IMongoDatabase db)
            {
                var collection = db.GetCollection<Product>("Product");
                collection.DeleteOne(Builders<Product>.Filter.Eq(c => c.Name, "AddedInMigration"));
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Catalog.API v1"));
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

       
    }
}
