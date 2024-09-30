using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Clubs.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EliteClubsController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public EliteClubsController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [HttpGet]
        public async Task<IActionResult> GetEliteClubs([FromQuery] string nation)
        {
            if (string.IsNullOrEmpty(nation))
            {
                return BadRequest(new { error = "Nation is required as a query parameter" });
            }

            string baseUrl = $"https://jsonmock.hackerrank.com/api/football_teams?nation={nation}";
            List<FootballTeam> allTeams = new List<FootballTeam>();
            int page = 1;
            int totalPages = 1;

            // Fetch paginated data
            do
            {
                string url = $"{baseUrl}&page={page}";
                HttpResponseMessage response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, "Error fetching data from API");
                }

                var responseData = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<FootballApiResponse>(responseData);

                if (apiResponse == null || apiResponse.data == null)
                {
                    return NotFound(new { error = "No data found for the specified nation" });
                }

                allTeams.AddRange(apiResponse.data);
                totalPages = apiResponse.total_pages;
                page++;

            } while (page <= totalPages);

            // Sort the teams by valuation in descending order
            var sortedTeams = allTeams.OrderByDescending(team => team.estimated_value_numeric).ToList();

            return Ok(sortedTeams);
        }
    }

    // Model to represent the Football Team
    public class FootballTeam
    {
        public string name { get; set; }
        public ulong estimated_value_numeric { get; set; }
    }

    // Model to represent the API Response
    public class FootballApiResponse
    {
        public int page { get; set; }
        public int per_page { get; set; }
        public int total { get; set; }
        public int total_pages { get; set; }
        public List<FootballTeam> data { get; set; }
    }
}
