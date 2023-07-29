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

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var movies = await _context.Movies.Include(m => m.Genre)
                .Select(m => new MovieDetailsDto
                {
                    Id = m.Id,
                    GenreId = m.GenreId,
                    GenreName = m.Genre.Name,
                    poster = m.poster,
                    Rate = m.Rate,
                    StoreLine = m.StoreLine,
                    Title = m.Title,
                    Year = m.Year,
                })
                .ToListAsync();
            return Ok(movies);
        }
        [HttpGet("{id}")]

        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var movie = await _context.Movies
                .Include(m => m.Genre).SingleOrDefaultAsync(m => m.Id == id);

            if (movie == null)

                return NotFound();
            var dto = new MovieDetailsDto
            {
                Id = movie.Id,
                GenreId = movie.GenreId,
                GenreName = movie.Genre.Name,
                poster = movie.poster,
                Rate = movie.Rate,
                StoreLine = movie.StoreLine,
                Title = movie.Title,
                Year = movie.Year,

            };

            return Ok(dto);

        }
        [HttpGet("GetByGenreId")]

        public async Task<IActionResult> GetByGenreIdAsync(byte genreId)
        {
            var movies = await _context.Movies
                .Where(m => m.GenreId == genreId)
                .Include(m => m.Genre)
                .Select(m => new MovieDetailsDto
                {
                    Id = m.Id,
                    GenreId = m.GenreId,
                    GenreName = m.Genre.Name,
                    poster = m.poster,
                    Rate = m.Rate,
                    StoreLine = m.StoreLine,
                    Title = m.Title,
                    Year = m.Year,
                })
                .ToListAsync();
            return Ok(movies);

        }


        [HttpPost]

        public async Task<IActionResult> CreateAsync([FromForm] MovieDto dto)
        {
            if (!_allowedExtenstions.Contains(Path.GetExtension(dto.poster.FileName).ToLower()))
                return BadRequest("Only .png and .jpg images are allowed!");

            if (dto.poster.Length > _maxAllowedPosterSize)
                return BadRequest("Max allowed size for poster is 1MB!");

            var isValidGenre = await _context.Genres.AnyAsync(g => g.id == dto.GenreId);
            if (!isValidGenre)
                return BadRequest("Invalid genere ID!");

            using var dataStream = new MemoryStream();

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
   


    [HttpDelete("{id}")]
    public  async Task<IActionResult> DeleteAsync(int id)
    {
        var movie = await _context.Movies.FindAsync(id);
        if (movie == null)

            return NotFound( $"No movie was found with ID {id} ");
          
         _context.Remove(movie);
            _context.SaveChanges();  

            return Ok(movie);

    }


    }
}
