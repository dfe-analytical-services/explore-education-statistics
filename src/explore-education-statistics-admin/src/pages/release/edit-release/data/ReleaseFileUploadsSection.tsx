import useFormSubmit from '@admin/hooks/useFormSubmit';
import permissionService from '@admin/services/permissions/permissionService';
import editReleaseDataService, {
  AncillaryFile,
} from '@admin/services/release/edit-release/data/editReleaseDataService';
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
import Yup from '@common/lib/validation/yup';
import { FormikActions, FormikProps } from 'formik';
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
  const [files, setFiles] = useState<AncillaryFile[]>();
  const [deleteFileName, setDeleteFileName] = useState('');
  const [canUpdateRelease, setCanUpdateRelease] = useState(false);
  const [openedAccordions, setOpenedAccordions] = useState<string[]>([]);

  useEffect(() => {
    Promise.all([
      editReleaseDataService.getAncillaryFiles(releaseId),
      permissionService.canUpdateRelease(releaseId),
    ]).then(([filesResult, canUpdateReleaseResult]) => {
      setFiles(filesResult);
      setCanUpdateRelease(canUpdateReleaseResult);
    });
  }, [publicationId, releaseId]);

  const resetPage = async <T extends {}>({ resetForm }: FormikActions<T>) => {
    resetForm();
    document
      .querySelectorAll(`#${formId} input[type='file']`)
      .forEach(input => {
        const fileInput = input as HTMLInputElement;
        fileInput.value = '';
      });

    const latestFiles = await editReleaseDataService.getAncillaryFiles(
      releaseId,
    );
    setFiles(latestFiles);
  };

  const handleSubmit = useFormSubmit<FormValues>(async (values, actions) => {
    await editReleaseDataService.uploadAncillaryFile(releaseId, {
      name: values.name,
      file: values.file as File,
    });

    await resetPage(actions);
  }, errorCodeMappings);

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
                        headingId={accId}
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
                        <SummaryList key={file.filename}>
                          <SummaryListItem term="Name">
                            <h4 className="govuk-heading-m">{file.title}</h4>
                          </SummaryListItem>
                          <SummaryListItem term="File">
                            <ButtonText
                              onClick={() =>
                                editReleaseDataService.downloadAncillaryFile(
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
              onConfirm={async () => {
                await editReleaseDataService.deleteAncillaryFile(
                  releaseId,
                  deleteFileName,
                );
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

export default ReleaseFileUploadsSection;
