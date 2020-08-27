import ImporterStatus from '@admin/components/ImporterStatus';
import useFormSubmit from '@admin/hooks/useFormSubmit';
import { ImportStatusCode } from '@admin/services/importService';
import releaseDataFileService, {
  DataFile,
  DeleteDataFilePlan,
} from '@admin/services/releaseDataFileService';
import releaseMetaFileService from '@admin/services/releaseMetaFileService';
import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import { Form, FormFieldset } from '@common/components/form';
import FormFieldFileInput from '@common/components/form/FormFieldFileInput';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import LoadingSpinner from '@common/components/LoadingSpinner';
import ModalConfirm from '@common/components/ModalConfirm';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import { mapFieldErrors } from '@common/validation/serverValidations';
import Yup from '@common/validation/yup';
import { format } from 'date-fns';
import { Formik, FormikHelpers, FormikProps } from 'formik';
import remove from 'lodash/remove';
import React, { useState } from 'react';

interface FormValues {
  subjectTitle: string;
  dataFile: File | null;
  metadataFile: File | null;
}

const errorMappings = [
  mapFieldErrors<FormValues>({
    target: 'dataFile',
    messages: {
      CANNOT_OVERWRITE_DATA_FILE: 'Choose a unique data file name',
      DATA_AND_METADATA_FILES_CANNOT_HAVE_THE_SAME_NAME:
        'Choose a different file name for data and metadata files',
      DATA_FILE_CANNOT_BE_EMPTY: 'Choose a data file that is not empty',
      DATA_FILE_MUST_BE_CSV_FILE:
        'Data file must be a csv file with UTF-8 encoding',
      DATA_FILENAME_CANNOT_CONTAIN_SPACES_OR_SPECIAL_CHARACTERS:
        'Data filename cannot contain spaces or special characters',
      DATA_FILE_ALREADY_UPLOADED: 'Data file has already been uploaded',
    },
  }),
  mapFieldErrors<FormValues>({
    target: 'metadataFile',
    messages: {
      CANNOT_OVERWRITE_METADATA_FILE: 'Choose a unique metadata file name',
      METADATA_FILE_CANNOT_BE_EMPTY: 'Choose a metadata file that is not empty',
      META_FILE_MUST_BE_CSV_FILE:
        'Meta file must be a csv file with UTF-8 encoding',
      META_FILENAME_CANNOT_CONTAIN_SPACES_OR_SPECIAL_CHARACTERS:
        'Meta filename cannot contain spaces or special characters',
      META_FILE_IS_INCORRECTLY_NAMED: 'Meta filename is incorrectly named',
    },
  }),
  // mapFieldErrors<FormValues>({
  //   target: 'zipFile',
  //   messages: {
  //     DATA_FILE_MUST_BE_ZIP_FILE:
  //         'Data file must be a zip file',
  //     DATA_ZIP_FILE_CAN_ONLY_CONTAIN_TWO_FILES:
  //         'Data zip file can only contain two files',
  //     DATA_ZIP_FILE_DOES_NOT_CONTAIN_CSV_FILES:
  //         'Data zip file does not contain csv files',
  //     DATA_ZIP_FILE_ALREADY_EXISTS:
  //         'Data zip file already exists',
  //     CANNOT_OVERWRITE_DATA_FILE: 'Choose a unique data file name',
  //     DATA_AND_METADATA_FILES_CANNOT_HAVE_THE_SAME_NAME:
  //         'Choose a different file name for data and metadata files',
  //     DATA_FILE_CANNOT_BE_EMPTY: 'Choose a data file that is not empty',
  //     DATA_FILENAME_CANNOT_CONTAIN_SPACES_OR_SPECIAL_CHARACTERS:
  //         'Data filename cannot contain spaces or special characters',
  //     CANNOT_OVERWRITE_METADATA_FILE: 'Choose a unique metadata file name',
  //     METADATA_FILE_CANNOT_BE_EMPTY: 'Choose a metadata file that is not empty',
  //     META_FILENAME_CANNOT_CONTAIN_SPACES_OR_SPECIAL_CHARACTERS:
  //         'Meta filename cannot contain spaces or special characters',
  //     META_FILE_IS_INCORRECTLY_NAMED:
  //         'Meta filename is incorrectly named',
  //   },
  // }),
  mapFieldErrors<FormValues>({
    target: 'subjectTitle',
    messages: {
      SUBJECT_TITLE_MUST_BE_UNIQUE: 'Subject title must be unique',
    },
  }),
];

interface Props {
  releaseId: string;
  canUpdateRelease: boolean;
}

interface DeleteDataFile {
  plan: DeleteDataFilePlan;
  file: DataFile;
}

const formId = 'dataFileUploadForm';

const ReleaseDataUploadsSection = ({ releaseId, canUpdateRelease }: Props) => {
  const [deleteDataFile, setDeleteDataFile] = useState<DeleteDataFile>();
  const [openedAccordions, setOpenedAccordions] = useState<string[]>([]);
  const [isUploading, setIsUploading] = useState(false);

  const {
    value: dataFiles = [],
    setState: setDataFiles,
    retry: fetchDataFiles,
    isLoading,
  } = useAsyncHandledRetry(
    () => releaseDataFileService.getReleaseDataFiles(releaseId),
    [releaseId],
  );

  const resetPage = async ({ resetForm }: FormikHelpers<FormValues>) => {
    resetForm();

    document
      .querySelectorAll(`#${formId} input[type='file']`)
      .forEach(input => {
        const fileInput = input as HTMLInputElement;
        fileInput.value = '';
      });

    await fetchDataFiles();
  };

  const setDeleting = (dataFile: DeleteDataFile, deleting: boolean) => {
    setDataFiles({
      value: dataFiles.map(file =>
        file.filename !== dataFile.file.filename
          ? file
          : {
              ...file,
              isDeleting: deleting,
            },
      ),
    });
  };

  const statusChangeHandler = async (
    dataFile: DataFile,
    statusCode: ImportStatusCode,
  ) => {
    setDataFiles({
      value: dataFiles.map(file =>
        file.filename !== dataFile.filename
          ? file
          : {
              ...file,
              canDelete:
                statusCode &&
                (statusCode === 'NOT_FOUND' ||
                  statusCode === 'COMPLETE' ||
                  statusCode === 'FAILED'),
            },
      ),
    });
  };

  const handleSubmit = useFormSubmit<FormValues>(async (values, actions) => {
    setIsUploading(true);

    try {
      await releaseDataFileService.uploadDataFiles(releaseId, {
        subjectTitle: values.subjectTitle,
        dataFile: values.dataFile as File,
        metadataFile: values.metadataFile as File,
      });
      setIsUploading(false);
      await resetPage(actions);
    } finally {
      setIsUploading(false);
    }
  }, errorMappings);

  const handleDelete = async (
    dataFileToDelete: DeleteDataFile,
    form: FormikProps<FormValues>,
  ) => {
    setDeleting(dataFileToDelete, true);
    setDeleteDataFile(undefined);

    try {
      await releaseDataFileService.deleteDataFiles(
        releaseId,
        (deleteDataFile as DeleteDataFile).file,
      );
      setDeleting(dataFileToDelete, false);
      await resetPage(form);
    } finally {
      setDeleting(dataFileToDelete, false);
    }
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
      {form => (
        <Form id={formId}>
          {canUpdateRelease && (
            <>
              {isUploading && <LoadingSpinner text="Uploading files" overlay />}
              <FormFieldset
                id={`${formId}-allFieldsFieldset`}
                legend="Add new data to release"
              >
                <div className="govuk-inset-text">
                  <h2>Before you start</h2>

                  <div className="govuk-list--bullet">
                    <li>
                      make sure your data has passed the screening checks in our{' '}
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

              <ButtonGroup>
                <Button type="submit" id="upload-data-files-button">
                  Upload data files
                </Button>
                <ButtonText onClick={() => resetPage(form)}>Cancel</ButtonText>
              </ButtonGroup>
            </>
          )}

          {typeof canUpdateRelease !== 'undefined' &&
            !canUpdateRelease &&
            'This release has been approved, and can no longer be updated.'}

          <LoadingSpinner loading={isLoading}>
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
                          className="govuk-!-margin-bottom-9"
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
                          {canUpdateRelease && dataFile.canDelete && (
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
          </LoadingSpinner>

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
                              The following infographic files will also be
                              removed:
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
        </Form>
      )}
    </Formik>
  );
};

export default ReleaseDataUploadsSection;
