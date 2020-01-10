import Link from '@admin/components/Link';
import service from '@admin/services/release/edit-release/data/service';
import permissionService from '@admin/services/permissions/service';
import { AncillaryFile } from '@admin/services/release/edit-release/data/types';
import submitWithFormikValidation from '@admin/validation/formikSubmitHandler';
import withErrorControl, {
  ErrorControlProps,
} from '@admin/validation/withErrorControl';
import Button from '@common/components/Button';
import { Form, FormFieldset, Formik } from '@common/components/form';
import FormFieldFileSelector from '@common/components/form/FormFieldFileSelector';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import { errorCodeToFieldError } from '@common/components/form/util/serverValidationHandler';
import ModalConfirm from '@common/components/ModalConfirm';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import Yup from '@common/lib/validation/yup';
import { FormikActions, FormikProps } from 'formik';
import React, { useEffect, useState } from 'react';

interface FormValues {
  name: string;
  file: File | null;
}

interface Props {
  publicationId: string;
  releaseId: string;
}

const formId = 'fileUploadForm';

const ReleaseFileUploadsSection = ({
  publicationId,
  releaseId,
  handleApiErrors,
}: Props & ErrorControlProps) => {
  const [files, setFiles] = useState<AncillaryFile[]>();
  const [deleteFileName, setDeleteFileName] = useState('');
  const [canUpdateRelease, setCanUpdateRelease] = useState(false);

  useEffect(() => {
    Promise.all([
      service.getAncillaryFiles(releaseId),
      permissionService.canUpdateRelease(releaseId),
    ])
      .then(([filesResult, canUpdateReleaseResult]) => {
        setFiles(filesResult);
        setCanUpdateRelease(canUpdateReleaseResult);
      })
      .catch(handleApiErrors);
  }, [publicationId, releaseId, handleApiErrors]);

  const resetPage = async <T extends {}>({ resetForm }: FormikActions<T>) => {
    resetForm();
    document
      .querySelectorAll(`#${formId} input[type='file']`)
      .forEach(input => {
        const fileInput = input as HTMLInputElement;
        fileInput.value = '';
      });

    const latestFiles = await service.getAncillaryFiles(releaseId);
    setFiles(latestFiles);
  };

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
  ];

  const submitFormHandler = submitWithFormikValidation<FormValues>(
    async (values, actions) => {
      await service.uploadAncillaryFile(releaseId, {
        name: values.name,
        file: values.file as File,
      });

      await resetPage(actions);
    },
    handleApiErrors,
    ...errorCodeMappings,
  );

  return (
    <Formik<FormValues>
      enableReinitialize
      initialValues={{
        name: '',
        file: null,
      }}
      onSubmit={submitFormHandler}
      validationSchema={Yup.object<FormValues>({
        name: Yup.string().required('Enter a name'),
        file: Yup.mixed().required('Choose a file'),
      })}
      render={(form: FormikProps<FormValues>) => {
        return (
          <Form id={formId}>
            {canUpdateRelease && (
              <>
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

                  <FormFieldFileSelector<FormValues>
                    id={`${formId}-file`}
                    name="file"
                    label="Upload file"
                    formGroupClass="govuk-!-margin-top-6"
                    form={form}
                  />
                </FormFieldset>

                <Button
                  type="submit"
                  className="govuk-button govuk-!-margin-right-6"
                >
                  Upload file
                </Button>

                <Link
                  to="#"
                  className="govuk-button govuk-button--secondary"
                  onClick={() => resetPage(form)}
                >
                  Cancel
                </Link>
              </>
            )}

            {files && files.length > 0 && (
              <>
                <hr />
                <h2 className="govuk-heading-m">Uploaded files</h2>
              </>
            )}

            {files &&
              files.map(file => (
                <SummaryList key={file.filename}>
                  <SummaryListItem term="Name">
                    <h4 className="govuk-heading-m">{file.title}</h4>
                  </SummaryListItem>
                  <SummaryListItem term="File">
                    <Link
                      to="#"
                      onClick={() =>
                        service
                          .downloadAncillaryFile(releaseId, file.filename)
                          .catch(handleApiErrors)
                      }
                    >
                      {file.filename}
                    </Link>
                  </SummaryListItem>
                  <SummaryListItem term="Filesize">
                    {file.fileSize.size.toLocaleString()} {file.fileSize.unit}
                  </SummaryListItem>
                  {canUpdateRelease && (
                    <SummaryListItem
                      term="Actions"
                      actions={
                        <Link
                          to="#"
                          onClick={() => setDeleteFileName(file.filename)}
                        >
                          Delete file
                        </Link>
                      }
                    />
                  )}
                </SummaryList>
              ))}

            <ModalConfirm
              mounted={deleteFileName !== null && deleteFileName.length > 0}
              title="Confirm deletion of file"
              onExit={() => setDeleteFileName('')}
              onCancel={() => setDeleteFileName('')}
              onConfirm={async () => {
                await service
                  .deleteAncillaryFile(releaseId, deleteFileName)
                  .catch(handleApiErrors);
                setDeleteFileName('');
                resetPage(form);
              }}
            >
              <p>
                This file will no longer be available for use in this release
              </p>
            </ModalConfirm>
          </Form>
        );
      }}
    />
  );
};

export default withErrorControl(ReleaseFileUploadsSection);
