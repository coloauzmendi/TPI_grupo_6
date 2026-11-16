namespace Application.Models
{
    // Límites compartidos por las validaciones de los Request y de los servicios
    public static class ValidationLimits
    {
        public const int MaxQuantity = 100_000;

        public const string MinAmountText = "0.01";
        public const string MaxAmountText = "99999999.99";

        public const decimal MinAmount = 0.01m;
        public const decimal MaxAmount = 99_999_999.99m;

        public const string MaxTotalAmountText = "9999999999.99";
        public const decimal MaxTotalAmount = 9_999_999_999.99m;
    }
}
