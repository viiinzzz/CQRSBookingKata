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

public class ParsableNullableDouble : IParsable<ParsableNullableDouble>
{
    public double? Value { get; init; } = null;

    public static ParsableNullableDouble Parse(string value, IFormatProvider? provider)
    {

        if (!TryParse(value, provider, out var result))
        {
            throw new ArgumentException("Could not parse supplied value.", nameof(value));
        }

        return result;
    }

    public static bool TryParse(string? value, IFormatProvider? provider, out ParsableNullableDouble nullableDouble)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            nullableDouble = new ParsableNullableDouble();

            return true;
        }

        if (double.TryParse(value, out var doubleValue))
        {
            nullableDouble = new ParsableNullableDouble() { Value = doubleValue };

            return true;
        }

        nullableDouble = new ParsableNullableDouble();

        return false;
    }
}