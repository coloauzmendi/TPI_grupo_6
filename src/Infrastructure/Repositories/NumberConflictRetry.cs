using Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    // Guarda un comprobante numerado; si otra operación tomó el mismo número entre el cálculo y el guardado, se recalcula y se reintenta una vez
    public static class NumberConflictRetry
    {
        private const int SqliteUniqueConstraintError = 2067;

        public static void SaveChanges(ApplicationContext context, string tableName, Action assignNumber)
        {
            assignNumber();

            try
            {
                context.SaveChanges();
            }
            catch (DbUpdateException ex) when (IsNumberConflict(ex, tableName))
            {
                assignNumber();
                context.SaveChanges();
            }
        }

        private static bool IsNumberConflict(DbUpdateException ex, string tableName)
        {
            return ex.InnerException is SqliteException sqliteException
                && sqliteException.SqliteExtendedErrorCode == SqliteUniqueConstraintError
                && sqliteException.Message.Contains($"{tableName}.Number");
        }
    }
}
