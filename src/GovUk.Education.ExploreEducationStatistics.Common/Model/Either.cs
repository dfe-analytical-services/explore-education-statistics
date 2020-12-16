using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model
{
public class Either<Tl, Tr> {
        private readonly Tl _left;
        private readonly Tr _right;

        public Either(Tl left)
        {
            _left = left;
            IsLeft = true;
        }

        public Either(Tr right)
        {
            _right = right;
            IsLeft = false;
        }

        public bool IsLeft { get; }

        public bool IsRight => !IsLeft;

        public Tl Left => IsLeft ? _left : throw new ArgumentException("Calling Left on a Right");

        public Tr Right => !IsLeft ? _right : throw new ArgumentException("Calling Right on a Left");

        public Either<Tl, T> Map<T>(Func<Tr, T> func) =>
            IsLeft ? new Either<Tl, T>(Left) : new Either<Tl, T>(func.Invoke(Right));

        public Either<Tl, T> Map<T>(Func<Tr, Either<Tl, T>> func) =>
            IsLeft ? new Either<Tl, T>(Left) : func.Invoke(Right);

        public Either<Tl, T> OnSuccess<T>(Func<Tr, T> func) => Map(func);

        public Either<Tl, T> OnSuccess<T>(Func<T> func) => Map(_ => func.Invoke());

        public Either<Tl, Tr> OrElse(Func<Tr> func) => IsLeft ? func() : Right;

        public T Fold<T>(Func<Tl, T> leftFunc, Func<Tr, T> rightFunc) => IsRight ? rightFunc(Right) : leftFunc(Left);

        public T FoldLeft<T>(Func<Tl, T> leftFunc, T defaultValue) => IsLeft ? leftFunc(Left) : defaultValue;

        public T FoldRight<T>(Func<Tr, T> rightFunc, T defaultValue) => IsRight ? rightFunc(Right) : defaultValue;

        public static implicit operator Either<Tl, Tr>(Tl left) => new Either<Tl, Tr>(left);

        public static implicit operator Either<Tl, Tr>(Tr right) => new Either<Tl, Tr>(right);
    }

    public static class EitherTaskExtensions
    {
        public static async Task<Either<Tl, Tr>> OnSuccessDo<Tl, Tr>(this Task<Either<Tl, Tr>> task, Func<Task> successTask)
        {
            return await task.OnSuccessDo(async _ => await successTask());
        }

        public static async Task<Either<Tl, Tr>> OnSuccessDo<Tl, Tr>(this Task<Either<Tl, Tr>> task, Func<Tr, Task> successTask)
        {
            var firstResult = await task;

            if (firstResult.IsLeft)
            {
                return firstResult.Left;
            }

            await successTask(firstResult.Right);
            return firstResult.Right;
        }

        public static async Task<Either<Tl, Tr>> OnSuccessDo<Tl, Tr, T>(this Task<Either<Tl, Tr>> task, Func<Task<Either<Tl, T>>> successTask)
        {
            return await task.OnSuccessDo(async _ => await successTask());
        }

        public static async Task<Either<Tl, Tr>> OnSuccessDo<Tl, Tr, T>(this Task<Either<Tl, Tr>> task, Func<Tr, Task<Either<Tl, T>>> successTask)
        {
            var firstResult = await task;

            if (firstResult.IsLeft)
            {
                return firstResult.Left;
            }

            var result = await successTask(firstResult.Right);

            if (result.IsRight)
            {
                return firstResult.Right;
            }

            return result.Left;
        }

        /**
         * Convenience method so that the chained function can be
         * void and doesn't have to explicitly return a Unit.
         */
        public static async Task<Either<Tl, Unit>> OnSuccessVoid<Tl, Tr>(this Task<Either<Tl, Tr>> task, Func<Task> func)
        {
            var firstResult = await task;

            if (firstResult.IsLeft)
            {
                return firstResult.Left;
            }

            await func();

            return Unit.Instance;
        }

        public static async Task<Either<Tl, T>> OnSuccess<Tl, Tr, T>(this Task<Either<Tl, Tr>> task, Func<Task<T>> func)
        {
            var firstResult = await task;

            if (firstResult.IsLeft)
            {
                return firstResult.Left;
            }

            return await func();
        }

        public static async Task<Either<Tl, T>> OnSuccess<Tl, Tr, T>(this Task<Either<Tl, Tr>> task, Func<Task<Either<Tl, T>>> func)
        {
            var firstResult = await task;

            if (firstResult.IsLeft)
            {
                return firstResult.Left;
            }

            return await func();
        }

        /**
         * Convenience method so that the chained function can be
         * void and doesn't have to explicitly return a Unit.
         */
        public static async Task<Either<Tl, Unit>> OnSuccessVoid<Tl, Tr>(this Task<Either<Tl, Tr>> task, Func<Tr, Task> func)
        {
            var firstResult = await task;

            if (firstResult.IsLeft)
            {
                return firstResult.Left;
            }

            await func(firstResult.Right);

            return Unit.Instance;
        }

        public static async Task<Either<Tl, T>> OnSuccess<Tl, Tr, T>(this Task<Either<Tl, Tr>> task, Func<Tr, T> func)
        {
            return await task.OnSuccess(async success => await Task.FromResult(func(success)));
        }

        public static async Task<Either<Tl, T>> OnSuccess<Tl, Tr, T>(this Task<Either<Tl, Tr>> task, Func<Tr, Task<T>> func)
        {
            var firstResult = await task;

            if (firstResult.IsLeft)
            {
                return firstResult.Left;
            }

            return await func(firstResult.Right);
        }

        public static async Task<Either<Tl, T>> OnSuccess<Tl, Tr, T>(this Task<Either<Tl, Tr>> task, Func<Tr, Either<Tl, T>> func)
        {
            return await task.OnSuccess(async success => await Task.FromResult(func(success)));
        }

        public static async Task<Either<Tl, T>> OnSuccess<Tl, Tr, T>(this Task<Either<Tl, Tr>> task, Func<Tr, Task<Either<Tl, T>>> func)
        {
            var firstResult = await task;

            if (firstResult.IsLeft)
            {
                return firstResult.Left;
            }

            return await func(firstResult.Right);
        }

        public static async Task<Either<Tl, Tr>> OnFailureDo<Tl, Tr>(this Task<Either<Tl, Tr>> task, Func<Tl, Task> failureTask)
        {
            var firstResult = await task;

            if (firstResult.IsRight)
            {
                return firstResult.Right;
            }

            await failureTask(firstResult.Left);
            return firstResult.Left;
        }

        public static async Task<Either<Tl, Tr>> OnFailureFailWith<Tl, Tr>(this Task<Either<Tl, Tr>> task, Func<Tl> failureTask)
        {
            return await task.OnFailureFailWith(async _ => await Task.FromResult(failureTask()));
        }

        /**
         * If the previous Either failed, provide the errors to the given action and then return a new set of errors
         * provided by the action.
         */
        public static async Task<Either<Tl, Tr>> OnFailureFailWith<Tl, Tr>(this Task<Either<Tl, Tr>> task, Func<Tl, Task<Tl>> func)
        {
            var firstResult = await task;

            if (firstResult.IsRight)
            {
                return firstResult.Right;
            }

            return await func(firstResult.Left);
        }

        /**
         * If the previous Either failed, perform the given action and then handle it as a success case anyway.
         * This allows a prior step to fail but overall be treated as a success (unless a subsequent step happens to
         * fail after this one).
        */
        public static async Task<Either<Tl, Tr>> OnFailureSucceedWith<Tl, Tr>(this Task<Either<Tl, Tr>> task, Func<Tl, Task<Tr>> func)
        {
            var firstResult = await task;

            if (firstResult.IsRight)
            {
                return firstResult.Right;
            }

            return await func(firstResult.Left);
        }

        public static async Task<Tr> OrElse<Tl, Tr>(this Task<Either<Tl, Tr>> task, Func<Task<Tr>> func)
        {
            var firstResult = await task;

            if (firstResult.IsRight)
            {
                return firstResult.Right;
            }

            return await func();
        }

        public static async Task<Tr> OrElse<Tl, Tr>(this Task<Either<Tl, Tr>> task, Func<Tr> func)
        {
            var firstResult = await task;

            if (firstResult.IsRight)
            {
                return firstResult.Right;
            }

            return func();
        }

        public static async Task<Either<Tl, List<Tr>>> OnSuccessAll<Tl, Tr>(this IEnumerable<Task<Either<Tl, Tr>>> tasks)
        {
            var result = new List<Tr>();
            foreach (var task in tasks)
            {
                var r = await task;
                if (r.IsLeft)
                {
                    return r.Left;
                }
                result.Add(r.Right);
            }
            return result;
        }
    }
}
