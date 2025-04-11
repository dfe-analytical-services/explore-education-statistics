#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class DataSetValidator(ContentDbContext context) : IDataSetValidator
    {
        public const int MaxFilenameSize = 150;

        private readonly List<ErrorViewModel> _errors = [];

        public async Task<Either<List<ErrorViewModel>, DataSet>> ValidateDataSet(
            Guid releaseVersionId,
            string dataSetTitle,
            List<DataSetFileDto> dataSetFiles,
            File? replacingFile = null)
        {
            var validator = new DataSetFileDto.Validator();

            foreach (var file in dataSetFiles)
            {
                var result = validator.Validate(file);

                if (!result.IsValid)
                {
                    _errors.AddRange(result.Errors.Select(e => new ErrorViewModel
                    {
                        Code = e.ErrorCode,
                        Message = e.ErrorMessage,
                    }));
                }

                //file.FileStream.SeekToBeginning(); // if not using mime detective in validator, this shouldn't be needed
            }

            var isReplacement = replacingFile != null;

            ValidateDataSetTitleDuplication(releaseVersionId, dataSetTitle, isReplacement);

            if (dataSetFiles.Count != 2)
            {
                _errors.Add(ValidationMessages.GenerateErrorDataSetShouldContainTwoFiles());
            }

            var dataFile = dataSetFiles.FirstOrDefault(file => !file.FileName.EndsWith(Constants.DataSet.MetaFileExtension));
            var metaFile = dataSetFiles.FirstOrDefault(file => file.FileName.EndsWith(Constants.DataSet.MetaFileExtension));

            if (dataFile is null || metaFile is null)
            {
                _errors.Add(ValidationMessages.GenerateErrorDataSetFileNamesShouldMatchConvention());
                return _errors;
            }

            ValidateDataFileNames(releaseVersionId, dataFile.FileName, replacingFile);

            if (isReplacement)
            {
                var releaseFileWithApiDataSet = await context.ReleaseFiles
                    .SingleOrDefaultAsync(rf =>
                        rf.ReleaseVersionId == releaseVersionId
                        && rf.Name == dataSetTitle
                        && rf.PublicApiDataSetId != null);
                if (releaseFileWithApiDataSet != null)
                {
                    _errors.Add(ValidationMessages.GenerateErrorCannotReplaceDataSetWithApiDataSet(dataSetTitle));
                }
            }

            return _errors.Count > 0
                ? (Either<List<ErrorViewModel>, DataSet>)_errors
                : (Either<List<ErrorViewModel>, DataSet>)new DataSet
                {
                    Title = dataSetTitle,
                    DataFile = dataFile,
                    MetaFile = metaFile,
                };
        }

        private void ValidateDataSetTitleDuplication(
            Guid releaseVersionId,
            string title,
            bool isReplacement)
        {
            if (!isReplacement) // if it's a replacement, we get the title from the replacement which is already validated as unique
            {
                var dataSetNameExists = context.ReleaseFiles
                    .Include(rf => rf.File)
                    .Any(rf =>
                        rf.ReleaseVersionId == releaseVersionId
                        && rf.File.Type == FileType.Data
                        && rf.Name == title);

                if (dataSetNameExists)
                {
                    _errors.Add(ValidationMessages.GenerateErrorDataSetTitleShouldBeUnique(title));
                }
            }
        }

        private bool IsFileExisting(
            Guid releaseVersionId,
            FileType type,
            string filename)
        {
            return context
                .ReleaseFiles
                .Include(rf => rf.File)
                .Where(rf => rf.ReleaseVersionId == releaseVersionId && rf.File.Type == type)
                .AsEnumerable()
                .Any(rf => string.Equals(rf.File.Filename, filename, StringComparison.CurrentCultureIgnoreCase));
        }

        private void ValidateDataFileNames(
            Guid releaseVersionId,
            string dataFileName,
            File? replacingFile = null)
        {
            // - Original uploads' data filename is not unique if a ReleaseFile exists with the same filename.
            // - With replacement uploads, we can ignore a preexisting ReleaseFile if it is the file being replaced -
            // we only care if the preexisting duplicate ReleaseFile name isn't the file being replaced.
            if (IsFileExisting(releaseVersionId, FileType.Data, dataFileName) &&
                (replacingFile == null || replacingFile.Filename != dataFileName))
            {
                _errors.Add(ValidationMessages.GenerateErrorFilenameNotUnique(dataFileName, FileType.Data));
            }

            // NOTE: We allow duplicate meta file names - meta files aren't included in publicly downloadable
            // zips, so meta files won't be included in the same directory by filename and thereby cannot clash
        }
    }
}
