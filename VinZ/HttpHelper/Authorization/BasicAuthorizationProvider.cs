/*
 * HttpHelper
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

using System.Net.Http.Headers;
using System.Text;

namespace VinZ.Common.Http;

public class BasicAuthorizationProvider : IHttpAuthorizationProvider
{

    private readonly AuthenticationHeaderValue? _authorization;

    public BasicAuthorizationProvider(string username, string password)
    {
        _authorization =
            username == default || password == default
                ? default
                : new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(Encoding.ASCII.GetBytes(
                        $"{username}:{password}"
                    )));
    }

    public async Task<AuthenticationHeaderValue?> GetAuthorization(CancellationToken? cancellationToken) => _authorization;
}