import releaseDataFileQueries from '@admin/queries/releaseDataFileQueries';
import ButtonText from '@common/components/ButtonText';
import LoadingSpinner from '@common/components/LoadingSpinner';
import VisuallyHidden from '@common/components/VisuallyHidden';
import useToggle from '@common/hooks/useToggle';
import Modal from '@common/components/Modal';
import Button from '@common/components/Button';
import Yup from '@common/validation/yup';
import Form from '@common/components/form/Form';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import FormProvider from '@common/components/form/FormProvider';
import releaseDataFileService from '@admin/services/releaseDataFileService';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import React from 'react';
import ButtonGroup from '@common/components/ButtonGroup';

const titleMaxLength = 120;

interface Props {
  releaseVersionId: string;
  dataFileId: string;
  dataFileTitle: string;
  onConfirm: () => void;
}

interface FormValues {
  title: string;
}

export default function DataFilesTableRowEditTitleModal({
  releaseVersionId,
  dataFileId,
  dataFileTitle,
  onConfirm,
}: Props) {
  const [open, toggleOpen] = useToggle(false);
  const { data: dataFile, isLoading } = useQuery({
    ...releaseDataFileQueries.getDataFile(releaseVersionId, dataFileId),
    enabled: open,
  });

  const queryClient = useQueryClient();

  const handleSubmit = async (values: FormValues) => {
    await releaseDataFileService.updateFile(releaseVersionId, dataFileId, {
      title: values.title,
    });

    queryClient.removeQueries({
      queryKey: releaseDataFileQueries.list(releaseVersionId).queryKey,
    });
    queryClient.removeQueries({
      queryKey: releaseDataFileQueries.listUploads(releaseVersionId).queryKey,
    });
    await queryClient.invalidateQueries({
      queryKey: releaseDataFileQueries.list._def,
    });
    await queryClient.invalidateQueries({
      queryKey: releaseDataFileQueries.listUploads._def,
    });
    toggleOpen.off();
    onConfirm();
  };

  return (
    <Modal
      open={open}
      className="govuk-!-width-two-thirds"
      title="Edit title"
      triggerButton={
        <ButtonText onClick={toggleOpen.on}>
          Edit title
          <VisuallyHidden>{` for ${dataFileTitle}`}</VisuallyHidden>
        </ButtonText>
      }
    >
      <LoadingSpinner loading={isLoading}>
        {dataFile && (
          <>
            {dataFile.replacementInProgress && (
              <>
                <p>This data file is currently being replaced.</p>
                <p>Cancel the replacement first.</p>
              </>
            )}
            <FormProvider
              initialValues={{ title: dataFileTitle }}
              validationSchema={Yup.object<FormValues>({
                title: Yup.string()
                  .required('Enter a title')
                  .max(
                    titleMaxLength,
                    `Title must be ${titleMaxLength} characters or fewer`,
                  ),
              })}
            >
              <Form id="dataFileForm" onSubmit={handleSubmit}>
                {!dataFile.replacementInProgress && (
                  <FormFieldTextInput<FormValues>
                    className="govuk-!-width-full"
                    label="Title"
                    name="title"
                    maxLength={titleMaxLength}
                    disabled={dataFile.replacementInProgress}
                  />
                )}
                <ButtonGroup>
                  <Button
                    type="submit"
                    disabled={dataFile.replacementInProgress}
                  >
                    Save changes
                  </Button>
                  <ButtonText onClick={toggleOpen.off}>Cancel</ButtonText>
                </ButtonGroup>
              </Form>
            </FormProvider>
          </>
        )}
      </LoadingSpinner>
    </Modal>
  );
}
