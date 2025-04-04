#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static System.StringComparison;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class DataSetValidatorService(ContentDbContext context) : IDataSetValidatorService
    {
        public const int MaxFilenameSize = 150;

        public async Task<List<ErrorViewModel>> ValidateDataSet(
            Guid releaseVersionId,
            string dataSetTitle,
            List<DataSetFileDto> dataSetFiles,
            File? replacingFile = null)
        {
            var errorViewModels = new List<ErrorViewModel>();

            var validator = new DataSetFileDto.Validator();

            foreach (var file in dataSetFiles)
            {
                var result = validator.Validate(file);

                if (!result.IsValid)
                {
                    errorViewModels.AddRange(result.Errors.Select(e => new ErrorViewModel
                    {
                        Code = e.ErrorCode,
                        Message = e.ErrorMessage,
                    }));
                }

                //file.FileStream.SeekToBeginning(); // if not using mime detective in validator, this shouldn't be needed
            }

            var isReplacement = replacingFile != null;

            errorViewModels.AddRange(ValidateDataSetTitleDuplication(
                releaseVersionId,
                dataSetTitle,
                isReplacement));

            if (dataSetFiles.Count != 2)
            {
                errorViewModels.Add(ValidationMessages.GenerateErrorDataZipShouldContainTwoFiles());
            }

            var dataFile = dataSetFiles.FirstOrDefault(file => !file.FileName.EndsWith(".meta.csv"));
            var metaFile = dataSetFiles.FirstOrDefault(file => file.FileName.EndsWith(".meta.csv"));

            if (dataFile is null || metaFile is null)
            {
                return [.. errorViewModels, ValidationMessages.GenerateErrorDataSetFileNamesShouldMatchConvention()];
            }

            errorViewModels.AddRange(ValidateDataFileNames(
                releaseVersionId,
                dataFile.FileName,
                metaFile.FileName,
                replacingFile));

            if (isReplacement)
            {
                var releaseFileWithApiDataSet = await context.ReleaseFiles
                    .SingleOrDefaultAsync(rf =>
                        rf.ReleaseVersionId == releaseVersionId
                        && rf.Name == dataSetTitle
                        && rf.PublicApiDataSetId != null);
                if (releaseFileWithApiDataSet != null)
                {
                    errorViewModels.Add(ValidationMessages.GenerateErrorCannotReplaceDataSetWithApiDataSet(dataSetTitle));
                }
            }

            return errorViewModels;
        }

        private List<ErrorViewModel> ValidateDataSetTitleDuplication(
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
                    return [ValidationMessages.GenerateErrorDataSetTitleShouldBeUnique(title)];
                }
            }

            return [];
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
                .Any(rf => string.Equals(rf.File.Filename, filename, CurrentCultureIgnoreCase));
        }

        private List<ErrorViewModel> ValidateDataFileNames(
            Guid releaseVersionId,
            string dataFileName,
            string metaFileName,
            File? replacingFile = null)
        {
            List<ErrorViewModel> errors = [];

            if (string.Equals(dataFileName.ToLower(), metaFileName.ToLower(), OrdinalIgnoreCase))
            {
                errors.Add(new ErrorViewModel
                {
                    Code = ValidationMessages.DataAndMetaFilesCannotHaveSameName.Code,
                    Message = ValidationMessages.DataAndMetaFilesCannotHaveSameName.Message,
                });
            }

            // - Original uploads' data filename is not unique if a ReleaseFile exists with the same filename.
            // - With replacement uploads, we can ignore a preexisting ReleaseFile if it is the file being replaced -
            // we only care if the preexisting duplicate ReleaseFile name isn't the file being replaced.
            if (IsFileExisting(releaseVersionId, FileType.Data, dataFileName) &&
                (replacingFile == null || replacingFile.Filename != dataFileName))
            {
                errors.Add(ValidationMessages.GenerateErrorFilenameNotUnique(dataFileName, FileType.Data));
            }

            // NOTE: We allow duplicate meta file names - meta files aren't included in publicly downloadable
            // zips, so meta files won't be included in the same directory by filename and thereby cannot clash

            return errors;
        }
    }
}
