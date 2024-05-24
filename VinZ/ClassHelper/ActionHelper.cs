/*
 * ClassHelper
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

namespace VinZ.Common.Class; 

public abstract class ActionHelper
{
    protected abstract Delegate CreateActionImpl(Action<object> action);

    private class ActionHelperP<T> : ActionHelper
    {
        protected override Delegate CreateActionImpl(Action<object> action)
        {
            //return new Action<T>(obj => Console.WriteLine("Called = " + (object)obj));
            Action<T> action2 =  o => action(o);

            return action2;
        }
    }

    public static Delegate CreateAction(Type type, Action<object> action)
    {
        var helperType = typeof(ActionHelperP<>).MakeGenericType(type);
        var helper = (ActionHelper)Activator.CreateInstance(helperType);

        return helper.CreateActionImpl(action);
    }
}