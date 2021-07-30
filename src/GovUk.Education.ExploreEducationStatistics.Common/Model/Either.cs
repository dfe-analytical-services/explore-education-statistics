using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model
{
    public class Either<Tl, Tr>
    {
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
        public static async Task<Either<TFailure, TSuccess>> OnSuccessDo<TFailure, TSuccess>(
            this Task<Either<TFailure, TSuccess>> task,
            Func<Task> successTask)
        {
            return await task.OnSuccessDo(async _ => await successTask());
        }

        public static async Task<Either<TFailure, TSuccess>> OnSuccessDo<TFailure, TSuccess>(
            this Task<Either<TFailure, TSuccess>> task,
            Func<TSuccess, Task> successTask)
        {
            var firstResult = await task;

            if (firstResult.IsLeft)
            {
                return firstResult.Left;
            }

            await successTask(firstResult.Right);
            return firstResult.Right;
        }

        public static async Task<Either<TFailure, TSuccess1>> OnSuccessDo<TFailure, TSuccess1, TSuccess2>(
            this Task<Either<TFailure, TSuccess1>> task,
            Func<Task<Either<TFailure, TSuccess2>>> successTask)
        {
            return await task.OnSuccessDo(async _ => await successTask());
        }

        public static async Task<Either<TFailure, TSuccess1>> OnSuccessDo<TFailure, TSuccess1, TSuccess2>(
            this Task<Either<TFailure, TSuccess1>> task,
            Func<TSuccess1, Task<Either<TFailure, TSuccess2>>> successTask)
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
        public static async Task<Either<TFailure, Unit>> OnSuccessVoid<TFailure, TSuccess>(
            this Task<Either<TFailure, TSuccess>> task,
            Func<Task> func)
        {
            var firstResult = await task;

            if (firstResult.IsLeft)
            {
                return firstResult.Left;
            }

            await func();

            return Unit.Instance;
        }

        /**
         * Convenience method so that the chained function can be
         * void and doesn't have to explicitly return a Unit.
         */
        public static async Task<Either<TFailure, Unit>> OnSuccessVoid<TFailure, TSuccess>(
            this Task<Either<TFailure, TSuccess>> task,
            Action action)
        {
            var firstResult = await task;

            if (firstResult.IsLeft)
            {
                return firstResult.Left;
            }

            action();

            return Unit.Instance;
        }

        /**
         * Convenience method so that the success chain can be converted to a Unit without having to explicitly supply
         * it.
         */
        public static Task<Either<TFailure, Unit>> OnSuccessVoid<TFailure, TSuccess>(
            this Task<Either<TFailure, TSuccess>> task)
        {
            return OnSuccessVoid(task, () => { });
        }

        public static async Task<Either<TFailure, TSuccess2>> OnSuccess<TFailure, TSuccess1, TSuccess2>(
            this Task<Either<TFailure, TSuccess1>> task,
            Func<Task<TSuccess2>> func)
        {
            var firstResult = await task;

            if (firstResult.IsLeft)
            {
                return firstResult.Left;
            }

            return await func();
        }

        public static async Task<Either<TFailure, TSuccess2>> OnSuccess<TFailure, TSuccess1, TSuccess2>(
            this Task<Either<TFailure, TSuccess1>> task,
            Func<Task<Either<TFailure, TSuccess2>>> func)
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
        public static async Task<Either<TFailure, Unit>> OnSuccessVoid<TFailure, TSuccess>(
            this Task<Either<TFailure, TSuccess>> task,
            Func<TSuccess, Task> func)
        {
            var firstResult = await task;

            if (firstResult.IsLeft)
            {
                return firstResult.Left;
            }

            await func(firstResult.Right);

            return Unit.Instance;
        }

        public static async Task<Either<TFailure, TSuccess2>> OnSuccess<TFailure, TSuccess1, TSuccess2>(
            this Task<Either<TFailure, TSuccess1>> task,
            Func<TSuccess1, TSuccess2> func)
        {
            return await task.OnSuccess(async success => await Task.FromResult(func(success)));
        }

        /**
         * Allows 2 OnSuccess(...) calls to be chained and the next OnSuccess() to receive a Tuple containing both of
         * the results.  If either OnSuccess(...) fails, the entire result fails and additionally, if the first result
         * fails, the second OnSuccess(...) will not be called.
         *
         * When C# allows better destructuring, we will be able to destructure the resulting Tuple much better. 
         */
        public static async Task<Either<TFailure, Tuple<TSuccess1, TSuccess2>>>
            OnSuccessCombineWith<TFailure, TSuccess1, TSuccess2>(
                this Task<Either<TFailure, TSuccess1>> task,
                Func<TSuccess1, Task<Either<TFailure, TSuccess2>>> func)
        {
            return await task.OnSuccess(success =>
            {
                return func(success).OnSuccess(combinator => new Tuple<TSuccess1, TSuccess2>(success, combinator));
            });
        }

        /**
         * Allows 3 OnSuccess(...) calls to be chained and the next OnSuccess() to receive a Tuple containing all 3 of
         * the results.  If any OnSuccess(...) fails, the entire result fails and additionally, if the first result
         * fails, the subsequent OnSuccess(...) will not be called.
         *
         * When C# allows better destructuring, we will be able to destructure the resulting Tuple much better. 
         */
        public static async Task<Either<TFailure, Tuple<TSuccess1, TSuccess2, TSuccess3>>>
            OnSuccessCombineWith<TFailure, TSuccess1, TSuccess2, TSuccess3>(
                this Task<Either<TFailure, Tuple<TSuccess1, TSuccess2>>> task,
                Func<Tuple<TSuccess1, TSuccess2>, Task<Either<TFailure, TSuccess3>>> func)
        {
            return await task.OnSuccess(success =>
            {
                return func(success).OnSuccess(combinator =>
                    new Tuple<TSuccess1, TSuccess2, TSuccess3>(success.Item1, success.Item2, combinator));
            });
        }

        public static async Task<Either<TFailure, TSuccess2>> OnSuccess<TFailure, TSuccess1, TSuccess2>(
            this Task<Either<TFailure, TSuccess1>> task,
            Func<TSuccess1, Task<TSuccess2>> func)
        {
            var firstResult = await task;

            if (firstResult.IsLeft)
            {
                return firstResult.Left;
            }

            return await func(firstResult.Right);
        }

        public static async Task<Either<TFailure, TSuccess2>> OnSuccess<TFailure, TSuccess1, TSuccess2>(
            this Task<Either<TFailure, TSuccess1>> task,
            Func<TSuccess1, Either<TFailure, TSuccess2>> func)
        {
            return await task.OnSuccess(async success => await Task.FromResult(func(success)));
        }

        public static async Task<Either<TFailure, TSuccess2>> OnSuccess<TFailure, TSuccess1, TSuccess2>(
            this Task<Either<TFailure, TSuccess1>> task,
            Func<TSuccess1, Task<Either<TFailure, TSuccess2>>> func)
        {
            var firstResult = await task;

            if (firstResult.IsLeft)
            {
                return firstResult.Left;
            }

            return await func(firstResult.Right);
        }

        public static async Task<Either<TFailure1, TFaillure2>> OnFailureDo<TFailure1, TFaillure2>(
            this Task<Either<TFailure1, TFaillure2>> task,
            Func<TFailure1, Task> failureTask)
        {
            var firstResult = await task;

            if (firstResult.IsRight)
            {
                return firstResult.Right;
            }

            await failureTask(firstResult.Left);
            return firstResult.Left;
        }

        public static async Task<Either<TFailure1, TFailure2>> OnFailureFailWith<TFailure1, TFailure2>(
            this Task<Either<TFailure1, TFailure2>> task,
            Func<TFailure1> failureTask)
        {
            return await task.OnFailureFailWith(async _ => await Task.FromResult(failureTask()));
        }

        /**
         * If the previous Either failed, provide the errors to the given action and then return a new set of errors
         * provided by the action.
         */
        public static async Task<Either<TFailure1, TFailure2>> OnFailureFailWith<TFailure1, TFailure2>(
            this Task<Either<TFailure1, TFailure2>> task,
            Func<TFailure1, Task<TFailure1>> func)
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
        public static async Task<Either<TFailure, TSuccess>> OnFailureSucceedWith<TFailure, TSuccess>(
            this Task<Either<TFailure, TSuccess>> task,
            Func<TFailure, Task<TSuccess>> func)
        {
            var firstResult = await task;

            if (firstResult.IsRight)
            {
                return firstResult.Right;
            }

            return await func(firstResult.Left);
        }

        public static async Task<TSuccess> OrElse<TFailure, TSuccess>(
            this Task<Either<TFailure, TSuccess>> task,
            Func<Task<TSuccess>> func)
        {
            var firstResult = await task;

            if (firstResult.IsRight)
            {
                return firstResult.Right;
            }

            return await func();
        }

        public static async Task<TSuccess> OrElse<TFailure, TSuccess>(
            this Task<Either<TFailure, TSuccess>> task,
            Func<TSuccess> func)
        {
            var firstResult = await task;

            if (firstResult.IsRight)
            {
                return firstResult.Right;
            }

            return func();
        }

        public static async Task<Either<TFailure, List<TSuccess>>> OnSuccessAll<TFailure, TSuccess>(
            this IEnumerable<Task<Either<TFailure, TSuccess>>> tasks)
        {
            var result = new List<TSuccess>();
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
