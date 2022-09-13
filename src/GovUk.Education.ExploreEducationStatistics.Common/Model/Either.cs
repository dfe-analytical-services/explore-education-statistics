using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model
{
    public class Either<TL, TR>
    {
        private readonly TL _left;
        private readonly TR _right;

        public Either(TL left)
        {
            _left = left;
            IsLeft = true;
        }

        public Either(TR right)
        {
            _right = right;
            IsLeft = false;
        }

        public bool IsLeft { get; }

        public bool IsRight => !IsLeft;

        public TL Left => IsLeft ? _left : throw new ArgumentException("Calling Left on a Right");

        public TR Right => !IsLeft ? _right : throw new ArgumentException("Calling Right on a Left");

        private Either<TL, T> Map<T>(Func<TR, T> func) =>
            IsLeft ? new Either<TL, T>(Left) : new Either<TL, T>(func.Invoke(Right));

        public Either<TL, T> OnSuccess<T>(Func<TR, T> func) => Map(func);

        public async Task<Either<TL, T>> OnSuccess<T>(Func<TR, Task<Either<TL, T>>> func)
        {
            if (IsLeft)
            {
                return _left;
            }

            return await func.Invoke(_right);
        }

        public Either<TL, T> OnSuccess<T>(Func<T> func) => Map(_ => func.Invoke());

        public Either<TL, T> OnSuccess<T>(Func<TR, Either<TL, T>> func) => IsLeft ? Left : func.Invoke(Right);

        public Either<TL, TR> OrElse(Func<TR> func) => IsLeft ? func() : Right;

        public Either<TL, TR> OrElse(Func<TL, TR> func) => IsLeft ? func(Left) : Right;

        public T Fold<T>(Func<TL, T> leftFunc, Func<TR, T> rightFunc) => IsRight ? rightFunc(Right) : leftFunc(Left);

        public T FoldLeft<T>(Func<TL, T> leftFunc, T defaultValue) => IsLeft ? leftFunc(Left) : defaultValue;

        public T FoldRight<T>(Func<TR, T> rightFunc, T defaultValue) => IsRight ? rightFunc(Right) : defaultValue;

        public static implicit operator Either<TL, TR>(TL left) => new(left);

        public static implicit operator Either<TL, TR>(TR right) => new(right);
    }

    public static class EitherExtensions
    {
        public static Either<TFailure, List<TSuccess>> OnSuccessAll<TFailure, TSuccess>(
            this IEnumerable<Either<TFailure, TSuccess>> items)
        {
            var result = new List<TSuccess>();
            foreach (var either in items)
            {
                if (either.IsLeft)
                {
                    return either.Left;
                }

                result.Add(either.Right);
            }
            return result;
        }

        public static Either<TFailure, Unit> OnSuccessVoid<TFailure, TSuccess>(
            this Either<TFailure, TSuccess> either)
        {
            if (either.IsLeft)
            {
                return either.Left;
            }

            return Unit.Instance;
        }
    }

    public static class EitherTaskExtensions
    {
        public static async Task<bool> IsLeft<TFailure, TSuccess>(this Task<Either<TFailure, TSuccess>> task)
        {
            return (await task).IsLeft;
        }

        public static async Task<bool> IsRight<TFailure, TSuccess>(this Task<Either<TFailure, TSuccess>> task)
        {
            return (await task).IsRight;
        }

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
            Func<Either<TFailure, TSuccess2>> successTask)
        {
            return await task.OnSuccessDo(async _ => await Task.FromResult(successTask()));
        }

        public static async Task<Either<TFailure, TSuccess1>> OnSuccessDo<TFailure, TSuccess1, TSuccess2>(
            this Task<Either<TFailure, TSuccess1>> task,
            Func<TSuccess1, Either<TFailure, TSuccess2>> successTask)
        {
            return await task.OnSuccessDo(async result => await Task.FromResult(successTask(result)));
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

        [Obsolete("Use OnSuccessDo or OnSuccessVoid for chaining a non-generic Task")]
        public static async Task<Either<TFailure, Unit>> OnSuccess<TFailure, TSuccess1>(
            this Task<Either<TFailure, TSuccess1>> task,
            Func<TSuccess1, Task> func)
        {
            return await task.OnSuccessVoid(func);
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

        /**
         * Convenience method so that the chained function can be
         * void and doesn't have to explicitly return a Unit.
         */
        public static Task<Either<TFailure, Unit>> OnSuccessVoid<TFailure, TSuccess1, TSuccess2>(
            this Task<Either<TFailure, TSuccess1>> task,
            Func<TSuccess1, Task<Either<TFailure, TSuccess2>>> task2)
        {
            return task
                .OnSuccess(task2.Invoke)
                .OnSuccessVoid();
        }

        /**
         * Convenience method so that the chained function can be
         * void and doesn't have to explicitly return a Unit.
         */
        public static Task<Either<TFailure, Unit>> OnSuccessVoid<TFailure, TSuccess1, TSuccess2>(
            this Task<Either<TFailure, TSuccess1>> task,
            Func<Task<Either<TFailure, TSuccess2>>> func)
        {
            return task
                .OnSuccess(func.Invoke)
                .OnSuccessVoid();
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
                return func(success).OnSuccess(combinator => TupleOf(success, combinator));
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
                Func<TSuccess1, Either<TFailure, TSuccess2>> func)
        {
            return await task.OnSuccess(success =>
            {
                return func(success).OnSuccess(combinator => TupleOf(success, combinator));
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

        public static async Task<Either<Unit, TSuccess>> OnFailureVoid<TFailure, TSuccess>(
            this Task<Either<TFailure, TSuccess>> task,
            Action<TFailure> failureTask)
        {
            var firstResult = await task;

            if (firstResult.IsRight)
            {
                return firstResult.Right;
            }

            failureTask(firstResult.Left);
            return Unit.Instance;
        }

        public static async Task<Either<TFailure1, TFailure2>> OnFailureDo<TFailure1, TFailure2>(
            this Task<Either<TFailure1, TFailure2>> task,
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
            Func<TFailure1> failure)
        {
            return await task.OnFailureFailWith(async _ => await Task.FromResult(failure()));
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

        public static async Task<Either<TFailure, TSuccess>> OrElse<TFailure, TSuccess>(
            this Task<Either<TFailure, TSuccess>> task,
            Func<Task<Either<TFailure, TSuccess>>> func)
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
