using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;

namespace Demo.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class LoginController : Controller
	{
		[HttpGet("Login")]
		public async Task<IActionResult> Login()
		{
			var accessToken = await HttpContext.GetTokenAsync("access_token");
			var client = new HttpClient();
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

			var content = await client.GetStringAsync("https://localhost:6001/identity");

			return Json(content);
		}
	}
}
