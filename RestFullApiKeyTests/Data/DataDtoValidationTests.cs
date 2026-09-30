using RestFullApiKey.Data;
using System.ComponentModel.DataAnnotations;

namespace RestFullApiKeyTests.Data;

public class DataDtoValidationTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validate_Fail_WhenMessageIsNullOrEmpty(string? message)
    {
        var dto = new DataDto(message!);
        var ok = TryValidate(dto, out var results);

        Assert.False(ok);
        Assert.Contains(results, r => r.MemberNames.Contains("message"));
    }

    private static bool TryValidate(DataDto dto, out List<ValidationResult> results)
    {
        var context = new ValidationContext(dto);
        results = [];
        return Validator.TryValidateObject(dto, context, results, validateAllProperties: true);
    }
}
