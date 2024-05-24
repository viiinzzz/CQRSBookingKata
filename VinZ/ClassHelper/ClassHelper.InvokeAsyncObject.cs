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

using System.Reflection;

namespace VinZ.Common.Class;

public static partial class ClassHelper
{
    public static Task<TRet>? Invoke2Async<TRet>(this MethodInfo method, object instance, params object[] args)

    {
        try
        {
            //
            //
            return (Task<TRet>?)method.Invoke(instance, args);
            //
            //
        }
        catch (Exception e)
        {
            if (e.InnerException != null) e = e.InnerException;

            throw new Exception(@$"invoke method failed: {e.Message}
class {instance?.GetType()?.FullName}
{{
    {method.Signature()}
}}", e);
        }
    }

    public static Task Invoke2Async(this MethodInfo method, object instance, params object[] args)
    {
        try
        {
            //
            //
            method.Invoke(instance, args);

            return Task.CompletedTask;
            //
            //
        }
        catch (Exception e)
        {
            if (e.InnerException != null) e = e.InnerException;

            throw new Exception(@$"invoke method failed: {e.Message}
class {instance?.GetType()?.FullName}
{{
    {method.Signature()}
}}", e);
        }
    }


    public static Task<TRet>? Invoke2Async<TRet, T1>(this MethodInfo method, object instance, out T1? t1, params object[] args)
    {
        try
        {
            var parameters = method
                .GetParameters()
                .Select((parameter, index) => new
                {
                    index,
                    parameter.IsOut,
                    parameter.ParameterType,
                })
                .ToArray();

            var outIndexes = parameters
                .Where(r => r.IsOut)
                .Select(r => r.index)
                .ToArray();

            if (outIndexes.Length < 1)
            {
                throw new ArgumentException(
                    $"out parameters count ({outIndexes.Length}) is less than deconstruct count (1)");
            }

            //
            //
            var ret = method.Invoke(instance, args);
            //
            //

            var outArr = args
                .Select((value, index) => new
                {
                    index,
                    value,
                    parameters[index].IsOut
                })
                .ToArray();

            t1 = (T1?)outArr[outIndexes[0]].value;

            return (Task<TRet>?)ret;
        }
        catch (Exception e)
        {
            if (e.InnerException != null) e = e.InnerException;

            throw new Exception(@$"invoke method failed: {e.Message}
class {instance?.GetType()?.FullName}
{{
    {method.Signature()}
}}", e);
        }
    }

    public static Task Invoke2Async<T1>(this MethodInfo method, object instance, out T1? t1, params object[] args)
    {
        try
        {
            var parameters = method
                .GetParameters()
                .Select((parameter, index) => new
                {
                    index,
                    parameter.IsOut,
                    parameter.ParameterType,
                })
                .ToArray();

            var outIndexes = parameters
                .Where(r => r.IsOut)
                .Select(r => r.index)
                .ToArray();

            if (outIndexes.Length < 1)
            {
                throw new ArgumentException(
                    $"out parameters count ({outIndexes.Length}) is less than deconstruct count (1)");
            }

            //
            //
            method.Invoke(instance, args);
            //
            //

            var outArr = args
                .Select((value, index) => new
                {
                    index,
                    value,
                    parameters[index].IsOut
                })
                .ToArray();

            t1 = (T1?)outArr[outIndexes[0]].value;

            return Task.CompletedTask;
        }
        catch (Exception e)
        {
            if (e.InnerException != null) e = e.InnerException;

            throw new Exception(@$"invoke method failed: {e.Message}
class {instance?.GetType()?.FullName}
{{
    {method.Signature()}
}}", e);
        }
    }


    public static Task<TRet>? Invoke2Async<TRet, T1, T2>(this MethodInfo method, object instance, out T1? t1, out T2? t2, params object[] args)
    {
        try
        {
            var parameters = method
                .GetParameters()
                .Select((parameter, index) => new
                {
                    index,
                    parameter.IsOut,
                    parameter.ParameterType,
                })
                .ToArray();

            var outIndexes = parameters
                .Where(r => r.IsOut)
                .Select(r => r.index)
                .ToArray();

            if (outIndexes.Length < 2)
            {
                throw new ArgumentException(
                    $"out parameters count ({outIndexes.Length}) is less than deconstruct count (2)");
            }

            //
            //
            var ret = method.Invoke(instance, args);
            //
            //

            var outArr = args
                .Select((value, index) => new
                {
                    index,
                    value,
                    parameters[index].IsOut
                })
                .ToArray();

            t1 = (T1?)outArr[outIndexes[0]].value;
            t2 = (T2?)outArr[outIndexes[1]].value;

            return (Task<TRet>?)ret;
        }
        catch (Exception e)
        {
            if (e.InnerException != null) e = e.InnerException;

            throw new Exception(@$"invoke method failed: {e.Message}
class {instance?.GetType()?.FullName}
{{
    {method.Signature()}
}}", e);
        }
    }

    public static Task Invoke2Async<T1, T2>(this MethodInfo method, object instance, out T1? t1, out T2? t2, params object[] args)
    {
        try
        {
            var parameters = method
                .GetParameters()
                .Select((parameter, index) => new
                {
                    index,
                    parameter.IsOut,
                    parameter.ParameterType,
                })
                .ToArray();

            var outIndexes = parameters
                .Where(r => r.IsOut)
                .Select(r => r.index)
                .ToArray();

            if (outIndexes.Length < 2)
            {
                throw new ArgumentException(
                    $"out parameters count ({outIndexes.Length}) is less than deconstruct count (2)");
            }

            //
            //
            method.Invoke(instance, args);
            //
            //

            var outArr = args
                .Select((value, index) => new
                {
                    index,
                    value,
                    parameters[index].IsOut
                })
                .ToArray();

            t1 = (T1?)outArr[outIndexes[0]].value;
            t2 = (T2?)outArr[outIndexes[1]].value;

            return Task.CompletedTask;
        }
        catch (Exception e)
        {
            if (e.InnerException != null) e = e.InnerException;

            throw new Exception(@$"invoke method failed: {e.Message}
class {instance?.GetType()?.FullName}
{{
    {method.Signature()}
}}", e);
        }
    }


    public static Task<TRet>? Invoke2Async<TRet, T1, T2, T3>(this MethodInfo method, object instance, out T1? t1, out T2? t2, out T3? t3, params object[] args)
    {
        try
        {
            var parameters = method
                .GetParameters()
                .Select((parameter, index) => new
                {
                    index,
                    parameter.IsOut,
                    parameter.ParameterType,
                })
                .ToArray();

            var outIndexes = parameters
                .Where(r => r.IsOut)
                .Select(r => r.index)
                .ToArray();

            if (outIndexes.Length < 3)
            {
                throw new ArgumentException(
                    $"out parameters count ({outIndexes.Length}) is less than deconstruct count (3)");
            }

            //
            //
            var ret = method.Invoke(instance, args);
            //
            //

            var outArr = args
                .Select((value, index) => new
                {
                    index,
                    value,
                    parameters[index].IsOut
                })
                .ToArray();

            t1 = (T1?)outArr[outIndexes[0]].value;
            t2 = (T2?)outArr[outIndexes[1]].value;
            t3 = (T3?)outArr[outIndexes[2]].value;

            return (Task<TRet>?)ret;
        }
        catch (Exception e)
        {
            if (e.InnerException != null) e = e.InnerException;

            throw new Exception(@$"invoke method failed: {e.Message}
class {instance?.GetType()?.FullName}
{{
    {method.Signature()}
}}", e);
        }
    }

    public static Task Invoke2Async<T1, T2, T3>(this MethodInfo method, object instance, out T1? t1, out T2? t2, out T3? t3, params object[] args)
    {
        try
        {
            var parameters = method
                .GetParameters()
                .Select((parameter, index) => new
                {
                    index,
                    parameter.IsOut,
                    parameter.ParameterType,
                })
                .ToArray();

            var outIndexes = parameters
                .Where(r => r.IsOut)
                .Select(r => r.index)
                .ToArray();

            if (outIndexes.Length < 3)
            {
                throw new ArgumentException(
                    $"out parameters count ({outIndexes.Length}) is less than deconstruct count (3)");
            }

            //
            //
            method.Invoke(instance, args);
            //
            //

            var outArr = args
                .Select((value, index) => new
                {
                    index,
                    value,
                    parameters[index].IsOut
                })
                .ToArray();

            t1 = (T1?)outArr[outIndexes[0]].value;
            t2 = (T2?)outArr[outIndexes[1]].value;
            t3 = (T3?)outArr[outIndexes[2]].value;

            return Task.CompletedTask;
        }
        catch (Exception e)
        {
            if (e.InnerException != null) e = e.InnerException;

            throw new Exception(@$"invoke method failed: {e.Message}
class {instance?.GetType()?.FullName}
{{
    {method.Signature()}
}}", e);
        }
    }


    public static Task<TRet>? Invoke2Async<TRet, T1, T2, T3, T4>(this MethodInfo method, object instance, out T1? t1, out T2? t2, out T3? t3, out T4? t4, params object[] args)
    {
        try
        {
            var parameters = method
                .GetParameters()
                .Select((parameter, index) => new
                {
                    index,
                    parameter.IsOut,
                    parameter.ParameterType,
                })
                .ToArray();

            var outIndexes = parameters
                .Where(r => r.IsOut)
                .Select(r => r.index)
                .ToArray();

            if (outIndexes.Length < 4)
            {
                throw new ArgumentException(
                    $"out parameters count ({outIndexes.Length}) is less than deconstruct count (4)");
            }

            //
            //
            var ret = method.Invoke(instance, args);
            //
            //

            var outArr = args
                .Select((value, index) => new
                {
                    index,
                    value,
                    parameters[index].IsOut
                })
                .ToArray();

            t1 = (T1?)outArr[outIndexes[0]].value;
            t2 = (T2?)outArr[outIndexes[1]].value;
            t3 = (T3?)outArr[outIndexes[2]].value;
            t4 = (T4?)outArr[outIndexes[3]].value;

            return (Task<TRet>?)ret;
        }
        catch (Exception e)
        {
            if (e.InnerException != null) e = e.InnerException;

            throw new Exception(@$"invoke method failed: {e.Message}
class {instance?.GetType()?.FullName}
{{
    {method.Signature()}
}}", e);
        }
    }

    public static Task Invoke2Async<T1, T2, T3, T4>(this MethodInfo method, object instance, out T1? t1, out T2? t2, out T3? t3, out T4? t4, params object[] args)
    {
        try
        {
            var parameters = method
                .GetParameters()
                .Select((parameter, index) => new
                {
                    index,
                    parameter.IsOut,
                    parameter.ParameterType,
                })
                .ToArray();

            var outIndexes = parameters
                .Where(r => r.IsOut)
                .Select(r => r.index)
                .ToArray();

            if (outIndexes.Length < 4)
            {
                throw new ArgumentException(
                    $"out parameters count ({outIndexes.Length}) is less than deconstruct count (4)");
            }

            //
            //
            method.Invoke(instance, args);
            //
            //

            var outArr = args
                .Select((value, index) => new
                {
                    index,
                    value,
                    parameters[index].IsOut
                })
                .ToArray();

            t1 = (T1?)outArr[outIndexes[0]].value;
            t2 = (T2?)outArr[outIndexes[1]].value;
            t3 = (T3?)outArr[outIndexes[2]].value;
            t4 = (T4?)outArr[outIndexes[3]].value;

            return Task.CompletedTask;
        }
        catch (Exception e)
        {
            if (e.InnerException != null) e = e.InnerException;

            throw new Exception(@$"invoke method failed: {e.Message}
class {instance?.GetType()?.FullName}
{{
    {method.Signature()}
}}", e);
        }
    }


    public static Task<TRet>? Invoke2Async<TRet, T1, T2, T3, T4, T5>(this MethodInfo method, object instance, out T1? t1, out T2? t2, out T3? t3, out T4? t4, out T5? t5, params object[] args)
    {
        try
        {
            var parameters = method
                .GetParameters()
                .Select((parameter, index) => new
                {
                    index,
                    parameter.IsOut,
                    parameter.ParameterType,
                })
                .ToArray();

            var outIndexes = parameters
                .Where(r => r.IsOut)
                .Select(r => r.index)
                .ToArray();

            if (outIndexes.Length < 5)
            {
                throw new ArgumentException(
                    $"out parameters count ({outIndexes.Length}) is less than deconstruct count (5)");
            }

            //
            //
            var ret = method.Invoke(instance, args);
            //
            //

            var outArr = args
                .Select((value, index) => new
                {
                    index,
                    value,
                    parameters[index].IsOut
                })
                .ToArray();

            t1 = (T1?)outArr[outIndexes[0]].value;
            t2 = (T2?)outArr[outIndexes[1]].value;
            t3 = (T3?)outArr[outIndexes[2]].value;
            t4 = (T4?)outArr[outIndexes[3]].value;
            t5 = (T5?)outArr[outIndexes[4]].value;

            return (Task<TRet>?)ret;
        }
        catch (Exception e)
        {
            if (e.InnerException != null) e = e.InnerException;

            throw new Exception(@$"invoke method failed: {e.Message}
class {instance?.GetType()?.FullName}
{{
    {method.Signature()}
}}", e);
        }
    }

    public static Task Invoke2Async<T1, T2, T3, T4, T5>(this MethodInfo method, object instance, out T1? t1, out T2? t2, out T3? t3, out T4? t4, out T5? t5, params object[] args)
    {
        try
        {
            var parameters = method
                .GetParameters()
                .Select((parameter, index) => new
                {
                    index,
                    parameter.IsOut,
                    parameter.ParameterType,
                })
                .ToArray();

            var outIndexes = parameters
                .Where(r => r.IsOut)
                .Select(r => r.index)
                .ToArray();

            if (outIndexes.Length < 5)
            {
                throw new ArgumentException(
                    $"out parameters count ({outIndexes.Length}) is less than deconstruct count (5)");
            }

            //
            //
            method.Invoke(instance, args);
            //
            //

            var outArr = args
                .Select((value, index) => new
                {
                    index,
                    value,
                    parameters[index].IsOut
                })
                .ToArray();

            t1 = (T1?)outArr[outIndexes[0]].value;
            t2 = (T2?)outArr[outIndexes[1]].value;
            t3 = (T3?)outArr[outIndexes[2]].value;
            t4 = (T4?)outArr[outIndexes[3]].value;
            t5 = (T5?)outArr[outIndexes[4]].value;

            return Task.CompletedTask;
        }
        catch (Exception e)
        {
            if (e.InnerException != null) e = e.InnerException;

            throw new Exception(@$"invoke method failed: {e.Message}
class {instance?.GetType()?.FullName}
{{
    {method.Signature()}
}}", e);
        }
    }

}