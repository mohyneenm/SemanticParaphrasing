using Puzzles_NetCore;
using System.Collections.Generic;
using Xunit;

public class PhraseMatchingTests
{
    [Theory]
    [InlineData("am I meeting someone tomorrow", 2)]
    [InlineData("do I have a meeting tomorrow", 1)]
    [InlineData("am I scheduled for a meeting tomorrow", 0)]
    public void GetMatchingPhrase_MeetingQuestion_ShouldReturnMatch(string input, int expectedResult)
    {
        var list = new List<string> { "yes, a meeting is scheduled for you tomorrow", "yes, you do have a meeting tomorrow", "yes, you are meeting someone tommorow" };
        var result = PhraseMatching.GetMatchingPhrase(input, list);

        Assert.True(list.IndexOf(result[0]) == expectedResult);
    }

    [Theory]
    [InlineData("do I have any messages", 0)]
    [InlineData("can you check my messages for me", 1)]
    [InlineData("do I have any pending messages", 2)]
    public void GetMatchingPhrase_MessageQuestion_ShouldReturnMatch(string input, int expectedResult)
    {
        var list = new List<string> { "yes, you have three new messages", "sure, I can check your messages", "yes, you have three pending messages" };
        var result = PhraseMatching.GetMatchingPhrase(input, list);

        Assert.True(list.IndexOf(result[0]) == expectedResult);
    }

    [Theory]
    [InlineData("when is my next shift", 0)]
    [InlineData("when am I working next", 1)]
    [InlineData("when do I have to go to work", 2)]
    public void GetMatchingPhrase_ShiftQuestion_ShouldReturnMatch(string input, int expectedResult)
    {
        var list = new List<string> { "your next shift is tomorrow at 10am", "you are working next tomorrow at 10am", "you have to go to work tomorrow at 10am" };
        var result = PhraseMatching.GetMatchingPhrase(input, list);

        Assert.True(list.IndexOf(result[0]) == expectedResult);
    }

    [Theory]
    [InlineData("do you have a name",                       "yes I do have a name, it's Bambi")]
    [InlineData("what's your name",                         "my name is Bambi")]
    [InlineData("what's your favorite color",               "my favorite color is Daisy white")]
    [InlineData("who is the CEO of Facebook",               "the CEO of Facebook is, hmm, I don't know that one")]
    [InlineData("what's your favorite company",             "you are my favorite company")]
    [InlineData("when is the next world cup",               "")]
    public void ProcessUserInput_SimilarSoundingWords_ShouldReturnMatchingExpectation(string userInput, string expectedValue)
    {
        // Arrange
        var phrases = new List<string> {
                "you are scheduled to work on friday",
                "yes I do have a name, it's Bambi",
                "my name is Bambi",
                "you are probably going to wrok on friday",
                "you are my favorite company",
                "the CEO of Facebook is, hmm, I don't know that one",
                "my favorite color is Daisy white"
            };

        // Act
        var matches = PhraseMatching.GetMatchingPhrase(userInput, phrases);

        // Assert
        Assert.NotNull(matches);
        Assert.True(matches[0] == expectedValue);
    }
}
