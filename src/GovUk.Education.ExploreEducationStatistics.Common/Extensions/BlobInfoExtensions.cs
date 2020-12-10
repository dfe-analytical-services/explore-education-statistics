using System;
using System.Globalization;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions
{
    public static class BlobInfoExtensions
    {
        public const string FilenameKey = BlobInfo.FilenameKey;
        public const string NameKey = BlobInfo.NameKey;
        public const string NumberOfRowsKey = "NumberOfRows";
        public const string UserNameKey = "userName";
        public const string ReleaseDateTimeKey = "releasedatetime";

        /**
         * Property key on a metadata file to point at the data file
         */
        public const string DataFileKey = "datafile";

        /**
         * Property key on a data file to point at the metadata file
         */
        public const string MetaFileKey = "metafile";

        public static bool IsReleased(this BlobInfo blob)
        {
            if (blob.Meta.TryGetValue(ReleaseDateTimeKey, out var releaseDateTime))
            {
                return DateTime.Compare(ParseDateTime(releaseDateTime), DateTime.Now) <= 0;
            }

            return false;
        }

        public static string GetMetaFileName(this BlobInfo blob)
        {
            return blob.Meta[MetaFileKey];
        }

        public static string GetUserName(this BlobInfo blob)
        {
            return blob.Meta.TryGetValue(UserNameKey, out var name) ? name : string.Empty;
        }

        public static int GetNumberOfRows(this BlobInfo blob)
        {
            return blob.Meta.TryGetValue(NumberOfRowsKey, out var numberOfRows) &&
                   int.TryParse(numberOfRows, out var numberOfRowsValue) ? numberOfRowsValue : 0;
        }

        private static DateTime ParseDateTime(string dateTime)
        {
            return DateTime.ParseExact(dateTime, "o", CultureInfo.InvariantCulture, DateTimeStyles.None);
        }
    }
}