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

public static  class TimeServiceConst
{
    public const string Time = nameof(Time);

    public static class Verb
    {
            public const string Set = $"{Time}:{nameof(Set)}";
            public const string Freeze = $"{Time}:{nameof(Freeze)}";
            public const string Unfreeze = $"{Time}:{nameof(Unfreeze)}";
            public const string Reset = $"{Time}:{nameof(Reset)}";
            public const string Forward = $"{Time}:{nameof(Forward)}";
    }
}