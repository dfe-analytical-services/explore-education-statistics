import Link from '@admin/components/Link';
import {dataRoute} from '@admin/routes/edit-release/routes';
import service from '@admin/services/edit-release/data/service';
import {DataFile} from '@admin/services/edit-release/data/types';
import Button from '@common/components/Button';
import FormFieldFileSelector from '@common/components/form/FormFieldFileSelector';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import {Form, FormFieldset, Formik} from '@common/components/form/index';
import ModalConfirm from '@common/components/ModalConfirm';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import Yup from '@common/lib/validation/yup/index';
import {FormikProps} from 'formik';
import React, {useEffect, useState} from 'react';

interface FormValues {
  subjectTitle: string;
  dataFile: File | null;
  metadataFile: File | null;
}

interface Props {
  releaseId: string;
}

const formId = 'dataFileUploadForm';

const ReleaseDataUploadsSection = ({ releaseId }: Props) => {
  const [dataFiles, setDataFiles] = useState<DataFile[]>();
  const [deleteFileId, setDeleteFilesRequest] = useState('');

  useEffect(() => {
    service.getReleaseDataFiles(releaseId).then(setDataFiles);
  }, [releaseId]);

  return (
    <>
      {dataFiles &&
        dataFiles.map(dataFile => (
          <SummaryList key={dataFile.file.id}>
            <SummaryListItem term="Subject title">
              {dataFile.title}
            </SummaryListItem>
            <SummaryListItem term="Data file">
              <a
                href={service.createDownloadDataFileLink(
                  releaseId,
                  dataFile.file.id,
                )}
              >
                {dataFile.file.fileName}
              </a>
            </SummaryListItem>
            <SummaryListItem term="Filesize">
              {dataFile.fileSize.size.toLocaleString()} {dataFile.fileSize.unit}
            </SummaryListItem>
            <SummaryListItem term="Number of rows">
              {dataFile.numberOfRows.toLocaleString()}
            </SummaryListItem>
            <SummaryListItem term="Metadata file">
              <a
                href={service.createDownloadDataMetadataFileLink(
                  releaseId,
                  dataFile.file.id,
                )}
              >
                {dataFile.metadataFile.fileName}
              </a>
            </SummaryListItem>
            <SummaryListItem
              term="Actions"
              actions={
                <Link
                  to="#"
                  onClick={_ => setDeleteFilesRequest(dataFile.file.id)}
                >
                  Delete files
                </Link>
              }
            />
          </SummaryList>
        ))}
      <Formik<FormValues>
        enableReinitialize
        initialValues={{
          subjectTitle: '',
          dataFile: null,
          metadataFile: null,
        }}
        onSubmit={async (values: FormValues, actions) => {
          service
            .uploadDataFiles(releaseId, {
              subjectTitle: values.subjectTitle,
              dataFile: values.dataFile as File,
              metadataFile: values.metadataFile as File,
            })
            .then(_ => service.getReleaseDataFiles(releaseId))
            .then(setDataFiles)
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
          subjectTitle: Yup.string().required('Enter a subject title'),
          dataFile: Yup.mixed().required('Choose a data file'),
          metadataFile: Yup.mixed().required('Choose a metadata file'),
        })}
        render={(form: FormikProps<FormValues>) => {
          return (
            <Form id={formId}>
              <FormFieldset
                id={`${formId}-allFieldsFieldset`}
                legend="Add new data to release"
              >
                <FormFieldTextInput<FormValues>
                  id={`${formId}-subjectTitle`}
                  name="subjectTitle"
                  label="Subject title"
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

              <Button type="submit" className="govuk-!-margin-top-6">
                Upload data files
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
        title="Confirm deletion of selected data files"
        onExit={() => setDeleteFilesRequest('')}
        onCancel={() => setDeleteFilesRequest('')}
        onConfirm={() =>
          service
            .deleteDataFiles(releaseId, deleteFileId)
            .then(_ => service.getReleaseDataFiles(releaseId))
            .then(setDataFiles)
            .then(_ => setDeleteFilesRequest(''))
        }
      >
        <p>This data will no longer be available for use in this release</p>
      </ModalConfirm>
    </>
  );
};

export default ReleaseDataUploadsSection;
