using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MoviesApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MoviesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]   

        public async    Task<IActionResult> CreateAsync([FromForm] MovieDto dto)
        {
            using var dataStream= new MemoryStream();

            await dto.poster.CopyToAsync(dataStream);
            var movie = new Movie
            {
                GenreId = dto.GenreId,
                Title = dto.Title,
                poster = dataStream.ToArray(),
                Rate = dto.Rate,
                StoreLine = dto.StoreLine,
                Year = dto.Year,
            
            };
            await _context.AddAsync(movie);
            _context.SaveChangesAsync();
            return Ok(movie);
        }
    }
}
