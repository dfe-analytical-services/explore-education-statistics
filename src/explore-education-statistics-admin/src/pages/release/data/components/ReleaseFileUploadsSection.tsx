import useFormSubmit from '@admin/hooks/useFormSubmit';
import permissionService from '@admin/services/permissionService';
import releaseAncillaryFileService, {
  AncillaryFile,
} from '@admin/services/releaseAncillaryFileService';
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
import { Formik, FormikHelpers, FormikProps } from 'formik';
import remove from 'lodash/remove';
import React, { useEffect, useState } from 'react';

const errorCodeMappings = [
  errorCodeToFieldError(
    'CANNOT_OVERWRITE_FILE',
    'file',
    'Choose a unique file name',
  ),
  errorCodeToFieldError(
    'FILE_CANNOT_BE_EMPTY',
    'file',
    'Choose a file that is not empty',
  ),
  errorCodeToFieldError(
    'FILE_TYPE_INVALID',
    'file',
    'Choose a file of an allowed format',
  ),
  errorCodeToFieldError(
    'FILENAME_CANNOT_CONTAIN_SPACES_OR_SPECIAL_CHARACTERS',
    'file',
    'Filename cannot contain spaces or special characters',
  ),
];

interface FormValues {
  name: string;
  file: File | null;
}

interface Props {
  publicationId: string;
  releaseId: string;
}

const formId = 'fileUploadForm';

const ReleaseFileUploadsSection = ({ publicationId, releaseId }: Props) => {
  const [files, setFiles] = useState<AncillaryFile[]>([]);
  const [deleteFileName, setDeleteFileName] = useState('');
  const [canUpdateRelease, setCanUpdateRelease] = useState(false);
  const [openedAccordions, setOpenedAccordions] = useState<string[]>([]);
  const [isUploading, setIsUploading] = useState(false);

  useEffect(() => {
    Promise.all([
      releaseAncillaryFileService.getAncillaryFiles(releaseId),
      permissionService.canUpdateRelease(releaseId),
    ]).then(([filesResult, canUpdateReleaseResult]) => {
      setFiles(filesResult);
      setCanUpdateRelease(canUpdateReleaseResult);
    });
  }, [publicationId, releaseId]);

  const resetPage = async ({ resetForm }: FormikHelpers<FormValues>) => {
    resetForm();
    document
      .querySelectorAll(`#${formId} input[type='file']`)
      .forEach(input => {
        const fileInput = input as HTMLInputElement;
        fileInput.value = '';
      });

    const latestFiles = await releaseAncillaryFileService.getAncillaryFiles(
      releaseId,
    );
    setFiles(latestFiles);
  };

  const handleSubmit = useFormSubmit<FormValues>(async (values, actions) => {
    setIsUploading(true);
    await releaseAncillaryFileService
      .uploadAncillaryFile(releaseId, {
        name: values.name,
        file: values.file as File,
      })
      .then(() => {
        setIsUploading(false);
        resetPage(actions);
      })
      .finally(() => {
        setIsUploading(false);
      });
  }, errorCodeMappings);

  const setDeleting = (ancillaryFile: string, deleting: boolean) => {
    setFiles(
      files.map(file =>
        file.filename !== ancillaryFile
          ? file
          : {
              ...file,
              isDeleting: deleting,
            },
      ),
    );
  };

  const handleDelete = async (
    ancillaryFileToDelete: string,
    form: FormikProps<FormValues>,
  ) => {
    setDeleting(ancillaryFileToDelete, true);
    setDeleteFileName('');
    await releaseAncillaryFileService
      .deleteAncillaryFile(releaseId, deleteFileName)
      .then(() => {
        setDeleting(ancillaryFileToDelete, false);
        resetPage(form);
      })
      .finally(() => {
        setDeleting(ancillaryFileToDelete, false);
      });
  };

  return (
    <Formik<FormValues>
      enableReinitialize
      initialValues={{
        name: '',
        file: null,
      }}
      onSubmit={handleSubmit}
      validationSchema={Yup.object<FormValues>({
        name: Yup.string().required('Enter a name'),
        file: Yup.mixed().required('Choose a file'),
      })}
    >
      {form => {
        return (
          <Form id={formId}>
            {canUpdateRelease && (
              <>
                {isUploading && (
                  <LoadingSpinner text="Uploading files" overlay />
                )}
                <FormFieldset
                  id={`${formId}-allFieldsFieldset`}
                  legend="Upload file"
                >
                  <FormFieldTextInput<FormValues>
                    id={`${formId}-name`}
                    name="name"
                    label="Name"
                    width={20}
                  />

                  <FormFieldFileInput<FormValues>
                    id={`${formId}-file`}
                    name="file"
                    label="Upload file"
                    formGroupClass="govuk-!-margin-top-6"
                  />
                </FormFieldset>

                <Button
                  type="submit"
                  id="upload-file-button"
                  className="govuk-button govuk-!-margin-right-6"
                >
                  Upload file
                </Button>

                <ButtonText
                  className="govuk-button govuk-button--secondary"
                  onClick={() => resetPage(form)}
                >
                  Cancel
                </ButtonText>
              </>
            )}

            {files && files.length > 0 && (
              <>
                <hr />
                <h2 className="govuk-heading-m">Uploaded files</h2>
                <Accordion id="uploaded-files">
                  {files.map((file, index) => {
                    const accId = `${file.title}-${index}`;
                    return (
                      <AccordionSection
                        /* eslint-disable-next-line react/no-array-index-key */
                        key={accId}
                        heading={file.title}
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
                        {file.isDeleting && (
                          <LoadingSpinner text="Deleting file" overlay />
                        )}
                        <SummaryList key={file.filename}>
                          <SummaryListItem term="Name">
                            <h4 className="govuk-heading-m">{file.title}</h4>
                          </SummaryListItem>
                          <SummaryListItem term="File">
                            <ButtonText
                              onClick={() =>
                                releaseAncillaryFileService.downloadAncillaryFile(
                                  releaseId,
                                  file.filename,
                                )
                              }
                            >
                              {file.filename}
                            </ButtonText>
                          </SummaryListItem>
                          <SummaryListItem term="Filesize">
                            {file.fileSize.size.toLocaleString()}{' '}
                            {file.fileSize.unit}
                          </SummaryListItem>
                          {canUpdateRelease && (
                            <SummaryListItem
                              term="Actions"
                              actions={
                                <ButtonText
                                  onClick={() =>
                                    setDeleteFileName(file.filename)
                                  }
                                >
                                  Delete file
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

            <ModalConfirm
              mounted={deleteFileName !== null && deleteFileName.length > 0}
              title="Confirm deletion of file"
              onExit={() => setDeleteFileName('')}
              onCancel={() => setDeleteFileName('')}
              onConfirm={() => handleDelete(deleteFileName, form)}
            >
              <p>
                This file will no longer be available for use in this release
              </p>
            </ModalConfirm>
          </Form>
        );
      }}
    </Formik>
  );
};

export default ReleaseFileUploadsSection;
