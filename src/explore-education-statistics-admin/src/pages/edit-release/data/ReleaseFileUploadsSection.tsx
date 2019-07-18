import Link from '@admin/components/Link';
import { dataRoute } from '@admin/routes/releaseRoutes';
import service from '@admin/services/edit-release/data/service';
import { AdhocFile, DataFile } from '@admin/services/edit-release/data/types';
import Button from '@common/components/Button';
import { Form, FormFieldset, Formik } from '@common/components/form';
import FormFieldFileSelector from '@common/components/form/FormFieldFileSelector';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import ModalConfirm from '@common/components/ModalConfirm';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import Yup from '@common/lib/validation/yup';
import { FormikProps } from 'formik';
import React, { useEffect, useState } from 'react';

interface FormValues {
  name: string;
  file: File | null;
}

interface Props {
  releaseId: string;
}

const formId = 'fileUploadForm';

const ReleaseFileUploadsSection = ({ releaseId }: Props) => {
  const [files, setFiles] = useState<AdhocFile[]>();
  const [deleteFileId, setDeleteFileRequest] = useState('');

  useEffect(() => {
    service.getReleaseAdhocFiles(releaseId).then(setFiles);
  }, [releaseId]);

  return (
    <>
      {files &&
        files.map(file => (
          <SummaryList key={file.file.id}>
            <SummaryListItem term="Name">{file.title}</SummaryListItem>
            <SummaryListItem term="File">
              <a
                href={service.createDownloadDataFileLink(
                  releaseId,
                  file.file.id,
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
                <Link to="#" onClick={_ => setDeleteFileRequest(file.file.id)}>
                  Delete file
                </Link>
              }
            />
          </SummaryList>
        ))}
      <Formik<FormValues>
        enableReinitialize
        initialValues={{
          name: '',
          file: null,
        }}
        onSubmit={async (values: FormValues, actions) => {
          service
            .uploadAdhocFile(releaseId, {
              name: values.name,
              file: values.file as File,
            })
            .then(_ => service.getReleaseAdhocFiles(releaseId))
            .then(setFiles)
            .then(_ => {
              actions.resetForm();
              document
                .querySelectorAll(`#${formId} input[type='file']`)
                .forEach(input => {
                  const fileInput = input as HTMLInputElement;
                  fileInput.value = '';
                });
            });
        }}
        validationSchema={Yup.object<FormValues>({
          name: Yup.string().required('Enter a name'),
          file: Yup.mixed().required('Choose a file'),
        })}
        render={(form: FormikProps<FormValues>) => {
          return (
            <Form id={formId}>
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
                <Link to={dataRoute.generateLink(releaseId)}>Cancel</Link>
              </div>
            </Form>
          );
        }}
      />

      <ModalConfirm
        mounted={deleteFileId != null && deleteFileId.length > 0}
        title="Confirm deletion of file"
        onExit={() => setDeleteFileRequest('')}
        onCancel={() => setDeleteFileRequest('')}
        onConfirm={() =>
          service
            .deleteAdhocFile(releaseId, deleteFileId)
            .then(_ => service.getReleaseAdhocFiles(releaseId))
            .then(setFiles)
            .then(_ => setDeleteFileRequest(''))
        }
      >
        <p>This file will no longer be available for use in this release</p>
      </ModalConfirm>
    </>
  );
};

export default ReleaseFileUploadsSection;
