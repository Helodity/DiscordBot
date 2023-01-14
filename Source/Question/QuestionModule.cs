using DiscordBotRewrite.Extensions;

namespace DiscordBotRewrite.Question
{
    public class QuestionModule
    {
        public List<Question> TruthQuestions;
        public List<Question> ParanoiaQuestions;
        public List<ulong> ParanoiaInProgress = new List<ulong>();

        public QuestionModule()
        {
            TruthQuestions = LoadJson<List<Question>>(Question.TruthJsonLocation);
            ParanoiaQuestions = LoadJson<List<Question>>(Question.ParanoiaJsonLocation);

            if (TruthQuestions.Count == 0)
            {
                TruthQuestions.Add(new Question("Questions need to be added in the config!", QuestionRating.All));
                SaveJson(TruthQuestions, Question.TruthJsonLocation);
            }
            if (ParanoiaQuestions.Count == 0)
            {
                ParanoiaQuestions.Add(new Question("Questions need to be added in the config!", QuestionRating.All));
                SaveJson(ParanoiaQuestions, Question.ParanoiaJsonLocation);
            }
        }

        public Question PickQuestion(List<Question> questions, QuestionRating rating)
        {
            List<Question> validQuestions = new List<Question>();
            foreach (Question question in questions)
            {
                if (question.Groups.HasFlag(rating) || question.Groups.HasFlag(QuestionRating.All))
                {
                    validQuestions.Add(question);
                }
            }
            if (validQuestions.Count == 0)
                return new Question("No valid question!", QuestionRating.All);
            return validQuestions[GenerateRandomNumber(0, validQuestions.Count - 1)];
        }
    }
}