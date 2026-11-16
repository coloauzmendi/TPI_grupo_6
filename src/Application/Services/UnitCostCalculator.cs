using Application.Models;
using Domain.Exceptions;

namespace Application.Services
{
    // Costo unitario de una carga de mercadería: viene indicado o se calcula a partir del costo total
    public static class UnitCostCalculator
    {
        public static decimal Resolve(decimal? unitCost, decimal? totalCost, int quantity)
        {
            if (unitCost.HasValue == totalCost.HasValue)
            {
                throw new AppValidationException("Indicá el costo por unidad o el costo total (uno de los dos).");
            }

            if (unitCost.HasValue)
            {
                if (unitCost.Value < ValidationLimits.MinAmount || unitCost.Value > ValidationLimits.MaxAmount)
                {
                    throw new AppValidationException("El costo unitario debe estar entre 0,01 y 99.999.999,99.");
                }

                return unitCost.Value;
            }

            if (totalCost!.Value < ValidationLimits.MinAmount || totalCost.Value > ValidationLimits.MaxTotalAmount)
            {
                throw new AppValidationException("El costo total debe estar entre 0,01 y 9.999.999.999,99.");
            }

            if (quantity <= 0)
            {
                throw new AppValidationException("La cantidad debe ser mayor a 0.");
            }

            decimal calculated = Math.Round(totalCost.Value / quantity, 2, MidpointRounding.AwayFromZero);
            if (calculated < ValidationLimits.MinAmount || calculated > ValidationLimits.MaxAmount)
            {
                throw new AppValidationException("El costo unitario resultante debe estar entre 0,01 y 99.999.999,99.");
            }

            return calculated;
        }
    }
}
