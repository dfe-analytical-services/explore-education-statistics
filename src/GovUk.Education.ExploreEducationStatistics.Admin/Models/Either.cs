using System;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models
{
    public class Either<TL, TR> {
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

        public TL Left() => IsLeft ? _left : throw new ArgumentException("Calling Left on a Right");

        public TR Right() => !IsLeft ? _right : throw new ArgumentException("Calling Right on a Left");
        
        public static implicit operator Either<TL, TR>(TL left) => new Either<TL, TR>(left);
        
        public static implicit operator Either<TL, TR>(TR right) => new Either<TL, TR>(right);
    }
}