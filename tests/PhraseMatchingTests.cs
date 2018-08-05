using NUnit.Framework;
using Puzzles;
using System;
using System.Collections.Generic;

[TestFixture]
class PhraseMatchingTests
{
    [TestCase("am I meeting someone tomorrow", 2)]
    [TestCase("do I have a meeting tomorrow", 1)]
    [TestCase("am I scheduled for a meeting tomorrow", 0)]
    public void GetMatchingPhrase_MeetingQuestion_ShouldReturnMatch(string input, int expectedResult)
    {
        var list = new List<string> { "yes, a meeting is scheduled for you tomorrow", "yes, you do have a meeting tomorrow", "yes, you are meeting someone tommorow" };
        var result = PhraseMatching.GetMatchingPhrase(input, list);

        Assert.IsTrue(list.IndexOf(result[0]) == expectedResult);
    }

    [TestCase("do I have any messages", 0)]
    [TestCase("can you check my messages for me", 1)]
    [TestCase("do I have any pending messages", 2)]
    public void GetMatchingPhrase_MessageQuestion_ShouldReturnMatch(string input, int expectedResult)
    {
        var list = new List<string> { "yes, you have three new messages", "sure, I can check your messages", "yes, you have three pending messages" };
        var result = PhraseMatching.GetMatchingPhrase(input, list);

        Assert.IsTrue(list.IndexOf(result[0]) == expectedResult);
    }

    [TestCase("when is my next shift", 0)]
    [TestCase("when am I working next", 1)]
    [TestCase("when do I have to go to work", 2)]
    public void GetMatchingPhrase_ShiftQuestion_ShouldReturnMatch(string input, int expectedResult)
    {
        var list = new List<string> { "your next shift is tomorrow at 10am", "you are working next tomorrow at 10am", "you have to go to work tomorrow at 10am" };
        var result = PhraseMatching.GetMatchingPhrase(input, list);

        Assert.IsTrue(list.IndexOf(result[0]) == expectedResult);
    }
}

