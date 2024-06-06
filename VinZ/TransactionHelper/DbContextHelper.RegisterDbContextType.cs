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

public static partial class DbContextHelper
{
    public static void RegisterDbContexts
    (
        this WebApplicationBuilder webApplicationBuilder,
        IEnumerable<Type> myDbContextTypes,
        ConfigureMyWayOptions options,
        Dictionary<Type, object> effectsMap
    )
    {
        var registerDbContextType = typeof(DbContextHelper)
            .GetMethod(nameof(RegisterDbContextTypePrivate),
                BindingFlags.NonPublic | BindingFlags.Static) ?? throw new MissingMethodException(
            nameof(DbContextHelper), nameof(RegisterDbContextTypePrivate));

        var dbContextFactory = new RegisteredDbContextFactory();

        foreach (var myDbContextType in myDbContextTypes)
        {
            if (!myDbContextType.IsAssignableTo(typeof(MyDbContext)))
            {
                throw new ArgumentException("types must be MyDbContext", nameof(myDbContextTypes));
            }

            var optionsForMyDbContextType = !effectsMap.TryGetValue(myDbContextType, out var effects) ? options 
                : options with { effects = effects };

            registerDbContextType
                .MakeGenericMethod([myDbContextType])
                .Invoke(null, [dbContextFactory, optionsForMyDbContextType]);
        }

        webApplicationBuilder.Services.AddSingleton<IDbContextFactory>(dbContextFactory);
    }

    public static void RegisterDbContextType<TContext>
    (
        this RegisteredDbContextFactory dbContextFactory,
        ConfigureMyWayOptions options
    )
        where TContext : MyDbContext, new()
    {
        dbContextFactory.RegisterDbContextTypePrivate<TContext>(options);
    }

    private static void RegisterDbContextTypePrivate<TContext>
    (
        this RegisteredDbContextFactory dbContextFactory,
        ConfigureMyWayOptions options
    )
        where TContext : MyDbContext, new()
    {
        dbContextFactory.RegisterDbContextType(() =>
        {
            return new TContext
            {
                IsDebug = options.isDebug,
                Env = options.env,
                logLevel = options.logLevel,
                // Effects = "the_effects"
                Effects = options.effects
            };
        });
    }

    private static readonly Regex contextRx = new("^(.*)Context$", RegexOptions.IgnoreCase);
    private static readonly Regex connectionStringRx = new(@"\(\$Context\)", RegexOptions.IgnoreCase);
}