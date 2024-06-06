/*
 * Copyright (C) 2024 Vincent Fontaine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

namespace VinZ.Common;

public class DatabaseSchemaIncompatibleException(string errors) : Exception(errors);

public static partial class DbContextHelper
{
//     public static void WaitNoTransaction(this DbContext dbContext, int millisecondsTimeout = 30_000, int millisecondsWait = 100)
//     {
//         SpinWait.SpinUntil(() => 
//             { 
//                 Thread.Sleep(millisecondsWait);
//
//                 return dbContext.Database.CurrentTransaction == null;
//             },
//             millisecondsTimeout
//         );
//     }


public static bool LostInTranslation<TEntity>
    (
        this IQueryable<TEntity> query,
        out string? sql,
        out string? translationError,
        bool doThrow = false
    ) 
        where TEntity : class
    {
        try
        {
            sql = query.ToQueryString();
            translationError = null;

            return false;
        }
        catch (Exception ex)
        {
            sql = null;
            translationError = ex.Message;

            if (doThrow)
            {
                throw new Exception($"We apologize, we got lost in translation: {translationError}");
            }

            Console.Error.WriteLine($@"
!!!ERROR!!!
Lost in translation!

{translationError}
");

            return true;
        }
    }

}