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

public static class Const
{

    public const string? Omni = default;
    public const string? AnyVerb = default;

    public const string ErrorProcessingRequest = $"{nameof(ErrorProcessingRequest)}";


    public const string Request = $"{nameof(Request)}";

    public const string Create = $"{nameof(Create)}";
    public const string Fetch = $"{nameof(Fetch)}";
    public const string Modify = $"{nameof(Modify)}";
    public const string Delete = $"{nameof(Delete)}";

    public const string RequestCreate = $"{Request}{Create}";
    public const string Created = $"{nameof(Created)}";

    public const string RequestFetch = $"{Request}{Fetch}";
    public const string Fetched = $"{nameof(Fetched)}";

    public const string RequestModify = $"{Request}{Modify}";
    public const string Modified = $"{nameof(Modified)}";

    public const string RequestDelete = $"{Request}{Delete}";
    public const string Deleted = $"{nameof(Deleted)}";




    public const string RequestPage = $"{nameof(RequestPage)}";

    public const string Respond = $"{nameof(Respond)}";


    public const string ImportantMessage = $"{nameof(ImportantMessage)}";
    public const string AuditMessage = $"{nameof(AuditMessage)}";
}