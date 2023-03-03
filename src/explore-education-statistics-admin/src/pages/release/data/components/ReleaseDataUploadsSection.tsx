import Link from '@admin/components/Link';
import DataFileDetailsTable from '@admin/pages/release/data/components/DataFileDetailsTable';
import DataFileUploadForm, {
  DataFileUploadFormValues,
} from '@admin/pages/release/data/components/DataFileUploadForm';
import { terminalImportStatuses } from '@admin/pages/release/data/components/ImporterStatus';
import {
  releaseDataFileReplaceRoute,
  ReleaseDataFileReplaceRouteParams,
  releaseDataFileRoute,
  ReleaseDataFileRouteParams,
} from '@admin/routes/releaseRoutes';
import permissionService from '@admin/services/permissionService';
import releaseDataFileService, {
  DataFile,
  DataFileImportStatus,
  DeleteDataFilePlan,
} from '@admin/services/releaseDataFileService';
import ButtonText from '@common/components/ButtonText';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import InsetText from '@common/components/InsetText';
import LoadingSpinner from '@common/components/LoadingSpinner';
import ModalConfirm from '@common/components/ModalConfirm';
import WarningMessage from '@common/components/WarningMessage';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import logger from '@common/services/logger';
import { mapFieldErrors } from '@common/validation/serverValidations';
import Yup from '@common/validation/yup';
import DataUploadCancelButton from '@admin/pages/release/data/components/DataUploadCancelButton';
import React, { useCallback, useState } from 'react';
import { generatePath } from 'react-router';
import ReorderableAccordion from '@admin/components/editable/ReorderableAccordion';
import ReorderableAccordionSection from '@admin/components/editable/ReorderableAccordionSection';

interface FormValues extends DataFileUploadFormValues {
  subjectTitle: string;
}

const errorMappings = [
  mapFieldErrors<FormValues>({
    target: 'subjectTitle',
    messages: {
      SubjectTitleMustBeUnique: 'Subject title must be unique',
      SubjectTitleCannotContainSpecialCharacters:
        'Subject title cannot contain special characters',
    },
  }),
];

interface Props {
  publicationId: string;
  releaseId: string;
  canUpdateRelease: boolean;
  onDataFilesChange?: (dataFiles: DataFile[]) => void;
}

interface DeleteDataFile {
  plan: DeleteDataFilePlan;
  file: DataFile;
}

const formId = 'dataFileUploadForm';

const ReleaseDataUploadsSection = ({
  publicationId,
  releaseId,
  canUpdateRelease,
  onDataFilesChange,
}: Props) => {
  const [deleteDataFile, setDeleteDataFile] = useState<DeleteDataFile>();
  const [activeFileId, setActiveFileId] = useState<string>();
  const {
    value: dataFiles = [],
    setState: setDataFilesState,
    isLoading,
  } = useAsyncHandledRetry(
    () => releaseDataFileService.getDataFiles(releaseId),
    [releaseId],
  );

  const setFileDeleting = (dataFile: DeleteDataFile, deleting: boolean) => {
    setDataFiles(
      dataFiles.map(file =>
        file.fileName !== dataFile.file.fileName
          ? file
          : {
              ...file,
              isDeleting: deleting,
            },
      ),
    );
  };

  const setDataFiles = useCallback(
    (nextDataFiles: DataFile[]) => {
      setDataFilesState({ value: nextDataFiles });

      if (onDataFilesChange) {
        onDataFilesChange(nextDataFiles);
      }
    },
    [onDataFilesChange, setDataFilesState],
  );

  const handleStatusChange = async (
    dataFile: DataFile,
    { totalRows, status }: DataFileImportStatus,
  ) => {
    const permissions = await permissionService.getDataFilePermissions(
      releaseId,
      dataFile.id,
    );

    setActiveFileId('');
    setDataFiles(
      dataFiles.map(file =>
        file.fileName !== dataFile.fileName
          ? file
          : {
              ...dataFile,
              rows: totalRows,
              status,
              permissions,
            },
      ),
    );
  };

  const handleSubmit = useCallback(
    async (values: FormValues) => {
      let file: DataFile;

      if (values.uploadType === 'csv') {
        file = await releaseDataFileService.uploadDataFiles(releaseId, {
          title: values.subjectTitle.trim(),
          dataFile: values.dataFile as File,
          metadataFile: values.metadataFile as File,
        });
      } else {
        file = await releaseDataFileService.uploadZipDataFile(releaseId, {
          title: values.subjectTitle.trim(),
          zipFile: values.zipFile as File,
        });
      }
      setActiveFileId(file.id);
      setDataFiles([...dataFiles, file]);
    },
    [dataFiles, releaseId, setDataFiles],
  );

  return (
    <>
      <h2>Add data file to release</h2>
      <InsetText>
        <h3>Before you start</h3>
        <p>
          Data files will be displayed in the table tool and can be used to
          create data blocks. They will also be attached to the release for
          users to download. Please ensure:
        </p>
        <ul>
          <li>
            your data files have passed the checks in our{' '}
            <a href="https://rsconnect/rsc/dfe-published-data-qa/">
              screening app
            </a>
          </li>
          <li>
            your data files meets these standards - if not you won’t be able to
            upload it to your release
          </li>
          <li>
            if you have any issues uploading data files, or questions about data
            standards contact:{' '}
            <a href="mailto:explore.statistics@education.gov.uk">
              explore.statistics@education.gov.uk
            </a>
          </li>
        </ul>
      </InsetText>
      {canUpdateRelease ? (
        <DataFileUploadForm
          id={formId}
          initialValues={{
            subjectTitle: '',
            uploadType: 'csv',
            dataFile: null,
            metadataFile: null,
            zipFile: null,
          }}
          errorMappings={errorMappings}
          onSubmit={handleSubmit}
          validationSchema={baseSchema =>
            baseSchema.shape({
              subjectTitle: Yup.string()
                .trim()
                .required('Enter a subject title')
                .test({
                  name: 'unique',
                  message: 'Enter a unique subject title',
                  test(value: string) {
                    if (!value) {
                      return true;
                    }

                    return (
                      dataFiles.find(
                        f => f.title.toUpperCase() === value.toUpperCase(),
                      ) === undefined
                    );
                  },
                }),
            })
          }
          beforeFields={
            <FormFieldTextInput<FormValues>
              name="subjectTitle"
              label="Subject title"
              className="govuk-!-width-two-thirds"
            />
          }
        />
      ) : (
        <WarningMessage>
          This release has been approved, and can no longer be updated.
        </WarningMessage>
      )}

      <hr className="govuk-!-margin-top-6 govuk-!-margin-bottom-6" />

      <LoadingSpinner loading={isLoading}>
        {dataFiles.length > 0 ? (
          <ReorderableAccordion
            canReorder={canUpdateRelease}
            id="uploadedDataFiles"
            heading="Uploaded data files"
            reorderHiddenText="files"
            onReorder={async (fileIds: string[]) => {
              setDataFiles(
                await releaseDataFileService.updateDataFilesOrder(
                  releaseId,
                  fileIds,
                ),
              );
            }}
          >
            {dataFiles.map(dataFile => (
              <ReorderableAccordionSection
                id={dataFile.id}
                key={dataFile.title}
                heading={dataFile.title}
                headingTag="h3"
                open={dataFile.id === activeFileId}
              >
                <div style={{ position: 'relative' }}>
                  {dataFile.isDeleting && (
                    <LoadingSpinner text="Deleting files" overlay />
                  )}
                  <DataFileDetailsTable
                    dataFile={dataFile}
                    releaseId={releaseId}
                    onStatusChange={handleStatusChange}
                  >
                    {canUpdateRelease &&
                      terminalImportStatuses.includes(dataFile.status) && (
                        <>
                          {dataFile.status === 'COMPLETE' && (
                            <>
                              <Link
                                className="govuk-!-margin-right-4"
                                to={generatePath<ReleaseDataFileRouteParams>(
                                  releaseDataFileRoute.path,
                                  {
                                    publicationId,
                                    releaseId,
                                    fileId: dataFile.id,
                                  },
                                )}
                              >
                                Edit title
                              </Link>
                              <Link
                                className="govuk-!-margin-right-4"
                                to={generatePath<
                                  ReleaseDataFileReplaceRouteParams
                                >(releaseDataFileReplaceRoute.path, {
                                  publicationId,
                                  releaseId,
                                  fileId: dataFile.id,
                                })}
                              >
                                Replace data
                              </Link>
                            </>
                          )}

                          <ButtonText
                            onClick={() =>
                              releaseDataFileService
                                .getDeleteDataFilePlan(releaseId, dataFile)
                                .then(plan => {
                                  setDeleteDataFile({
                                    plan,
                                    file: dataFile,
                                  });
                                })
                            }
                          >
                            Delete files
                          </ButtonText>
                        </>
                      )}
                    {dataFile.permissions.canCancelImport && (
                      <DataUploadCancelButton
                        releaseId={releaseId}
                        fileId={dataFile.id}
                      />
                    )}
                  </DataFileDetailsTable>
                </div>
              </ReorderableAccordionSection>
            ))}
          </ReorderableAccordion>
        ) : (
          <InsetText>No data files have been uploaded.</InsetText>
        )}
      </LoadingSpinner>
      {deleteDataFile && (
        <ModalConfirm
          open
          title="Confirm deletion of selected data files"
          onExit={() => setDeleteDataFile(undefined)}
          onCancel={() => setDeleteDataFile(undefined)}
          onConfirm={async () => {
            const { file } = deleteDataFile;

            setDeleteDataFile(undefined);
            setFileDeleting(deleteDataFile, true);

            try {
              await releaseDataFileService.deleteDataFiles(releaseId, file.id);

              setDataFiles(dataFiles.filter(dataFile => dataFile !== file));
            } catch (err) {
              logger.error(err);
              setFileDeleting(deleteDataFile, false);
            }
          }}
        >
          <p>Are you sure you want to delete {deleteDataFile.file.title}?</p>
          <p>This data will no longer be available for use in this release.</p>

          {deleteDataFile.plan.deleteDataBlockPlan.dependentDataBlocks.length >
            0 && (
            <>
              <p>The following data blocks will also be deleted:</p>

              <ul>
                {deleteDataFile.plan.deleteDataBlockPlan.dependentDataBlocks.map(
                  block => (
                    <li key={block.name}>
                      <p>{block.name}</p>
                      {block.contentSectionHeading && (
                        <p>
                          {`It will also be removed from the "${block.contentSectionHeading}" content section.`}
                        </p>
                      )}
                      {block.infographicFilesInfo.length > 0 && (
                        <p>
                          The following infographic files will also be removed:
                          <ul>
                            {block.infographicFilesInfo.map(fileInfo => (
                              <li key={fileInfo.filename}>
                                <p>{fileInfo.filename}</p>
                              </li>
                            ))}
                          </ul>
                        </p>
                      )}
                      {block.isKeyStatistic && (
                        <p>
                          A key statistic associated with this data block will
                          also be removed.
                        </p>
                      )}
                    </li>
                  ),
                )}
              </ul>
            </>
          )}
          {deleteDataFile.plan.footnoteIds.length > 0 && (
            <p>
              {`${deleteDataFile.plan.footnoteIds.length} ${
                deleteDataFile.plan.footnoteIds.length > 1
                  ? 'footnotes'
                  : 'footnote'
              } will be removed or updated.`}
            </p>
          )}
        </ModalConfirm>
      )}
    </>
  );
};

export default ReleaseDataUploadsSection;
