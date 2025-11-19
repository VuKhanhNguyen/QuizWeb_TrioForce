namespace QuizWeb_TrioForce.ViewModels.BookMark
{
    public class CreatedQuestionSetListViewModel
    {
        public int QSetId { get; set; }

        public string QSetName { get; set; } = null!;

        public string Description { get; set; } = null!;
        public int LevelId { get; set; }
        public int CategoryId { get; set; }
        public DateTime CreatedTime { get; set; }

    }
}
