namespace MoviesApi.Dtos
{
    public class MovieDetailsDto
    {
        public int Id { get; set; }
      
        public string Title { get; set; }

        public int Year { get; set; }

        public double Rate { get; set; }
       
        public string StoreLine { get; set; }

        public byte[] poster { get; set; }

        public byte GenreId { get; set; }
        public String GenreName { get; set; }
    }
}
