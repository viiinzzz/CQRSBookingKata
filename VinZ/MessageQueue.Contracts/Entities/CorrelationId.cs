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

namespace VinZ.MessageQueue;

public record struct CorrelationId(long Id1, long Id2)// : ICorrelationId_
{
    public static CorrelationId New()
    {
        return From(System.Guid.NewGuid());
    }

    public static CorrelationId From(Guid guid)
    {
        var bytes = guid.ToByteArray();

        long id1 = 0;
        long id2 = 0;

        for (var i = 0; i < 8; i++)
        {
            id1 |= (long)bytes[i] << (8 * i);
        }

        for (var i = 8; i < 16; i++)
        {
            id2 |= (long)bytes[i] << (8 * i);
        }

        return new CorrelationId(id1, id2);
    }

    public string Guid => ToGuid().ToString("B");

    public override string ToString() => Guid;

    public Guid ToGuid()
    {
        return new Guid(
            (int)Id1, (short)(Id1 >> 32), (short)(Id1 >> 48),
            (byte)Id2, (byte)(Id2 >> 8), (byte)(Id2 >> 16), (byte)(Id2 >> 24),
            (byte)(Id2 >> 32), (byte)(Id2 >> 40), (byte)(Id2 >> 48), (byte)(Id2 >> 56)
        );
    }
}
