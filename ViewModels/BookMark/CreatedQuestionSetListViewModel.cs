namespace QuizWeb_TrioForce.ViewModels.BookMark
{
    public class CreatedQuestionSetListViewModel
    {
        public int QSetId { get; set; }

        public string QSetName { get; set; } = null!;

        public string Description { get; set; } = null!;
        public string LevelName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public DateTime CreatedTime { get; set; }

    }
}
