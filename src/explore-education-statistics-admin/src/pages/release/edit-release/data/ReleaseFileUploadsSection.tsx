import Link from '@admin/components/Link';
import { dataRoute } from '@admin/routes/edit-release/routes';
import service from '@admin/services/release/edit-release/data/service';
import { AncillaryFile } from '@admin/services/release/edit-release/data/types';
import Button from '@common/components/Button';
import FormFieldFileSelector from '@common/components/form/FormFieldFileSelector';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import { Form, FormFieldset, Formik } from '@common/components/form';
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

const ReleaseFileUploadsSection = ({ publicationId, releaseId }: Props) => {
  const [files, setFiles] = useState<AncillaryFile[]>();
  const [deleteFileName, setDeleteFileName] = useState('');

  useEffect(() => {
    service.getAncillaryFiles(releaseId).then(setFiles);
  }, [publicationId, releaseId]);

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

  return (
    <Formik<FormValues>
      enableReinitialize
      initialValues={{
        name: '',
        file: null,
      }}
      onSubmit={async (values: FormValues, actions) => {
        await service.uploadAncillaryFile(releaseId, {
          name: values.name,
          file: values.file as File,
        });

        resetPage(actions);
      }}
      validationSchema={Yup.object<FormValues>({
        name: Yup.string().required('Enter a name'),
        file: Yup.mixed().required('Choose a file'),
      })}
      render={(form: FormikProps<FormValues>) => {
        return (
          <Form id={formId}>
            {files && files.map(file => (
              <SummaryList key={file.file.fileName}>
                <SummaryListItem term="Name">{file.title}</SummaryListItem>
                <SummaryListItem term="File">
                  <a
                    href={service.createDownloadDataFileLink(
                      releaseId,
                      file.file.fileName,
                    )}
                  >
                    {file.file.fileName}
                  </a>
                </SummaryListItem>
                <SummaryListItem term="Filesize">
                  {file.fileSize.size.toLocaleString()} {file.fileSize.unit}
                </SummaryListItem>
                <SummaryListItem
                  term="Actions"
                  actions={
                    <Link
                      to="#"
                      onClick={() => setDeleteFileName(file.file.fileName)}
                    >
                      Delete file
                    </Link>
                  }
                />
              </SummaryList>
            ))}
            <FormFieldset
              id={`${formId}-allFieldsFieldset`}
              legend="Upload file"
            >
              <FormFieldTextInput<FormValues>
                id={`${formId}-name`}
                name="name"
                label="Name"
              />

              <FormFieldFileSelector<FormValues>
                id={`${formId}-file`}
                name="file"
                label="Upload file"
                formGroupClass="govuk-!-margin-top-6"
                form={form}
              />
            </FormFieldset>

            <Button type="submit" className="govuk-!-margin-top-6">
              Upload file
            </Button>

            <div className="govuk-!-margin-top-6">
              <Link to={dataRoute.generateLink(publicationId, releaseId)}>
                Cancel
              </Link>
            </div>

            <ModalConfirm
              mounted={deleteFileName != null && deleteFileName.length > 0}
              title="Confirm deletion of file"
              onExit={() => setDeleteFileName('')}
              onCancel={() => setDeleteFileName('')}
              onConfirm={async () => {
                await service.deleteAncillaryFile(releaseId, deleteFileName);
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
