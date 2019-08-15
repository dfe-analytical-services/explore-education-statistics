import Link from '@admin/components/Link';
import service from '@admin/services/release/edit-release/data/service';
import { DataFile } from '@admin/services/release/edit-release/data/types';
import Button from '@common/components/Button';
import { Form, FormFieldset, Formik } from '@common/components/form';
import FormFieldFileSelector from '@common/components/form/FormFieldFileSelector';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import handleServerSideValidation, {
  errorCodeToFieldError,
} from '@common/components/form/util/serverValidationHandler';
import ModalConfirm from '@common/components/ModalConfirm';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import Yup from '@common/lib/validation/yup';
import { FormikActions, FormikProps } from 'formik';
import React, { useEffect, useState } from 'react';

interface FormValues {
  subjectTitle: string;
  dataFile: File | null;
  metadataFile: File | null;
}

interface Props {
  publicationId: string;
  releaseId: string;
}

const formId = 'dataFileUploadForm';

const ReleaseDataUploadsSection = ({ publicationId, releaseId }: Props) => {
  const [dataFiles, setDataFiles] = useState<DataFile[]>();
  const [deleteFileName, setDeleteFileName] = useState('');

  useEffect(() => {
    service.getReleaseDataFiles(releaseId).then(setDataFiles);
  }, [publicationId, releaseId]);

  const resetPage = async <T extends {}>({ resetForm }: FormikActions<T>) => {
    resetForm();

    document
      .querySelectorAll(`#${formId} input[type='file']`)
      .forEach(input => {
        const fileInput = input as HTMLInputElement;
        fileInput.value = '';
      });

    const files = await service.getReleaseDataFiles(releaseId);
    setDataFiles(files);
  };

  const handleServerValidation = handleServerSideValidation(
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
      'DATA_FILE_CAN_NOT_BE_EMPTY',
      'dataFile',
      'Choose a data file that is not empty',
    ),
    errorCodeToFieldError(
      'METADATA_FILE_CAN_NOT_BE_EMPTY',
      'metadataFile',
      'Choose a metadata file that is not empty',
    ),
  );

  return (
    <Formik<FormValues>
      enableReinitialize
      initialValues={{
        subjectTitle: '',
        dataFile: null,
        metadataFile: null,
      }}
      onSubmit={async (values: FormValues, actions) => {
        await service.uploadDataFiles(releaseId, {
          subjectTitle: values.subjectTitle,
          dataFile: values.dataFile as File,
          metadataFile: values.metadataFile as File,
        });

        await resetPage(actions);
      }}
      validationSchema={Yup.object<FormValues>({
        subjectTitle: Yup.string().required('Enter a subject title'),
        dataFile: Yup.mixed().required('Choose a data file'),
        metadataFile: Yup.mixed().required('Choose a metadata file'),
      })}
      render={(form: FormikProps<FormValues>) => {
        return (
          <Form id={formId} submitValidationHandler={handleServerValidation}>
            {dataFiles &&
              dataFiles.map(dataFile => (
                <SummaryList key={dataFile.filename}>
                  <SummaryListItem term="Subject title">
                    {dataFile.title}
                  </SummaryListItem>
                  <SummaryListItem term="Data file">
                    <a
                      href={service.createDownloadDataFileLink(
                        releaseId,
                        dataFile.filename,
                      )}
                    >
                      {dataFile.filename}
                    </a>
                  </SummaryListItem>
                  <SummaryListItem term="Filesize">
                    {dataFile.fileSize.size.toLocaleString()}{' '}
                    {dataFile.fileSize.unit}
                  </SummaryListItem>
                  <SummaryListItem term="Number of rows">
                    {dataFile.numberOfRows.toLocaleString()}
                  </SummaryListItem>
                  <SummaryListItem term="Metadata file">
                    <a
                      href={service.createDownloadDataMetadataFileLink(
                        releaseId,
                        dataFile.filename,
                      )}
                    >
                      {dataFile.metadataFilename}
                    </a>
                  </SummaryListItem>
                  <SummaryListItem
                    term="Actions"
                    actions={
                      <Link
                        to="#"
                        onClick={() => setDeleteFileName(dataFile.filename)}
                      >
                        Delete files
                      </Link>
                    }
                  />
                </SummaryList>
              ))}
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
              <Link to="#" onClick={() => resetPage(form)}>
                Cancel
              </Link>
            </div>

            <ModalConfirm
              mounted={deleteFileName != null && deleteFileName.length > 0}
              title="Confirm deletion of selected data files"
              onExit={() => setDeleteFileName('')}
              onCancel={() => setDeleteFileName('')}
              onConfirm={async () => {
                await service.deleteDataFiles(releaseId, deleteFileName);
                setDeleteFileName('');
                resetPage(form);
              }}
            >
              <p>
                This data will no longer be available for use in this release
              </p>
            </ModalConfirm>
          </Form>
        );
      }}
    />
  );
};

export default ReleaseDataUploadsSection;
