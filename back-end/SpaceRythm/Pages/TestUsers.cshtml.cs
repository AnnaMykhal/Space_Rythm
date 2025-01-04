using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Threading.Tasks;

namespace SpaceRythm.Pages
{
    public class TestUsersModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public TestUsersModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public string ResponseMessage { get; set; }

        public async Task<IActionResult> OnGetAllUsersAsync()
        {
            var response = await _httpClient.GetAsync("https://localhost:44395/api/users");
            ResponseMessage = await response.Content.ReadAsStringAsync();
            return Page();
        }

        public async Task<IActionResult> OnGetUserByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"https://localhost:44395/api/users/{id}");
            ResponseMessage = await response.Content.ReadAsStringAsync();
            return Page();
        }

        public async Task<IActionResult> OnGetByUsernameAsync(string username)
        {
            var response = await _httpClient.GetAsync($"https://localhost:44395/api/users/by-username/{username}");
            ResponseMessage = await response.Content.ReadAsStringAsync();
            return Page();
        }

        public async Task<IActionResult> OnGetByEmailAsync(string email)
        {
            var response = await _httpClient.GetAsync($"https://localhost:44395/api/users/by-email/{email}");
            ResponseMessage = await response.Content.ReadAsStringAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostUploadAvatarAsync(IFormFile avatar)
        {
            var formContent = new MultipartFormDataContent();
            using (var stream = new MemoryStream())
            {
                await avatar.CopyToAsync(stream);
                formContent.Add(new StreamContent(new MemoryStream(stream.ToArray())), "avatar", avatar.FileName);
            }

            var response = await _httpClient.PostAsync("https://localhost:44395/api/users/upload-avatar", formContent);
            ResponseMessage = await response.Content.ReadAsStringAsync();
            return Page();
        }

        // Додайте методи для інших функцій контролера, якщо потрібно
    }
}
