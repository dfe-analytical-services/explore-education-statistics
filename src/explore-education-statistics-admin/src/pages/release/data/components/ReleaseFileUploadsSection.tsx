import useFormSubmit from '@admin/hooks/useFormSubmit';
import releaseAncillaryFileService, {
  AncillaryFile,
} from '@admin/services/releaseAncillaryFileService';
import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import { Form } from '@common/components/form';
import FormFieldFileInput from '@common/components/form/FormFieldFileInput';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import InsetText from '@common/components/InsetText';
import LoadingSpinner from '@common/components/LoadingSpinner';
import ModalConfirm from '@common/components/ModalConfirm';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import WarningMessage from '@common/components/WarningMessage';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import logger from '@common/services/logger';
import { mapFieldErrors } from '@common/validation/serverValidations';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import React, { useState } from 'react';

interface FormValues {
  name: string;
  file: File | null;
}

const errorMappings = [
  mapFieldErrors<FormValues>({
    target: 'name',
    messages: {
      FILE_UPLOAD_NAME_CANNOT_CONTAIN_SPECIAL_CHARACTERS:
        'File upload name cannot contain special characters',
    },
  }),
  mapFieldErrors<FormValues>({
    target: 'file',
    messages: {
      CANNOT_OVERWRITE_FILE: 'Choose a unique file name',
      FILE_CANNOT_BE_EMPTY: 'Choose a file that is not empty',
      FILE_TYPE_INVALID: 'Choose a file of an allowed format',
      FILENAME_CANNOT_CONTAIN_SPACES_OR_SPECIAL_CHARACTERS:
        'Filename cannot contain spaces or special characters',
    },
  }),
];

interface Props {
  releaseId: string;
  canUpdateRelease: boolean;
}

const formId = 'fileUploadForm';

const ReleaseFileUploadsSection = ({ releaseId, canUpdateRelease }: Props) => {
  const [deleteFile, setDeleteFile] = useState<AncillaryFile>();
  const [isUploading, setIsUploading] = useState(false);

  const {
    value: files = [],
    setState: setFiles,
    isLoading,
  } = useAsyncHandledRetry(
    () => releaseAncillaryFileService.getAncillaryFiles(releaseId),
    [releaseId],
  );

  const setFileDeleting = (fileToDelete: AncillaryFile, deleting: boolean) => {
    setFiles({
      value: files.map(file =>
        file.filename !== fileToDelete.filename
          ? file
          : {
              ...file,
              isDeleting: deleting,
            },
      ),
    });
  };

  const handleSubmit = useFormSubmit<FormValues>(async (values, actions) => {
    setIsUploading(true);

    try {
      const newFile = await releaseAncillaryFileService.uploadAncillaryFile(
        releaseId,
        {
          name: values.name.trim(),
          file: values.file as File,
        },
      );

      actions.resetForm();
      setFiles({
        value: [...files, newFile],
      });
    } finally {
      setIsUploading(false);
    }
  }, errorMappings);

  return (
    <>
      <Formik<FormValues>
        enableReinitialize
        initialValues={{
          name: '',
          file: null,
        }}
        onReset={() => {
          document
            .querySelectorAll(`#${formId} input[type='file']`)
            .forEach(input => {
              const fileInput = input as HTMLInputElement;
              fileInput.value = '';
            });
        }}
        onSubmit={handleSubmit}
        validationSchema={Yup.object<FormValues>({
          name: Yup.string()
            .trim()
            .required('Enter a name')
            .test({
              name: 'unique',
              message: 'Enter a unique name',
              test(value: string) {
                if (!value) {
                  return true;
                }
                return (
                  files.find(
                    f => f.title.toUpperCase() === value.toUpperCase(),
                  ) === undefined
                );
              },
            }),
          file: Yup.file()
            .required('Choose a file')
            .minSize(0, 'Choose a file that is not empty'),
        })}
      >
        {form => (
          <>
            <h2>Add file to release</h2>
            <InsetText>
              <h3>Before you start</h3>
              <p>
                Ancillary files are additional files attached to the release for
                users to download. They will appear in the associated files list
                on the release page and the download files page.
              </p>
            </InsetText>

            {canUpdateRelease ? (
              <Form id={formId}>
                <div style={{ position: 'relative' }}>
                  {isUploading && (
                    <LoadingSpinner text="Uploading files" overlay />
                  )}

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
                  />

                  <ButtonGroup>
                    <Button type="submit" disabled={form.isSubmitting}>
                      Upload file
                    </Button>

                    <ButtonText
                      disabled={form.isSubmitting}
                      onClick={async () => {
                        form.resetForm();
                      }}
                    >
                      Cancel
                    </ButtonText>
                  </ButtonGroup>
                </div>
              </Form>
            ) : (
              <WarningMessage>
                This release has been approved, and can no longer be updated.
              </WarningMessage>
            )}
          </>
        )}
      </Formik>

      <hr className="govuk-!-margin-top-6 govuk-!-margin-bottom-6" />

      <h2>Uploaded files</h2>

      <LoadingSpinner loading={isLoading}>
        {files.length > 0 ? (
          <Accordion id="uploadedFiles">
            {files.map(file => (
              <AccordionSection
                key={file.title}
                heading={file.title}
                headingTag="h3"
              >
                <div style={{ position: 'relative' }}>
                  {file.isDeleting && (
                    <LoadingSpinner text="Deleting file" overlay />
                  )}

                  <SummaryList>
                    <SummaryListItem term="Name">{file.title}</SummaryListItem>
                    <SummaryListItem term="File">
                      <ButtonText
                        onClick={() =>
                          releaseAncillaryFileService.downloadFile(
                            releaseId,
                            file.id,
                            file.filename,
                          )
                        }
                      >
                        {file.filename}
                      </ButtonText>
                    </SummaryListItem>
                    <SummaryListItem term="File size">
                      {file.fileSize.size.toLocaleString()} {file.fileSize.unit}
                    </SummaryListItem>
                    {canUpdateRelease && (
                      <SummaryListItem
                        term="Actions"
                        actions={
                          <ButtonText onClick={() => setDeleteFile(file)}>
                            Delete file
                          </ButtonText>
                        }
                      />
                    )}
                  </SummaryList>
                </div>
              </AccordionSection>
            ))}
          </Accordion>
        ) : (
          <InsetText>No files have been uploaded.</InsetText>
        )}
      </LoadingSpinner>

      {deleteFile && (
        <ModalConfirm
          open
          title="Confirm deletion of file"
          onExit={() => setDeleteFile(undefined)}
          onCancel={() => setDeleteFile(undefined)}
          onConfirm={async () => {
            setFileDeleting(deleteFile, true);
            setDeleteFile(undefined);

            try {
              await releaseAncillaryFileService.deleteAncillaryFile(
                releaseId,
                deleteFile.id,
              );

              setFiles({
                value: files.filter(file => file !== deleteFile),
              });
            } catch (err) {
              logger.error(err);
              setFileDeleting(deleteFile, false);
            }
          }}
        >
          <p>This file will no longer be available for use in this release</p>
        </ModalConfirm>
      )}
    </>
  );
};

export default ReleaseFileUploadsSection;
