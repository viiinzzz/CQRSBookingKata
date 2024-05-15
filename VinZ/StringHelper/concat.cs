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

public static partial class StringHelper
{
    public static TextReader Concat(this string first, TextReader? second)
    {
        if (second == default)
        {
            return new StringReader(first);
        }

        return new ConcatTextReader(new StringReader(first), second);
    }

    public static TextReader Concat(this TextReader first, TextReader? second)
    {
        if (second == default)
        {
            return first;
        }
        
        return new ConcatTextReader(first, second);
    }

    private class ConcatTextReader : TextReader
    {
        private readonly TextReader _first;
        private readonly TextReader _second;

        private bool _firstYet = true;

        public ConcatTextReader(TextReader first, TextReader second)
        {
            _first = first;
            _second = second;
        }

        public override int Peek()
        {
            if (_firstYet)
            {
                return _first.Peek();
            }

            return _second.Peek();
        }

        public override int Read()
        {
            if (!_firstYet)
            {
                return _second.Read();
            }

            var value = _first.Read();

            if (value == -1)
            {
                _firstYet = false;
                return _second.Read();
            }

            return value;
        }

        public override void Close()
        {
            _first.Close();
            _second.Close();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!disposing)
            {
                return;
            }

            _first.Dispose();
            _second.Dispose();
        }
    }
}