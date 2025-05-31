using System.ComponentModel.DataAnnotations;
using ValidationException = ConexaStarWars.API.Middleware.ValidationException;

namespace ConexaStarWars.API.Filters;

public static class ValidationFilter
{
    public static IResult ValidateModel<T>(T model) where T : class
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(model);

        var isValid = Validator.TryValidateObject(model, validationContext, validationResults, true);

        if (!isValid)
        {
            var errors = validationResults
                .GroupBy(x => x.MemberNames.FirstOrDefault() ?? "")
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.ErrorMessage).ToArray()
                );

            throw new ValidationException(errors);
        }

        return Results.Ok();
    }
}