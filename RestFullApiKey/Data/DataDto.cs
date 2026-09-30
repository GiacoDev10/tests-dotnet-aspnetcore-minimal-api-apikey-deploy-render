using System.ComponentModel.DataAnnotations;

namespace RestFullApiKey.Data;

public record DataDto([property: Required] string message);
