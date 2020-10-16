import Link from '@admin/components/Link';
import DataFileDetailsTable from '@admin/pages/release/data/components/DataFileDetailsTable';
import DataFileUploadForm, {
  DataFileUploadFormValues,
} from '@admin/pages/release/data/components/DataFileUploadForm';
import { terminalImportStatuses } from '@admin/pages/release/data/components/ImporterStatus';
import {
  releaseDataFileRoute,
  ReleaseDataFileRouteParams,
} from '@admin/routes/releaseRoutes';
import releaseDataFileService, {
  DataFile,
  DataFileImportStatus,
  DeleteDataFilePlan,
} from '@admin/services/releaseDataFileService';
import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import ButtonText from '@common/components/ButtonText';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import LoadingSpinner from '@common/components/LoadingSpinner';
import ModalConfirm from '@common/components/ModalConfirm';
import WarningMessage from '@common/components/WarningMessage';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import logger from '@common/services/logger';
import { mapFieldErrors } from '@common/validation/serverValidations';
import Yup from '@common/validation/yup';
import React, { useCallback, useState } from 'react';
import { generatePath } from 'react-router';
import orderBy from 'lodash/orderBy';

interface FormValues extends DataFileUploadFormValues {
  subjectTitle: string;
}

const errorMappings = [
  mapFieldErrors<FormValues>({
    target: 'subjectTitle',
    messages: {
      SUBJECT_TITLE_MUST_BE_UNIQUE: 'Subject title must be unique',
      SUBJECT_TITLE_CANNOT_CONTAIN_SPECIAL_CHARACTERS:
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
    { status }: DataFileImportStatus,
  ) => {
    setDataFiles(
      dataFiles.map(file =>
        file.fileName !== dataFile.fileName
          ? file
          : {
              ...file,
              status,
            },
      ),
    );
  };

  const handleSubmit = useCallback(
    async (values: FormValues) => {
      let file: DataFile;

      if (values.uploadType === 'csv') {
        file = await releaseDataFileService.uploadDataFiles(releaseId, {
          name: values.subjectTitle,
          dataFile: values.dataFile as File,
          metadataFile: values.metadataFile as File,
        });
      } else {
        file = await releaseDataFileService.uploadZipDataFile(releaseId, {
          name: values.subjectTitle,
          zipFile: values.zipFile as File,
        });
      }

      setDataFiles(orderBy([...dataFiles, file], dataFile => dataFile.title));
    },
    [dataFiles, releaseId, setDataFiles],
  );

  return (
    <>
      <h2>Add data file to release</h2>
      <div className="govuk-inset-text">
        <h3>Before you start</h3>

        <ul>
          <li>
            make sure your data files have passed the checks in our{' '}
            <a href="https://rsconnect/rsc/dfe-published-data-qa/">
              screening app
            </a>
          </li>
          <li>
            if your data does not meet these standards, you won’t be able to
            upload it to your release
          </li>
          <li>
            if you have any issues uploading data and files, or questions about
            data standards contact:{' '}
            <a href="mailto:explore.statistics@education.gov.uk">
              explore.statistics@education.gov.uk
            </a>
          </li>
        </ul>
      </div>
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
              id={`${formId}-subjectTitle`}
              name="subjectTitle"
              label="Subject title"
              width={20}
            />
          }
        />
      ) : (
        <WarningMessage>
          This release has been approved, and can no longer be updated.
        </WarningMessage>
      )}

      <hr className="govuk-!-margin-top-6 govuk-!-margin-bottom-6" />

      <h2>Uploaded data files</h2>

      <LoadingSpinner loading={isLoading}>
        {dataFiles.length > 0 ? (
          <Accordion id="uploadedDataFiles">
            {dataFiles.map(dataFile => (
              <AccordionSection
                key={dataFile.title}
                heading={dataFile.title}
                headingTag="h3"
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
                              Replace data
                            </Link>
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
                  </DataFileDetailsTable>
                </div>
              </AccordionSection>
            ))}
          </Accordion>
        ) : (
          <p className="govuk-inset-text">No data files have been uploaded.</p>
        )}
      </LoadingSpinner>
      {deleteDataFile && (
        <ModalConfirm
          mounted
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
