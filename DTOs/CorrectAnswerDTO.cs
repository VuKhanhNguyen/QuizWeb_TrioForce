namespace QuizWeb_TrioForce.DTOs
{
    public class CorrectAnswerDTO
    {
        public int QuestionId { get; set; }
        public HashSet<int> CorrectAnswerIds { get; set; } = new HashSet<int>();
    }

}
