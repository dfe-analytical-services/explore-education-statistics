namespace GovUk.Education.ExploreEducationStatistics.Data.Processor
{
    public class ValidationError
    {
        public string Message { get; set; }

        public ValidationError(string message)
        {
            Message = message;
        }

        private bool Equals(ValidationError other)
        {
            return Message == other.Message;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ValidationError) obj);
        }

        public override int GetHashCode()
        {
            return (Message != null ? Message.GetHashCode() : 0);
        }
    }
}