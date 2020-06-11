import ImporterStatus from '@admin/components/ImporterStatus';
import useFormSubmit from '@admin/hooks/useFormSubmit';
import { ImportStatusCode } from '@admin/services/importService';
import permissionService from '@admin/services/permissionService';
import releaseDataFileService, {
  DataFile,
  DeleteDataFilePlan,
} from '@admin/services/releaseDataFileService';
import releaseMetaFileService from '@admin/services/releaseMetaFileService';
import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import Button from '@common/components/Button';
import ButtonText from '@common/components/ButtonText';
import { Form, FormFieldset } from '@common/components/form';
import FormFieldFileInput from '@common/components/form/FormFieldFileInput';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import { errorCodeToFieldError } from '@common/components/form/util/serverValidationHandler';
import LoadingSpinner from '@common/components/LoadingSpinner';
import ModalConfirm from '@common/components/ModalConfirm';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import Yup from '@common/validation/yup';
import { format } from 'date-fns';
import { Formik, FormikHelpers, FormikProps } from 'formik';
import remove from 'lodash/remove';
import React, { useEffect, useState } from 'react';

const errorCodeMappings = [
  errorCodeToFieldError(
    'CANNOT_OVERWRITE_DATA_FILE',
    'dataFile',
    'Choose a unique data file name',
  ),
  errorCodeToFieldError(
    'CANNOT_OVERWRITE_METADATA_FILE',
    'metadataFile',
    'Choose a unique metadata file name',
  ),
  errorCodeToFieldError(
    'DATA_AND_METADATA_FILES_CANNOT_HAVE_THE_SAME_NAME',
    'dataFile',
    'Choose a different file name for data and metadata files',
  ),
  errorCodeToFieldError(
    'DATA_FILE_CANNOT_BE_EMPTY',
    'dataFile',
    'Choose a data file that is not empty',
  ),
  errorCodeToFieldError(
    'METADATA_FILE_CANNOT_BE_EMPTY',
    'metadataFile',
    'Choose a metadata file that is not empty',
  ),
  errorCodeToFieldError(
    'DATA_FILE_MUST_BE_CSV_FILE',
    'dataFile',
    'Data file must be a csv file with UTF-8 encoding',
  ),
  errorCodeToFieldError(
    'META_FILE_MUST_BE_CSV_FILE',
    'metadataFile',
    'Meta file must be a csv file with UTF-8 encoding',
  ),
  errorCodeToFieldError(
    'SUBJECT_TITLE_MUST_BE_UNIQUE',
    'subjectTitle',
    'Subject title must be unique',
  ),
  errorCodeToFieldError(
    'DATA_FILENAME_CANNOT_CONTAIN_SPACES_OR_SPECIAL_CHARACTERS',
    'dataFile',
    'Data filename cannot contain spaces or special characters',
  ),
  errorCodeToFieldError(
    'META_FILENAME_CANNOT_CONTAIN_SPACES_OR_SPECIAL_CHARACTERS',
    'metadataFile',
    'Meta filename cannot contain spaces or special characters',
  ),
];

interface FormValues {
  subjectTitle: string;
  dataFile: File | null;
  metadataFile: File | null;
}

interface Props {
  publicationId: string;
  releaseId: string;
}

interface DeleteDataFile {
  plan: DeleteDataFilePlan;
  file: DataFile;
}

const formId = 'dataFileUploadForm';

const ReleaseDataUploadsSection = ({ publicationId, releaseId }: Props) => {
  const [dataFiles, setDataFiles] = useState<DataFile[]>([]);
  const [deleteDataFile, setDeleteDataFile] = useState<DeleteDataFile>();
  const [canUpdateRelease, setCanUpdateRelease] = useState<boolean>();
  // BAU-324 - temporary stopgap until Release Versioning phase 2 is tackled, to prevent data files being changed on a
  // Release amendment
  const [canUpdateReleaseDataFiles, setCanUpdateReleaseDataFiles] = useState<
    boolean
  >();
  const [openedAccordions, setOpenedAccordions] = useState<string[]>([]);
  const [isUploading, setIsUploading] = useState(false);

  useEffect(() => {
    Promise.all([
      releaseDataFileService.getReleaseDataFiles(releaseId),
      permissionService.canUpdateRelease(releaseId),
      permissionService.canUpdateReleaseDataFiles(releaseId),
    ]).then(
      ([
        releaseDataFiles,
        canUpdateReleaseResponse,
        canUpdateReleaseDataFilesResponse,
      ]) => {
        setDataFiles(releaseDataFiles);
        setCanUpdateRelease(canUpdateReleaseResponse);
        setCanUpdateReleaseDataFiles(canUpdateReleaseDataFilesResponse);
      },
    );
  }, [publicationId, releaseId]);

  const resetPage = async ({ resetForm }: FormikHelpers<FormValues>) => {
    resetForm();

    document
      .querySelectorAll(`#${formId} input[type='file']`)
      .forEach(input => {
        const fileInput = input as HTMLInputElement;
        fileInput.value = '';
      });

    const files = await releaseDataFileService.getReleaseDataFiles(releaseId);
    setDataFiles(files);
  };

  const setDeleting = (dataFile: DeleteDataFile, deleting: boolean) => {
    setDataFiles(
      dataFiles.map(file =>
        file.filename !== dataFile.file.filename
          ? file
          : {
              ...file,
              isDeleting: deleting,
            },
      ),
    );
  };

  const statusChangeHandler = async (
    dataFile: DataFile,
    importstatusCode: ImportStatusCode,
  ) => {
    setDataFiles(
      dataFiles.map(file =>
        file.filename !== dataFile.filename
          ? file
          : {
              ...file,
              canDelete:
                importstatusCode &&
                (importstatusCode === 'NOT_FOUND' ||
                  importstatusCode === 'COMPLETE' ||
                  importstatusCode === 'FAILED'),
            },
      ),
    );
  };

  const handleSubmit = useFormSubmit<FormValues>(async (values, actions) => {
    setIsUploading(true);
    await releaseDataFileService
      .uploadDataFiles(releaseId, {
        subjectTitle: values.subjectTitle,
        dataFile: values.dataFile as File,
        metadataFile: values.metadataFile as File,
      })
      .then(() => {
        setIsUploading(false);
        resetPage(actions);
      })
      .finally(() => {
        setIsUploading(false);
      });
  }, errorCodeMappings);

  const handleDelete = async (
    dataFileToDelete: DeleteDataFile,
    form: FormikProps<FormValues>,
  ) => {
    setDeleting(dataFileToDelete, true);
    setDeleteDataFile(undefined);
    await releaseDataFileService
      .deleteDataFiles(releaseId, (deleteDataFile as DeleteDataFile).file)
      .then(() => {
        setDeleting(dataFileToDelete, false);
        resetPage(form);
      })
      .finally(() => {
        setDeleting(dataFileToDelete, false);
      });
  };

  return (
    <Formik<FormValues>
      enableReinitialize
      initialValues={{
        subjectTitle: '',
        dataFile: null,
        metadataFile: null,
      }}
      onSubmit={handleSubmit}
      validationSchema={Yup.object<FormValues>({
        subjectTitle: Yup.string()
          .required('Enter a subject title')
          .test('unique', 'Subject title must be unique', function unique(
            value: string,
          ) {
            if (!value) {
              return true;
            }
            return (
              dataFiles.find(
                f => f.title.toUpperCase() === value.toUpperCase(),
              ) === undefined
            );
          }),
        dataFile: Yup.mixed().required('Choose a data file'),
        metadataFile: Yup.mixed().required('Choose a metadata file'),
      })}
    >
      {form => {
        return (
          <Form id={formId}>
            {canUpdateRelease && canUpdateReleaseDataFiles && (
              <>
                {isUploading && (
                  <LoadingSpinner text="Uploading files" overlay />
                )}
                <FormFieldset
                  id={`${formId}-allFieldsFieldset`}
                  legend="Add new data to release"
                >
                  <div className="govuk-inset-text">
                    <h2 className="govuk-heading-m">Before you start</h2>
                    <div className="govuk-list--bullet">
                      <li>
                        make sure your data has passed the screening checks in
                        our{' '}
                        <a href="https://github.com/dfe-analytical-services/ees-data-screener">
                          R Project
                        </a>{' '}
                      </li>
                      <li>
                        if your data doesn’t meet these standards, you won’t be
                        able to upload it to your release
                      </li>
                      <li>
                        if you have any issues uploading data and files, or
                        questions about data standards contact:{' '}
                        <a href="mailto:explore.statistics@education.gov.uk">
                          explore.statistics@education.gov.uk
                        </a>
                      </li>
                    </div>
                  </div>

                  <FormFieldTextInput<FormValues>
                    id={`${formId}-subjectTitle`}
                    name="subjectTitle"
                    label="Subject title"
                    width={20}
                  />

                  <FormFieldFileInput<FormValues>
                    id={`${formId}-dataFile`}
                    name="dataFile"
                    label="Upload data file"
                    formGroupClass="govuk-!-margin-top-6"
                  />

                  <FormFieldFileInput<FormValues>
                    id={`${formId}-metadataFile`}
                    name="metadataFile"
                    label="Upload metadata file"
                  />
                </FormFieldset>

                <Button
                  type="submit"
                  id="upload-data-files-button"
                  className="govuk-button govuk-!-margin-right-6"
                >
                  Upload data files
                </Button>
                <ButtonText
                  className="govuk-button govuk-button--secondary"
                  onClick={() => resetPage(form)}
                >
                  Cancel
                </ButtonText>
              </>
            )}

            {typeof canUpdateRelease !== 'undefined' &&
              !canUpdateRelease &&
              !canUpdateReleaseDataFiles &&
              'This release has been approved, and can no longer be updated.'}

            {typeof canUpdateRelease !== 'undefined' &&
              canUpdateRelease &&
              !canUpdateReleaseDataFiles &&
              'This release is an amendment to a live release and so it is not possible to change any data files.'}

            {dataFiles.length > 0 && (
              <>
                <hr />
                <h2 className="govuk-heading-m">Uploaded data files</h2>
                <Accordion
                  id="uploaded-files"
                  onToggleAll={openAll => {
                    if (openAll) {
                      setOpenedAccordions(
                        dataFiles.map((dataFile, index) => {
                          return `${dataFile.title}-${index}`;
                        }),
                      );
                    } else {
                      setOpenedAccordions([]);
                    }
                  }}
                >
                  {dataFiles.map((dataFile, index) => {
                    const accId = `${dataFile.title}-${index}`;
                    return (
                      <AccordionSection
                        key={accId}
                        heading={dataFile.title}
                        onToggle={() => {
                          if (openedAccordions.includes(accId)) {
                            setOpenedAccordions(
                              remove(openedAccordions, (item: string) => {
                                return item !== accId;
                              }),
                            );
                          } else {
                            setOpenedAccordions([...openedAccordions, accId]);
                          }
                        }}
                        open={openedAccordions.includes(accId)}
                      >
                        {dataFile.isDeleting && (
                          <LoadingSpinner text="Deleting files" overlay />
                        )}
                        <SummaryList
                          key={dataFile.filename}
                          additionalClassName="govuk-!-margin-bottom-9"
                        >
                          <SummaryListItem term="Subject title">
                            <h4 className="govuk-heading-m">
                              {dataFile.title}
                            </h4>
                          </SummaryListItem>
                          <SummaryListItem term="Data file">
                            <ButtonText
                              onClick={() =>
                                releaseDataFileService.downloadDataFile(
                                  releaseId,
                                  dataFile.filename,
                                )
                              }
                            >
                              {dataFile.filename}
                            </ButtonText>
                          </SummaryListItem>
                          <SummaryListItem term="Metadata file">
                            <ButtonText
                              onClick={() =>
                                releaseMetaFileService.downloadDataMetadataFile(
                                  releaseId,
                                  dataFile.metadataFilename,
                                )
                              }
                            >
                              {dataFile.metadataFilename}
                            </ButtonText>
                          </SummaryListItem>
                          <SummaryListItem term="Data file size">
                            {dataFile.fileSize.size.toLocaleString()}{' '}
                            {dataFile.fileSize.unit}
                          </SummaryListItem>
                          <SummaryListItem term="Number of rows">
                            {dataFile.rows.toLocaleString()}
                          </SummaryListItem>

                          <ImporterStatus
                            releaseId={releaseId}
                            dataFile={dataFile}
                            onStatusChangeHandler={statusChangeHandler}
                          />
                          <SummaryListItem term="Uploaded by">
                            <a href={`mailto:${dataFile.userName}`}>
                              {dataFile.userName}
                            </a>
                          </SummaryListItem>
                          <SummaryListItem term="Date uploaded">
                            {format(dataFile.created, 'd/M/yyyy HH:mm')}
                          </SummaryListItem>
                          {canUpdateRelease &&
                            canUpdateReleaseDataFiles &&
                            dataFile.canDelete && (
                              <SummaryListItem
                                term="Actions"
                                actions={
                                  <ButtonText
                                    onClick={() =>
                                      releaseDataFileService
                                        .getDeleteDataFilePlan(
                                          releaseId,
                                          dataFile,
                                        )
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
                                }
                              />
                            )}
                        </SummaryList>
                      </AccordionSection>
                    );
                  })}
                </Accordion>
              </>
            )}

            {deleteDataFile && (
              <ModalConfirm
                mounted
                title="Confirm deletion of selected data files"
                onExit={() => setDeleteDataFile(undefined)}
                onCancel={() => setDeleteDataFile(undefined)}
                onConfirm={() => handleDelete(deleteDataFile, form)}
              >
                <p>
                  This data will no longer be available for use in this release.
                </p>
                {deleteDataFile.plan.deleteDataBlockPlan.dependentDataBlocks
                  .length > 0 && (
                  <p>
                    The following data blocks will also be deleted:
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
                            {block.infographicFilenames.length > 0 && (
                              <p>
                                The following infographic files will also be
                                removed:
                                <ul>
                                  {block.infographicFilenames.map(filename => (
                                    <li key={filename}>
                                      <p>{filename}</p>
                                    </li>
                                  ))}
                                </ul>
                              </p>
                            )}
                          </li>
                        ),
                      )}
                    </ul>
                  </p>
                )}
                {deleteDataFile.plan.footnoteIds.length > 0 && (
                  <p>
                    {deleteDataFile.plan.footnoteIds.length}{' '}
                    {deleteDataFile.plan.footnoteIds.length > 1
                      ? 'footnotes'
                      : 'footnote'}{' '}
                    will be removed.
                  </p>
                )}
              </ModalConfirm>
            )}
          </Form>
        );
      }}
    </Formik>
  );
};

export default ReleaseDataUploadsSection;
