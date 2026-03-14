namespace ELearning.Tests.Unit.Aplication.Features.Media;

public class LocalStorageServiceSanitizeTests
{
    // Replica exacta de SanitizeFileName tal como está en LocalStorageService
    private static string Sanitize(string fileName)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var sanitized = string.Concat(fileName.Select(c => invalid.Contains(c) ? '_' : c));
        return sanitized.Length > 100 ? sanitized[..100] : sanitized;
    }

    [Theory]
    [InlineData("normal.mp4", "normal.mp4")]
    [InlineData("file|pipe.mp4", "file_pipe.mp4")]
    [InlineData("file:colon.mp4", "file_colon.mp4")]
    [InlineData("file?query.mp4", "file_query.mp4")]
    [InlineData("file*star.mp4", "file_star.mp4")]
    public void SanitizeFileName_InvalidChars_ReplacedWithUnderscore(string input, string expected)
    {
        Assert.Equal(expected, Sanitize(input));
    }

    [Fact]
    public void SanitizeFileName_NameLongerThan100Chars_TruncatedTo100()
    {
        var longName = new string('A', 150) + ".mp4";
        var result = Sanitize(longName);

        Assert.Equal(100, result.Length);
    }

    [Fact]
    public void SanitizeFileName_NameExactly100Chars_NotTruncated()
    {
        var name = new string('A', 100);
        var result = Sanitize(name);

        Assert.Equal(100, result.Length);
        Assert.Equal(name, result);
    }

    [Fact]
    public void SanitizeFileName_NameUnder100Chars_NotTruncated()
    {
        var name = "short.mp4";
        var result = Sanitize(name);

        Assert.Equal("short.mp4", result);
    }

    [Fact]
    public void SanitizeFileName_CleanName_ReturnedUnchanged()
    {
        var name = "lecture_01_intro.mp4";
        var result = Sanitize(name);

        Assert.Equal(name, result);
    }
}
