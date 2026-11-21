using Microsoft.AspNetCore.Mvc;
using mcAPI.Models;

namespace mcAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private static List<Movie> _movies = new List<Movie>
        {
            new Movie
            {
                Id = 1,
                Title = "Dune: Messiah",
                Description = "Arrakis planetində yeni müharibə və Paul Atreidesin taleyi.",
                Genre = "Sci-Fi",
                ReleaseDate = new DateTime(2027, 12, 18, 20, 0, 0),
                AvailableTickets = 180
            },
            new Movie
            {
                Id = 2,
                Title = "The Batman: Part II",
                Description = "Qotem şəhərində yeni sirli cinayətlərin araşdırılması.",
                Genre = "Action",
                ReleaseDate = new DateTime(2027, 10, 01, 19, 30, 0),
                AvailableTickets = 95
            }
        };

        // 1. Bütün filmləri gətir
        [HttpGet]
        public ActionResult<IEnumerable<Movie>> GetAllMovies()
        {
            return Ok(_movies);
        }

        // 2. ID-yə görə filmi tap
        [HttpGet("{id}")]
        public ActionResult<Movie> GetMovieById(int id)
        {
            var movie = _movies.FirstOrDefault(m => m.Id == id);
            if (movie == null)
                return NotFound(new { message = $"ID-si {id} olan film tapılmadı." });

            return Ok(movie);
        }

        // 3. Yeni film əlavə et
        [HttpPost]
        public ActionResult<Movie> CreateMovie([FromBody] Movie newMovie)
        {
            var validationError = ValidateMovie(newMovie);
            if (validationError != null)
                return BadRequest(new { message = validationError });

            newMovie.Id = _movies.Any() ? _movies.Max(m => m.Id) + 1 : 1;
            _movies.Add(newMovie);

            return CreatedAtAction(nameof(GetMovieById), new { id = newMovie.Id }, newMovie);
        }

        // 4. Filmi yenilə
        [HttpPut("{id}")]
        public IActionResult UpdateMovie(int id, [FromBody] Movie updatedMovie)
        {
            var existingMovie = _movies.FirstOrDefault(m => m.Id == id);
            if (existingMovie == null)
                return NotFound(new { message = $"ID-si {id} olan film tapılmadı." });

            var validationError = ValidateMovie(updatedMovie);
            if (validationError != null)
                return BadRequest(new { message = validationError });

            existingMovie.Title = updatedMovie.Title;
            existingMovie.Description = updatedMovie.Description;
            existingMovie.Genre = updatedMovie.Genre;
            existingMovie.ReleaseDate = updatedMovie.ReleaseDate;
            existingMovie.AvailableTickets = updatedMovie.AvailableTickets;

            return Ok(existingMovie);
        }

        // 5. Filmi sil
        [HttpDelete("{id}")]
        public IActionResult DeleteMovie(int id)
        {
            var movie = _movies.FirstOrDefault(m => m.Id == id);
            if (movie == null)
                return NotFound(new { message = $"ID-si {id} olan film tapılmadı." });

            _movies.Remove(movie);
            return NoContent();
        }

        // 6.1. Ada görə axtarış
        [HttpGet("search")]
        public ActionResult<IEnumerable<Movie>> SearchMoviesByTitle([FromQuery] string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return BadRequest(new { message = "Axtarış üçün film adı daxil edilməlidir." });

            var results = _movies
                .Where(m => m.Title.Contains(title, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return Ok(results);
        }

        // 6.2. Tarixə görə sıralama
        [HttpGet("sorted")]
        public ActionResult<IEnumerable<Movie>> GetMoviesSortedByDate()
        {
            return Ok(_movies.OrderBy(m => m.ReleaseDate).ToList());
        }

        // 6.3. Yalnız gələcək premyeralar
        [HttpGet("upcoming")]
        public ActionResult<IEnumerable<Movie>> GetUpcomingMovies()
        {
            return Ok(_movies.Where(m => m.ReleaseDate > DateTime.Now).ToList());
        }

        // 6.4. Janra görə filtr
        [HttpGet("genre/{genre}")]
        public ActionResult<IEnumerable<Movie>> FilterMoviesByGenre(string genre)
        {
            return Ok(_movies.Where(m => m.Genre.Equals(genre, StringComparison.OrdinalIgnoreCase)).ToList());
        }

        // 6.5. Bilet sayı 100-dən çox olanlar
        [HttpGet("large-premiere")]
        public ActionResult<IEnumerable<Movie>> GetLargePremiereMovies()
        {
            return Ok(_movies.Where(m => m.AvailableTickets > 100).ToList());
        }

        // 6.6. JSON faylı kimi ixrac etmək
        [HttpGet("export")]
        public IActionResult ExportMoviesToJson()
        {
            var options = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
            var jsonString = System.Text.Json.JsonSerializer.Serialize(_movies, options);
            var bytes = System.Text.Encoding.UTF8.GetBytes(jsonString);

            return File(bytes, "application/json", "movies_export.json");
        }

        private string? ValidateMovie(Movie movie)
        {
            if (movie == null)
                return "Film məlumatları boş ola bilməz.";
            if (string.IsNullOrWhiteSpace(movie.Title))
                return "Filmin adı boş ola bilməz.";
            if (movie.ReleaseDate < DateTime.Now)
                return "Premyera tarixi keçmiş tarix ola bilməz.";
            if (movie.AvailableTickets < 1)
                return "Bilet sayı 1-dən az ola bilməz.";

            return null;
        }
    }
}