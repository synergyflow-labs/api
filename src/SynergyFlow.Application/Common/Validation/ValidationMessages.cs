namespace SynergyFlow.Application.Common.Validation;

public static class ValidationMessages
{
    public static class Pagination
    {
        public const string PageNumberMin = "Page number must be greater than 0.";
        public const string PageSizeMin = "Page size must be greater than 0.";

        public static string PageSizeMax(int max) => $"Page size must not exceed {max}.";

        public static string SearchTermMax(int length) => $"Search term must not exceed {length} characters.";
    }
}
