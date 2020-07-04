import useFormSubmit from '@admin/hooks/useFormSubmit';
import releaseChartFileService, {
  ChartFile,
} from '@admin/services/releaseChartFileService';
import Button from '@common/components/Button';
import Form from '@common/components/form/Form';
import FormFieldFileInput from '@common/components/form/FormFieldFileInput';
import ModalConfirm from '@common/components/ModalConfirm';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import useToggle from '@common/hooks/useToggle';
import { mapFieldErrors } from '@common/validation/serverValidations';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import React, { useState } from 'react';

interface FormValues {
  file: File | null;
  fileId: string;
}

const errorMappings = [
  mapFieldErrors<FormValues>({
    target: 'file',
    messages: {
      FILE_TYPE_INVALID: 'Choose an image file',
      CANNOT_OVERWRITE_FILE: 'Choose a unique file',
    },
  }),
];

interface Props {
  canSaveChart: boolean;
  releaseId: string;
  fileId?: string;
  subjectName: string;
  onSubmit: (fileId: string) => void;
  onDelete: (fileId: string) => void;
}

const formId = 'fileUploadForm';

const InfographicChartForm = ({
  canSaveChart,
  releaseId,
  fileId = '',
  subjectName,
  onDelete,
  onSubmit,
}: Props) => {
  const [uploading, setUploading] = useState(false);
  const [deleteFile, toggleDeleteFile] = useToggle(false);

  const { value: files = [], retry: refreshFiles } = useAsyncRetry<ChartFile[]>(
    () => releaseChartFileService.getChartFiles(releaseId),
    [releaseId],
  );

  const handleSubmit = useFormSubmit<FormValues>(async values => {
    if (values.file) {
      setUploading(true);

      try {
        await releaseChartFileService.uploadChartFile(releaseId, {
          file: values.file as File,
        });
        await refreshFiles();

        onSubmit((values.file as File).name.toLowerCase());
      } finally {
        setUploading(false);
      }
    }
  }, errorMappings);

  const selectedFile = files.find(fileOption => fileOption.filename === fileId);

  return (
    <Formik<FormValues>
      enableReinitialize
      initialValues={{
        file: null,
        fileId: fileId || '',
      }}
      validationSchema={Yup.object<FormValues>({
        file: Yup.mixed().required('Choose a file'),
        fileId: Yup.string(),
      })}
      onSubmit={handleSubmit}
    >
      {form => {
        return (
          <Form id={formId}>
            {fileId && selectedFile && (
              <>
                <SummaryList>
                  <SummaryListItem term="Filename">{fileId}</SummaryListItem>
                </SummaryList>

                <Button
                  disabled={!canSaveChart}
                  variant="warning"
                  onClick={toggleDeleteFile.on}
                >
                  Delete infographic
                </Button>

                <ModalConfirm
                  mounted={deleteFile}
                  title="Confirm deletion of infographic"
                  onExit={toggleDeleteFile.off}
                  onCancel={toggleDeleteFile.off}
                  onConfirm={async () => {
                    await releaseChartFileService.deleteChartFile(
                      releaseId,
                      subjectName,
                      form.values.fileId,
                    );
                    await refreshFiles();

                    onDelete(form.values.fileId);
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
                <FormFieldFileInput<FormValues>
                  id={`${formId}-file`}
                  name="file"
                  label="Select a file to upload"
                />

                <Button type="submit" disabled={!form.values.file || uploading}>
                  Upload
                </Button>
              </>
            )}
          </Form>
        );
      }}
    </Formik>
  );
};

export default InfographicChartForm;
