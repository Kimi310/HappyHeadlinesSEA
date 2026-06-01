using System.Data;
using Moq;
using ProfanityService.DataAccess.Interfaces;
using ProfanityService.DataAccess.Models;
using Xunit;
using ProfanityFilterService = ProfanityService.Service.ProfanityService;

namespace ProfanityService.Tests;

public class ProfanityServiceTests
{
    private static ProfanityFilterService CreateSut(params string[] profaneWords)
    {
        var repo = new Mock<IProfanityRepository>();
        repo.Setup(r => r.GetProfaneWords())
            .ReturnsAsync(profaneWords
                .Select(w => new Profanity { Id = Guid.NewGuid(), Word = w })
                .ToList());

        return new ProfanityFilterService(repo.Object);
    }

    [Fact]
    public async Task FilterProfanityAsync_ReplacesProfaneWord_WithAsterisks()
    {
        var sut = CreateSut("badword");

        var result = await sut.FilterProfanityAsync("this is a badword here");

        Assert.Equal("this is a ******* here", result);
    }

    [Fact]
    public async Task FilterProfanityAsync_IsCaseInsensitive()
    {
        var sut = CreateSut("badword");

        var result = await sut.FilterProfanityAsync("This is BADWORD");

        Assert.Equal("This is *******", result);
    }

    [Fact]
    public async Task FilterProfanityAsync_OnlyMatchesWholeWords()
    {
        var sut = CreateSut("ass");

        // "class" should NOT be censored because of the word boundary
        var result = await sut.FilterProfanityAsync("a great class today");

        Assert.Equal("a great class today", result);
    }

    [Fact]
    public async Task FilterProfanityAsync_UsesCustomReplacementChar()
    {
        var sut = CreateSut("bad");

        var result = await sut.FilterProfanityAsync("so bad", replacementChar: '#');

        Assert.Equal("so ###", result);
    }

    [Fact]
    public async Task FilterProfanityAsync_LeavesCleanTextUnchanged()
    {
        var sut = CreateSut("badword");

        var result = await sut.FilterProfanityAsync("perfectly clean sentence");

        Assert.Equal("perfectly clean sentence", result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task FilterProfanityAsync_ThrowsOnNullOrWhitespace(string input)
    {
        var sut = CreateSut("badword");

        await Assert.ThrowsAsync<NoNullAllowedException>(
            () => sut.FilterProfanityAsync(input));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ContainsProfanityAsync_ReturnsFalse_ForEmptyOrWhitespace(string input)
    {
        var sut = CreateSut("badword");

        var result = await sut.ContainsProfanityAsync(input);

        Assert.False(result);
    }
}

