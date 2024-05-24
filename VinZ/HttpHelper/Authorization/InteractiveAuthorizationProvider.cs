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
using VinZ.Common;

namespace VinZ.Common.Http;

public class InteractiveAuthorizationProvider : IHttpAuthorizationProvider
{
    private readonly string? _authorizationScheme;
    private readonly bool _interactive;
    private readonly string? _username;
    private readonly string? _password;

    private AuthenticationHeaderValue? authorization = default;

    public InteractiveAuthorizationProvider(string? authorizationScheme, bool interactive,
        string? user = default, string? password = default)
    {
        _authorizationScheme = authorizationScheme;
        _interactive = interactive;
        _username = user;
        _password = password;
    }


    const string UpAndClearStr = "\u001b[1A\u001b[2K";
    const string BoldAndBlinkStr = "\u001b[1\u001b[5";
    private const char MaskChar = '*';

    public async Task<AuthenticationHeaderValue?> GetAuthorization(CancellationToken? cancellationToken)
    {
        if (authorization != default)
        {
            return authorization;
        }

        if (_authorizationScheme == default)
        {
            return default;
        }

        if (!_interactive)
        {
            if (_username == default || _password == default)
            {
                throw new NotImplementedException("non-interactive mode requires credentials set");
            }
            
        }

        if (_authorizationScheme.ToLower() != "basic")
        {
            throw new NotImplementedException("only basic scheme allowed");
        }

        string? username, password;
        if (!_interactive)
        {
            username = _username;
            password = _password;
        }
        else
        {
            Console.Write($"{BoldAndBlinkStr}Username: ");

            username = cancellationToken.HasValue 
                ? await ConsoleAsync.ReadLine(cancellationToken.Value) 
                : Console.ReadLine();

            cancellationToken?.ThrowIfCancellationRequested();

            Console.Write($"{UpAndClearStr}{BoldAndBlinkStr}Password: ");

            password = await StringHelper.ReadPassword(MaskChar, cancellationToken);

            Console.Write(UpAndClearStr);

            cancellationToken?.ThrowIfCancellationRequested();

        }


        authorization = new AuthenticationHeaderValue(
            "Basic",
            Convert.ToBase64String(Encoding.ASCII.GetBytes(
                $"{username}:{password}"
            )));

        return authorization;
    }
}