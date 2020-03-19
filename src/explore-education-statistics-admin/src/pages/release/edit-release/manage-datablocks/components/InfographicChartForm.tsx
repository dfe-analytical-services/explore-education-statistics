import useFormSubmit from '@admin/hooks/useFormSubmit';
import styles from '@admin/pages/release/edit-release/manage-datablocks/components/graph-builder.module.scss';
import editReleaseDataService from '@admin/services/release/edit-release/data/editReleaseDataService';
import Button from '@common/components/Button';
import ButtonText from '@common/components/ButtonText';
import { Formik } from '@common/components/form';
import Form from '@common/components/form/Form';
import FormFieldFileSelector from '@common/components/form/FormFieldFileSelector';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import { SelectOption } from '@common/components/form/FormSelect';
import { errorCodeToFieldError } from '@common/components/form/util/serverValidationHandler';
import ModalConfirm from '@common/components/ModalConfirm';
import useToggle from '@common/hooks/useToggle';
import Yup from '@common/validation/yup';
import { FormikProps } from 'formik';
import React, { useEffect, useState } from 'react';

const errorCodeMappings = [
  errorCodeToFieldError('FILE_TYPE_INVALID', 'file', 'Choose an image file'),
  errorCodeToFieldError(
    'CANNOT_OVERWRITE_FILE',
    'file',
    'Choose a unique file',
  ),
];

interface FormValues {
  name: string;
  file: File | null;
  fileId: string;
}

interface Props {
  releaseId: string;
  fileId?: string;
  onSubmit: (fileId: string) => void;
}

const loadChartFilesAndMapToSelectOptionAsync = (
  releaseId: string,
): Promise<SelectOption[]> => {
  return editReleaseDataService.getChartFiles(releaseId).then(chartFiles => {
    return [
      {
        label: 'Upload a new file',
        value: '',
      },
      ...chartFiles.map(({ title, filename }) => ({
        label: title,
        value: filename,
      })),
    ];
  });
};

const InfographicChartForm = ({ releaseId, fileId, onSubmit }: Props) => {
  const [chartFileOptions, setChartFileOptions] = useState<SelectOption[]>([]);

  const [uploading, setUploading] = useState(false);
  const [deleteFile, toggleDeleteFile] = useToggle(false);

  const formId = 'fileUploadForm';

  const handleSubmit = useFormSubmit<FormValues>(async values => {
    if (values.file) {
      setUploading(true);

      await editReleaseDataService
        .uploadChartFile(releaseId, {
          name: values.name,
          file: values.file as File,
        })
        .then(() => loadChartFilesAndMapToSelectOptionAsync(releaseId))
        .then(setChartFileOptions)
        .then(() => onSubmit((values.file as File).name))
        .finally(() => {
          setUploading(false);
        });
    }
  }, errorCodeMappings);

  useEffect(() => {
    loadChartFilesAndMapToSelectOptionAsync(releaseId).then(
      setChartFileOptions,
    );
  }, [releaseId]);

  const selectedFile = chartFileOptions.find(
    fileOption => fileOption.value === fileId,
  );

  return (
    <Formik<FormValues>
      enableReinitialize
      initialValues={{
        name: '',
        file: null,
        fileId: fileId || '',
      }}
      validationSchema={Yup.object<FormValues>({
        name: Yup.string().required('Enter a name'),
        file: Yup.mixed().required('Choose a file'),
        fileId: Yup.string(),
      })}
      onSubmit={handleSubmit}
      render={(form: FormikProps<FormValues>) => {
        return (
          <Form id={formId}>
            {fileId && selectedFile && (
              <>
                <div className={styles.deleteInfographicContainer}>
                  <p className="govuk-!-margin-right-2">{`${selectedFile.label}, ${fileId}`}</p>
                  <ButtonText
                    variant="warning"
                    onClick={() => toggleDeleteFile(true)}
                  >
                    Delete infographic
                  </ButtonText>
                </div>
                <ModalConfirm
                  mounted={deleteFile}
                  title="Confirm deletion of infographic"
                  onExit={toggleDeleteFile.off}
                  onCancel={toggleDeleteFile.off}
                  onConfirm={async () => {
                    // eslint-disable-next-line no-unused-expressions
                    form.values.fileId &&
                      editReleaseDataService
                        .deleteChartFile(releaseId, form.values.fileId)
                        .then(() =>
                          loadChartFilesAndMapToSelectOptionAsync(releaseId),
                        )
                        .then(setChartFileOptions);
                    onSubmit('');
                    toggleDeleteFile.off();
                  }}
                >
                  <p>
                    This data will no longer be available for use in this chart
                  </p>
                </ModalConfirm>
              </>
            )}

            {!fileId && (
              <>
                <FormFieldTextInput
                  id={`${formId}-name`}
                  name="name"
                  label="Select a name to give the file"
                  width={10}
                />

                <FormFieldFileSelector<FormValues>
                  id={`${formId}-file`}
                  name="file"
                  label="Select a file to upload"
                  form={form}
                />

                <Button
                  type="submit"
                  disabled={!form.values.file || !form.values.name || uploading}
                >
                  Upload
                </Button>
              </>
            )}
          </Form>
        );
      }}
    />
  );
};

export default InfographicChartForm;
