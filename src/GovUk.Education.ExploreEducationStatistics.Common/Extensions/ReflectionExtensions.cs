#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions
{
    /// <summary>
    /// Aka the magic box of tricks. Use with caution.
    /// </summary>
    public static class ReflectionExtensions
    {
        /// <summary>
        /// Recursively box up an unknown value until a target
        /// box type is reached e.g. an Either, Task or ActionResult.
        /// <para>
        /// This is particularly useful in situations where we cannot know
        /// the type of the value at compile-time and only have access to
        /// runtime types instead i.e. in AOP code.
        /// </para>
        /// </summary>
        public static bool TryBoxToResult(this object value, Type targetType, out object? boxedValue)
        {
            boxedValue = value;

            var path = targetType.GetUnboxedResultTypePath();
            path.Reverse();

            // Would not be able to reach the target type.
            if (!path.First().IsInstanceOfType(value))
            {
                return false;
            }

            path = path.Skip(1).ToList();

            foreach (var boxType in path)
            {
                // Stop at the first non-generic type along
                // the path. This is a non-boxable type.
                if (!boxType.IsConstructedGenericType)
                {
                    return false;
                }

                // Potentially unsafe to use just the name for this,
                // but realistically, we're never going to create
                // other types with these names.
                switch (boxType.GetNameWithoutGenericArity())
                {
                    case "ActionResult":
                        var actionResultType = typeof(ActionResult<>).MakeGenericType(boxType.GenericTypeArguments);
                        var actionResult = Activator.CreateInstance(actionResultType, boxedValue);

                        boxedValue = actionResult;
                        break;

                    case "Task":
                        var task = typeof(Task).GetMethod("FromResult")
                            ?.MakeGenericMethod(boxType.GenericTypeArguments)
                            .Invoke(null, new[] { boxedValue });

                        boxedValue = task;
                        break;

                    case "Either":
                        var eitherType = typeof(Either<,>).MakeGenericType(boxType.GenericTypeArguments);
                        var either = Activator.CreateInstance(eitherType, boxedValue);

                        boxedValue = either;
                        break;

                    default:
                        // Not a type that we handle so we
                        // can't box this any further.
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Try to unbox an unknown boxed value into its final
        /// result value by recursively unboxing each result.
        /// </summary>
        public static bool TryUnboxResult(this object? value, out object? result)
        {
            result = value;

            while (true)
            {
                if (result is null)
                {
                    result = null;
                    return true;
                }

                var resultType = result.GetType();

                // Stop at the first non-generic result type type.
                // We cannot un-box it further, so we should finish.
                if (!resultType.IsConstructedGenericType)
                {
                    return true;
                }

                if (result == Task.CompletedTask)
                {
                    result = null;
                    return false;
                }

                if (result is Task task)
                {
                    try
                    {
                        task.Wait();

                    }
                    catch (AggregateException)
                    {
                        result = null;
                        return false;
                    }

                    result = result.GetType().GetProperty("Result")?.GetValue(task);
                    continue;
                }

                // Potentially unsafe to use just the name for this,
                // but realistically, we're never going to create
                // other types with these names.
                switch (resultType.GetNameWithoutGenericArity())
                {
                    case "ActionResult":
                        result = resultType.GetProperty("Value")?.GetValue(result);
                        continue;

                    case "Either":
                        if (resultType.GetProperty("IsLeft")?.GetValue(result) is true)
                        {
                            result = null;
                            return false;
                        }

                        result = resultType.GetProperty("Right")?.GetValue(result);
                        continue;

                    default:
                        return true;
                }
            }
        }
    }
}
