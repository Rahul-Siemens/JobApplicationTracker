using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using JobApplicationTrackerAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace JobApplicationTrackerAPI.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IConfiguration _config;

        public AuthController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IConfiguration config
        )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserDto userModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = new IdentityUser { UserName = userModel.Username };
            var result = await _userManager.CreateAsync(user, userModel.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { user.Id, user.UserName });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserDto userModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByNameAsync(userModel.Username);
            if (user == null)
                return Unauthorized();

            var result = await _signInManager.CheckPasswordSignInAsync(
                user,
                userModel.Password,
                false
            );
            if (!result.Succeeded)
                return Unauthorized();

            var token = CreateToken(user);
            return Ok(new { token });
        }

        [HttpGet("github-login")]
        public IActionResult GitHubLogin()
        {
            var clientId = _config["GitHub:ClientId"];
            var redirectUri = "http://localhost:5082/auth/github-callback";
            var scope = "user:email";
            
            var authUrl = $"https://github.com/login/oauth/authorize?client_id={clientId}&redirect_uri={redirectUri}&scope={scope}";
            
            return Redirect(authUrl);
        }

        [HttpGet("github-callback")]
        public async Task<IActionResult> GitHubCallback([FromQuery] string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return BadRequest("Authorization code is missing");
            }

            try
            {
                // Exchange code for access token
                var accessToken = await ExchangeCodeForAccessToken(code);
                
                // Get user info from GitHub
                var userInfo = await GetGitHubUserInfo(accessToken);
                
                // Find or create user
                var user = await FindOrCreateUser(userInfo);
                
                // Generate JWT token
                var token = CreateToken(user);
                
                // Redirect to frontend callback URL with token
                var frontendCallbackUrl = $"http://localhost:4200/auth/github-callback?token={Uri.EscapeDataString(token)}&username={Uri.EscapeDataString(user.UserName)}";
                return Redirect(frontendCallbackUrl);
            }
            catch (Exception ex)
            {
                // Redirect to frontend with error
                var frontendErrorUrl = $"http://localhost:4200/auth/github-callback?error={Uri.EscapeDataString(ex.Message)}";
                return Redirect(frontendErrorUrl);
            }
        }

        private async Task<string> ExchangeCodeForAccessToken(string code)
        {
            var clientId = _config["GitHub:ClientId"];
            var clientSecret = _config["GitHub:ClientSecret"];
            
            using var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://github.com/login/oauth/access_token")
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["client_id"] = clientId,
                    ["client_secret"] = clientSecret,
                    ["code"] = code
                })
            };
            
            request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            var json = System.Text.Json.JsonDocument.Parse(content);
            
            return json.RootElement.GetProperty("access_token").GetString();
        }

        private async Task<GitHubUserInfo> GetGitHubUserInfo(string accessToken)
        {
            using var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/user");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            request.Headers.UserAgent.ParseAdd("JobApplicationTrackerAPI");
            request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            var json = System.Text.Json.JsonDocument.Parse(content);
            
            return new GitHubUserInfo
            {
                Id = json.RootElement.GetProperty("id").GetInt64(),
                Login = json.RootElement.GetProperty("login").GetString(),
                Email = json.RootElement.TryGetProperty("email", out var email) ? email.GetString() : null,
                AvatarUrl = json.RootElement.TryGetProperty("avatar_url", out var avatar) ? avatar.GetString() : null
            };
        }

        private async Task<IdentityUser> FindOrCreateUser(GitHubUserInfo userInfo)
        {
            // Try to find user by GitHub ID
            var userName = $"github_{userInfo.Id}";
            var user = await _userManager.FindByNameAsync(userName);
            
            if (user == null)
            {
                // Create new user
                user = new IdentityUser 
                { 
                    UserName = userName,
                    Email = userInfo.Email
                };
                
                var result = await _userManager.CreateAsync(user);
                if (!result.Succeeded)
                {
                    throw new Exception($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            
            return user;
        }

        private string CreateToken(IdentityUser user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public class GitHubUserInfo
    {
        public long Id { get; set; }
        public string Login { get; set; }
        public string Email { get; set; }
        public string AvatarUrl { get; set; }
    }
}
