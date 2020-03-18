import ImporterStatus from '@admin/components/ImporterStatus';
import useFormSubmit from '@admin/hooks/useFormSubmit';
import permissionService from '@admin/services/permissions/permissionService';
import editReleaseDataService, {
  DataFile,
  DeleteDataFilePlan,
} from '@admin/services/release/edit-release/data/editReleaseDataService';
import { ImportStatusCode } from '@admin/services/release/imports/types';
import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import Button from '@common/components/Button';
import ButtonText from '@common/components/ButtonText';
import { Form, FormFieldset, Formik } from '@common/components/form';
import FormFieldFileSelector from '@common/components/form/FormFieldFileSelector';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import { errorCodeToFieldError } from '@common/components/form/util/serverValidationHandler';
import ModalConfirm from '@common/components/ModalConfirm';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import Yup from '@common/validation/yup';
import { format } from 'date-fns';
import { FormikActions, FormikProps } from 'formik';
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
    'Data file must be a csv file',
  ),
  errorCodeToFieldError(
    'META_FILE_MUST_BE_CSV_FILE',
    'metadataFile',
    'Meta file must be a csv file',
  ),
  errorCodeToFieldError(
    'SUBJECT_TITLE_MUST_BE_UNIQUE',
    'subjectTitle',
    'Subject title must be unique',
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

  useEffect(() => {
    Promise.all([
      editReleaseDataService.getReleaseDataFiles(releaseId),
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

  const resetPage = async <T extends {}>({ resetForm }: FormikActions<T>) => {
    resetForm();

    document
      .querySelectorAll(`#${formId} input[type='file']`)
      .forEach(input => {
        const fileInput = input as HTMLInputElement;
        fileInput.value = '';
      });

    const files = await editReleaseDataService.getReleaseDataFiles(releaseId);
    setDataFiles(files);
  };

  const statusChangeHandler = async (
    dataFile: DataFile,
    importstatusCode: ImportStatusCode,
  ) => {
    const updatedDataFiles = [...dataFiles];
    const updatedFile = updatedDataFiles.find(
      file => file.filename === dataFile.filename,
    );

    if (!updatedFile) {
      return;
    }
    updatedFile.canDelete =
      importstatusCode &&
      (importstatusCode === 'COMPLETE' || importstatusCode === 'FAILED');
    setDataFiles(updatedDataFiles);
  };

  const handleSubmit = useFormSubmit<FormValues>(async (values, actions) => {
    await editReleaseDataService.uploadDataFiles(releaseId, {
      subjectTitle: values.subjectTitle,
      dataFile: values.dataFile as File,
      metadataFile: values.metadataFile as File,
    });

    await resetPage(actions);
  }, errorCodeMappings);

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
      render={(form: FormikProps<FormValues>) => {
        return (
          <Form id={formId}>
            {canUpdateRelease && (
              <>
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

                  <FormFieldFileSelector<FormValues>
                    id={`${formId}-dataFile`}
                    name="dataFile"
                    label="Upload data file"
                    formGroupClass="govuk-!-margin-top-6"
                    form={form}
                  />

                  <FormFieldFileSelector<FormValues>
                    id={`${formId}-metadataFile`}
                    name="metadataFile"
                    label="Upload metadata file"
                    form={form}
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
              'This release is an amendment to a live release and so cannot change any data files.'}

            {dataFiles.length > 0 && (
              <>
                <hr />
                <h2 className="govuk-heading-m">Uploaded data files</h2>
                <Accordion id="uploaded-files">
                  {dataFiles.map((dataFile, index) => {
                    const accId = `${dataFile.title}-${index}`;
                    return (
                      <AccordionSection
                        /* eslint-disable-next-line react/no-array-index-key */
                        key={accId}
                        headingId={accId}
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
                                editReleaseDataService.downloadDataFile(
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
                                editReleaseDataService.downloadDataMetadataFile(
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
                                    editReleaseDataService
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
                onConfirm={async () => {
                  await editReleaseDataService
                    .deleteDataFiles(
                      releaseId,
                      (deleteDataFile as DeleteDataFile).file,
                    )
                    .finally(() => {
                      setDeleteDataFile(undefined);
                      resetPage(form);
                    });
                }}
              >
                <p>
                  This data will no longer be available for use in this release.
                </p>
                {deleteDataFile.plan.dependentDataBlocks.length > 0 && (
                  <p>
                    The following data blocks will also be deleted:
                    <ul>
                      {deleteDataFile.plan.dependentDataBlocks.map(block => (
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
                      ))}
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
    />
  );
};

export default ReleaseDataUploadsSection;
