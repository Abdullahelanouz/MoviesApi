using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesApi.Services;

namespace MoviesApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        
        private new List<string> _allowedExtenstions = new List<string> { ".jpg", ".png" };
        private long _maxAllowedPosterSize = 1048576;
        private IMoviesService _MoviesService;
        private readonly IGenresService _genresService;

        public MoviesController(IMoviesService MoviesService , IGenresService genresService)
        {
            _MoviesService = MoviesService;
            _genresService= genresService;
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
            var movie = await _MoviesService.GetById(id);

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
            var movies = await _MoviesService.GetAll();
            //TODO : map movies to DTO
            return Ok(movies);

        }


        [HttpPost]

        public async Task<IActionResult> CreateAsync([FromForm] MovieDto dto)
        {
           
            if (!_allowedExtenstions.Contains(Path.GetExtension(dto.poster.FileName).ToLower()))
                return BadRequest("Only .png and .jpg images are allowed!");

            if (dto.poster.Length > _maxAllowedPosterSize)
                return BadRequest("Max allowed size for poster is 1MB!");

            var isValidGenre = await _genresService.IsvalidGenre(dto.GenreId);
            if (!isValidGenre)
                return BadRequest("Invalid genere ID!");
            if (dto.poster == null)
            {
                return BadRequest("Poster is required!");
            }

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
            await _MoviesService.Add(movie);
            return Ok(movie);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromForm] MovieDto dto)
        {
            var movie = await _MoviesService.GetById(id);

            if (movie == null)
                return NotFound($"No movie was found with ID {id}");

            var isValidGenre = await _genresService.IsvalidGenre(dto.GenreId);

            if (!isValidGenre)
                return BadRequest("Invalid genere ID!");

            if (dto.poster != null)
            {
                if (!_allowedExtenstions.Contains(Path.GetExtension(dto.poster.FileName).ToLower()))
                    return BadRequest("Only .png and .jpg images are allowed!");

                if (dto.poster.Length > _maxAllowedPosterSize)
                    return BadRequest("Max allowed size for poster is 1MB!");

                using var dataStream = new MemoryStream();

                await dto.poster.CopyToAsync(dataStream);

                movie.poster = dataStream.ToArray();
            }

            movie.Title = dto.Title;
            movie.GenreId = dto.GenreId;
            movie.Year = dto.Year;
            movie.StoreLine = dto.StoreLine;
            movie.Rate = dto.Rate;

            _MoviesService.Update(movie);
            return Ok(movie);
        }


        [HttpDelete("{id}")]
    public  async Task<IActionResult> DeleteAsync(int id)
    {
            var movie = await _MoviesService.GetById(id);
        if (movie == null)

            return NotFound( $"No movie was found with ID {id} ");

            _MoviesService.Delete(movie); 

            return Ok(movie);

    }


    }
}
