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

public class ParsableHexLong : IParsable<ParsableHexLong>
{
    public long Value { get; init; } = 0;

    public static ParsableHexLong Parse(string value, IFormatProvider? provider)
    {
        if (!TryParse(value, provider, out var result))
        {
            throw new ArgumentException("Could not parse supplied value.", nameof(value));
        }

        return result;
    }

    public static bool TryParse(string? value, IFormatProvider? provider, out ParsableHexLong hexLong)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            hexLong = new ParsableHexLong();

            return true;
        }

        try
        {
            hexLong = new ParsableHexLong()
            {
                Value = long.Parse(value.Replace("-", ""), System.Globalization.NumberStyles.HexNumber)
            };

            return true;
        }
        catch (Exception)
        {
            hexLong = new ParsableHexLong();

            return false;
        }
    }
}