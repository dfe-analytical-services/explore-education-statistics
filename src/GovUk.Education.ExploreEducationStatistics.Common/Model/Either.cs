using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

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
        
        public static implicit operator Either<Tl, Tr>(Tl left) => new Either<Tl, Tr>(left);
        
        public static implicit operator Either<Tl, Tr>(Tr right) => new Either<Tl, Tr>(right);

    }

    public static class EitherTaskExtensions 
    {
        public static async Task<Either<Tl, Tr>> OnSuccessDo<Tl, Tr>(this Task<Either<Tl, Tr>> task, Action func)
        {
            var firstResult = await task;
            if (firstResult.IsLeft)
            {
                return firstResult.Left;
            }

            func();
            return firstResult.Right;
        }
        
        public static async Task<Either<Tl, T>> OnSuccess<Tl, Tr, T>(this Task<Either<Tl, Tr>> task, Func<Task<T>> func)
        {
            var firstResult = await task;
            if (firstResult.IsLeft)
            {
                return firstResult.Left;
            }

            var next = await func();
            return next;
        }
        
        public static async Task<Either<Tl, T>> OnSuccess<Tl, Tr, T>(this Task<Either<Tl, Tr>> task, Func<Task<Either<Tl, T>>> func)
        {
            var firstResult = await task;
            if (firstResult.IsLeft)
            {
                return firstResult.Left;
            }

            return await func.Invoke();
        }
        
        public static async Task<Either<Tl, T>> OnSuccess<Tl, Tr, T>(this Task<Either<Tl, Tr>> task, Func<Tr, Task<T>> func)
        {
            var firstResult = await task;
            if (firstResult.IsLeft)
            {
                return firstResult.Left;
            }

            var next = await func(firstResult.Right);
            return next;
        }
        
        public static async Task<Either<Tl, T>> OnSuccess<Tl, Tr, T>(this Task<Either<Tl, Tr>> task, Func<Tr, T> func)
        {
            var firstResult = await task;
            if (firstResult.IsLeft)
            {
                return firstResult.Left;
            }

            var next = func(firstResult.Right);
            return next;
        }
        
        public static async Task<Either<Tl, T>> OnSuccess<Tl, Tr, T>(this Task<Either<Tl, Tr>> task, Func<Tr, Either<Tl, T>> func)
        {
            var firstResult = await task;
            if (firstResult.IsLeft)
            {
                return firstResult.Left;
            }

            var next = func(firstResult.Right);
            return next;
        }
        
        public static async Task<Either<Tl, T>> OnSuccess<Tl, Tr, T>(this Task<Either<Tl, Tr>> task, Func<Tr, Task<Either<Tl, T>>> func)
        {
            var firstResult = await task;
            if (firstResult.IsLeft)
            {
                return firstResult.Left;
            }

            var next = await func(firstResult.Right);
            return next;
        }
        
        public static async Task<Either<T, Tr>> OnFailure<Tl, Tr, T>(this Task<Either<Tl, Tr>> task, Func<Tl, Task<T>> func)
        {
            var firstResult = await task;
            if (firstResult.IsRight)
            {
                return firstResult.Right;
            }

            var next = await func(firstResult.Left);
            return next;
        }
        
        public static async Task<T> OnFailure<Tl, Tr, T>(this Task<Either<Tl, Tr>> task, Func<Tl, T> func) where Tr : T
        {
            var firstResult = await task;
            if (firstResult.IsRight)
            {
                return firstResult.Right;
            }

            return func(firstResult.Left);
        }
    }
}