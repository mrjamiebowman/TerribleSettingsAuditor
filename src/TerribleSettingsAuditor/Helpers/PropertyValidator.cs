using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace TerribleSettingsAuditor.Helpers;

public static class PropertyValidator
{
    public static bool IsRequired(PropertyInfo property)
    {
        if (property == null)
            throw new ArgumentNullException(nameof(property));

        return property.GetCustomAttributes(typeof(RequiredAttribute), inherit: true).Any();
    }

    public static IReadOnlyList<ValidationResult> ValidateProperty(object instance, string propertyName)
    {
        if (instance == null)
        {
            throw new ArgumentNullException(nameof(instance));
        }

        if (propertyName == null)
        {
            throw new ArgumentNullException(nameof(propertyName));
        }

        var property = instance.GetType().GetProperty(propertyName);

        if (property is null) 
        {
            throw new ArgumentException($"Property '{propertyName}' was not found on type '{instance.GetType().Name}'.");
        }

        var value = property.GetValue(instance);

        var context = new ValidationContext(instance) {
            MemberName = propertyName
        };

        var results = new List<ValidationResult>();

        Validator.TryValidateProperty(value, context, results);

        return results;
    }

    public static Dictionary<string, List<ValidationResult>> ValidateAllProperties(object instance)
    {
        if (instance == null) {
            throw new ArgumentNullException(nameof(instance));
        }

        var errors = new Dictionary<string, List<ValidationResult>>();

        var properties = instance.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead);

        foreach (var property in properties)
        {
            var value = property.GetValue(instance);

            var context = new ValidationContext(instance)
            {
                MemberName = property.Name
            };

            var results = new List<ValidationResult>();
            Validator.TryValidateProperty(value, context, results);

            if (results.Count > 0)
            {
                errors[property.Name] = results;
            }
        }

        return errors;
    }
}
