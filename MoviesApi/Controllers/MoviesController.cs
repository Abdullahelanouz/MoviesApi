using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MoviesApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private new List<string> _allowedExtenstions = new List<string> { ".jpg", ".png" };
        private long _maxAllowedPosterSize = 1048576;

        public MoviesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]   

        public async  Task<IActionResult> CreateAsync([FromForm] MovieDto dto)
        {
            if (!_allowedExtenstions.Contains(Path.GetExtension(dto.poster.FileName).ToLower()))
                return BadRequest("Only .png and .jpg images are allowed!");

            if (dto.poster.Length > _maxAllowedPosterSize)
                return BadRequest("Max allowed size for poster is 1MB!");

            var isValidGenre = await _context.Genres.AnyAsync(g => g.id == dto.GenreId);
            if (!isValidGenre)
                return BadRequest("Invalid genere ID!");

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
