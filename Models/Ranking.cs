using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace quizweb.Models
{
    public class Ranking
    {
        public string UserName { get; set; } = null!;
        public int TotalScore { get; set; }

        // Navigation property to ApplicationUser
        public ApplicationUser User { get; set; } = null!; 
    }
}
