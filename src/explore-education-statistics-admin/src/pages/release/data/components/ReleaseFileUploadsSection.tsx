import releaseAncillaryFileService, {
  AncillaryFile,
} from '@admin/services/releaseAncillaryFileService';
import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import { Form, FormFieldTextArea } from '@common/components/form';
import FormFieldFileInput from '@common/components/form/FormFieldFileInput';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import InsetText from '@common/components/InsetText';
import LoadingSpinner from '@common/components/LoadingSpinner';
import ModalConfirm from '@common/components/ModalConfirm';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import WarningMessage from '@common/components/WarningMessage';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import useFormSubmit from '@common/hooks/useFormSubmit';
import logger from '@common/services/logger';
import { mapFieldErrors } from '@common/validation/serverValidations';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import React, { useState } from 'react';
import Link from '@admin/components/Link';
import { generatePath } from 'react-router';
import {
  releaseAncillaryFileRoute,
  ReleaseDataFileRouteParams,
} from '@admin/routes/releaseRoutes';
import FormattedDate from '@common/components/FormattedDate';

interface FormValues {
  title: string;
  summary: string;
  file: File | null;
}

const errorMappings = [
  mapFieldErrors<FormValues>({
    target: 'title',
    messages: {
      FileUploadNameCannotContainSpecialCharacters:
        'File upload name cannot contain special characters',
    },
  }),
  mapFieldErrors<FormValues>({
    target: 'file',
    messages: {
      CannotOverwriteFile: 'Choose a unique file name',
      FileCannotBeEmpty: 'Choose a file that is not empty',
      FileTypeInvalid: 'Choose a file of an allowed format',
      FilenameCannotContainSpacesOrSpecialCharacters:
        'Filename cannot contain spaces or special characters',
    },
  }),
];

interface Props {
  publicationId: string;
  releaseId: string;
  canUpdateRelease: boolean;
}

const formId = 'fileUploadForm';

const ReleaseFileUploadsSection = ({
  publicationId,
  releaseId,
  canUpdateRelease,
}: Props) => {
  const [deleteFile, setDeleteFile] = useState<AncillaryFile>();
  const [isUploading, setIsUploading] = useState(false);

  const {
    value: files = [],
    setState: setFiles,
    isLoading,
  } = useAsyncHandledRetry(
    async () => releaseAncillaryFileService.getAncillaryFiles(releaseId),
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
          title: values.title.trim(),
          file: values.file as File,
          summary: values.summary.trim(),
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
          title: '',
          summary: '',
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
          title: Yup.string()
            .trim()
            .required('Enter a title')
            .test({
              name: 'unique',
              message: 'Enter a unique title',
              test(value) {
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
          summary: Yup.string().required('Enter a summary'),
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
                    className="govuk-!-width-one-half"
                    name="title"
                    label="Title"
                  />

                  <FormFieldTextArea<FormValues>
                    className="govuk-!-width-one-half"
                    name="summary"
                    label="Summary"
                  />

                  <FormFieldFileInput<FormValues>
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
                    <SummaryListItem term="Title">{file.title}</SummaryListItem>

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

                    <SummaryListItem term="Uploaded by">
                      <a href={`mailto:${file.userName}`}>{file.userName}</a>
                    </SummaryListItem>

                    <SummaryListItem term="Date uploaded">
                      <FormattedDate format="d MMMM yyyy HH:mm">
                        {file.created}
                      </FormattedDate>
                    </SummaryListItem>

                    <SummaryListItem term="Summary">
                      <div className="dfe-white-space--pre-wrap">
                        {file.summary}
                      </div>
                    </SummaryListItem>

                    {canUpdateRelease && (
                      <SummaryListItem
                        term="Actions"
                        actions={
                          <>
                            <Link
                              className="govuk-!-margin-right-4"
                              to={generatePath<ReleaseDataFileRouteParams>(
                                releaseAncillaryFileRoute.path,
                                {
                                  publicationId,
                                  releaseId,
                                  fileId: file.id,
                                },
                              )}
                            >
                              Edit file
                            </Link>
                            <ButtonText onClick={() => setDeleteFile(file)}>
                              Delete file
                            </ButtonText>
                          </>
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
          <p>
            This file will no longer be available for use in this release (
            {deleteFile.filename})
          </p>
        </ModalConfirm>
      )}
    </>
  );
};

export default ReleaseFileUploadsSection;
