using QuizWeb_TrioForce.ViewModels.BookMark;

namespace QuizWeb_TrioForce.ViewModels.BookMark
{
    public class BookMarkViewModel
    {
        public List<MarkedQuestionListViewModel> MarkedQuestionList = new();
        public List<ProgressQuestionSetListViewModel> ProgressQuestionSetList = new();
        public List<CreatedQuestionSetListViewModel> CreatedQuestionList = new();
    }
}
