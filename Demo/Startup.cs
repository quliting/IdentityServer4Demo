using System.Collections.Generic;
using IdentityServer4;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace Demo
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddIdentityServer().AddDeveloperSigningCredential().AddInMemoryApiScopes(Config.ApiScopes)
				.AddInMemoryClients(Config.Clients);
			services.AddControllers();
			services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo {Title = "Demo", Version = "v1"}); });

			services.AddAuthentication("Bearer").AddJwtBearer("Bearer", options =>
			{
				options.Authority = "http://localhost:50001";
				options.RequireHttpsMetadata = false;
				options.TokenValidationParameters = new TokenValidationParameters {ValidateAudience = false};
			});
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseSwagger();
				app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Demo v1"));
			}

			app.UseHttpsRedirection();

			app.UseRouting();
			app.UseIdentityServer();
			app.UseAuthorization();

			app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
		}

		public static class Config
		{
			public static IEnumerable<IdentityResource> IdentityResources =>
				new List<IdentityResource>
				{
					new IdentityResources.OpenId(),
					new IdentityResources.Profile()
				};


			public static IEnumerable<ApiScope> ApiScopes =>
				new List<ApiScope>
				{
					new("api1", "My API")
				};

			public static IEnumerable<Client> Clients =>
				new List<Client>
				{
					// machine to machine client
					new()
					{
						ClientId = "client",
						ClientSecrets = {new Secret("secret".Sha256())},

						AllowedGrantTypes = GrantTypes.ClientCredentials,
						// scopes that client has access to
						AllowedScopes = {"api1"}
					},

					// interactive ASP.NET Core MVC client
					new()
					{
						ClientId = "mvc",
						ClientSecrets = {new Secret("secret".Sha256())},

						AllowedGrantTypes = GrantTypes.Code,

						// where to redirect to after login
						RedirectUris = {"https://localhost:5002/signin-oidc"},

						// where to redirect to after logout
						PostLogoutRedirectUris = {"https://localhost:5002/signout-callback-oidc"},

						AllowedScopes = new List<string>
						{
							IdentityServerConstants.StandardScopes.OpenId,
							IdentityServerConstants.StandardScopes.Profile,
							"api1"
						}
					}
				};
		}
	}
}