namespace TerribleSettingsAuditor.Helpers;

internal static class MaskingHelper
{
    public static string MaskMiddleWithLimits(
        string value,
        int? showLeft = null,
        int? showRight = null,
        char maskChar = '*',
        int? maxLength = null,
        int minLength = 100)
    {
        if (string.IsNullOrEmpty(value))
        {
            return new string(maskChar, Math.Max(1, minLength));
        }

        int requestedLeft = Math.Max(0, showLeft ?? 0);
        int requestedRight = Math.Max(0, showRight ?? 0);
        int minimumLength = Math.Max(1, minLength);

        // If maxLength is provided and is less than minLength, maxLength wins.
        int effectiveMaxLength = maxLength.HasValue && maxLength.Value > 0
            ? Math.Max(1, maxLength.Value)
            : int.MaxValue;

        int targetLength = effectiveMaxLength == int.MaxValue
            ? minimumLength
            : Math.Min(minimumLength, effectiveMaxLength);

        // We must always leave at least one masked character.
        int maxVisibleFromValue = Math.Max(0, value.Length - 1);

        // Visible character budget if maxLength is enforced.
        int visibleBudget = effectiveMaxLength == int.MaxValue
            ? requestedLeft + requestedRight
            : Math.Max(0, effectiveMaxLength - 1);

        // Never allow enough visible chars to expose the entire value.
        visibleBudget = Math.Min(visibleBudget, maxVisibleFromValue);

        int left = Math.Min(requestedLeft, value.Length);
        int right = Math.Min(requestedRight, value.Length);

        // Shrink left/right until they fit within visible budget.
        while (left + right > visibleBudget)
        {
            if (left >= right && left > 0)
            {
                left--;
            }
            else if (right > 0)
            {
                right--;
            }
            else
            {
                break;
            }
        }

        // Final safety so we never reveal the full value.
        if (left + right >= value.Length)
        {
            int allowedVisible = Math.Max(0, value.Length - 1);

            if (left > allowedVisible)
            {
                left = allowedVisible;
                right = 0;
            }
            else
            {
                right = Math.Min(right, allowedVisible - left);
            }
        }

        string leftPart = left > 0
            ? value.Substring(0, left)
            : string.Empty;

        string rightPart = right > 0
            ? value.Substring(value.Length - right, right)
            : string.Empty;

        // Start with one mask character minimum.
        int maskCount = 1;

        // Expand mask section so final string is at least targetLength.
        int currentLength = leftPart.Length + maskCount + rightPart.Length;
        if (currentLength < targetLength)
        {
            maskCount += targetLength - currentLength;
        }

        // If maxLength exists, ensure final output does not exceed it.
        if (effectiveMaxLength != int.MaxValue)
        {
            int finalLength = leftPart.Length + maskCount + rightPart.Length;
            if (finalLength > effectiveMaxLength)
            {
                maskCount = Math.Max(1, effectiveMaxLength - leftPart.Length - rightPart.Length);
            }
        }

        string maskedPart = new string(maskChar, maskCount);

        var fullString = leftPart + maskedPart + rightPart;
        return fullString;
    }

    public static string MaskMiddle(string? value, int? showLeft = null, int? showRight = null, char maskChar = '*')
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        int left = Math.Max(0, showLeft ?? 0);
        int right = Math.Max(0, showRight ?? 0);

        // If nothing is requested, mask the whole thing
        if (left == 0 && right == 0)
        {
            return new string(maskChar, value.Length);
        }

        // If the visible counts would expose the whole value, just return it
        if (left + right >= value.Length)
        {
            return value;
        }

        string leftPart = left > 0 ? value.Substring(0, left) : string.Empty;
        string rightPart = right > 0 ? value.Substring(value.Length - right, right) : string.Empty;

        int maskedLength = value.Length - left - right;
        string maskedPart = new string(maskChar, maskedLength);

        return $"{leftPart}{maskedPart}{rightPart}";
    }
}
