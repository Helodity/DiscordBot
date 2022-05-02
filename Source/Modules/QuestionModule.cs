namespace DiscordBotRewrite.Modules;
public readonly struct Question {
    public const string TruthJsonLocation = "Json/Questions/TruthQuestions.json";
    public const string ParanoiaJsonLocation = "Json/Questions/ParanoiaQuestions.json";

    [Flags]
    public enum DepthGroup {
        All = 0,
        [ChoiceName("G")]
        G = 1 << 0,
        [ChoiceName("PG")]
        PG = 1 << 1,
        [ChoiceName("PG13")]
        PG13 = 1 << 2,
        [ChoiceName("R")]
        R = 1 << 3
    }

    [JsonProperty("Text")]
    public readonly string Text;
    [JsonProperty("Groups")]
    [JsonConverter(typeof(StringEnumConverter))]
    public readonly DepthGroup Groups;

    public Question(string text, DepthGroup depth) {
        Text = text;
        Groups = depth;
    }
}
public class QuestionModule {
    public List<Question> TruthQuestions;
    public List<Question> ParanoiaQuestions;
    public List<ulong> ParanoiaInProgress = new List<ulong>();

    public QuestionModule() {
        TruthQuestions = BotUtils.LoadJson<List<Question>>(Question.TruthJsonLocation);
        ParanoiaQuestions = BotUtils.LoadJson<List<Question>>(Question.ParanoiaJsonLocation);

        if(TruthQuestions.Count == 0) {
            TruthQuestions.Add(new Question("Questions need to be added in the config!", Question.DepthGroup.All));
            BotUtils.SaveJson(TruthQuestions, Question.TruthJsonLocation);
        }
        if(ParanoiaQuestions.Count == 0) {
            ParanoiaQuestions.Add(new Question("Questions need to be added in the config!", Question.DepthGroup.All));
            BotUtils.SaveJson(ParanoiaQuestions, Question.ParanoiaJsonLocation);
        }
    }

    public Question PickQuestion(List<Question> questions, Question.DepthGroup rating) {
        List<Question> validQuestions = new List<Question>();
        foreach(Question question in questions) {
            if(question.Groups.HasFlag(rating) || question.Groups.HasFlag(Question.DepthGroup.All)) {
                validQuestions.Add(question);
            }
        }
        if(validQuestions.Count == 0)
            return new Question("No valid question!", Question.DepthGroup.All);
        return validQuestions[BotUtils.GenerateRandomNumber(0, validQuestions.Count - 1)];
    }
}
