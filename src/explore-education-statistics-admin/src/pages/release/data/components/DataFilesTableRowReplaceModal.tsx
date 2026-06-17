import ButtonText from '@common/components/ButtonText';
import VisuallyHidden from '@common/components/VisuallyHidden';
import DataFileUploadForm from '@admin/pages/release/data/components/DataFileUploadForm';
import Modal from '@common/components/Modal';
import React from 'react';
import useToggle from '@common/hooks/useToggle';
import { useQuery } from '@tanstack/react-query';
import releaseDataFileQueries from '@admin/queries/releaseDataFileQueries';
import LoadingSpinner from '@common/components/LoadingSpinner';

interface Props {
  releaseVersionId: string;
  dataFileId: string;
  dataFileTitle: string;
  onReplaceFile: () => void;
}

export default function DataFilesTableRowReplaceModal({
  releaseVersionId,
  dataFileId,
  dataFileTitle,
  onReplaceFile,
}: Props) {
  const [open, toggleOpen] = useToggle(false);
  const { data: dataFile, isLoading } = useQuery({
    ...releaseDataFileQueries.getDataFile(releaseVersionId, dataFileId),
    enabled: open,
  });

  return (
    <Modal
      open={open}
      title="Replace data"
      triggerButton={
        <ButtonText onClick={toggleOpen.on}>
          Replace data
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
            <DataFileUploadForm
              isDataReplacement
              releaseVersionId={releaseVersionId}
              dataFileTitle={dataFile.title}
              hideFormFields={dataFile.replacementInProgress}
              onCancel={toggleOpen.off}
              onSubmit={async () => {
                toggleOpen.off();
                onReplaceFile();
              }}
            />
          </>
        )}
      </LoadingSpinner>
    </Modal>
  );
}
