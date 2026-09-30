import { ImporterStatusChangeHandler } from '@admin/pages/release/data/components/ImporterStatus';
import { DataFile } from '@admin/services/releaseDataFileService';
import ButtonText from '@common/components/ButtonText';
import Modal from '@common/components/Modal';
import VisuallyHidden from '@common/components/VisuallyHidden';
import React from 'react';
import DataFileSummaryList from './DataFileSummaryList';

interface Props {
  dataFile: DataFile;
  releaseVersionId: string;
  onStatusChange: ImporterStatusChangeHandler;
}

export default function DataFileDetailsModal({
  dataFile,
  releaseVersionId,
  onStatusChange,
}: Props) {
  return (
    <Modal
      showClose
      title="Data file details"
      triggerButton={
        <ButtonText>
          View details
          <VisuallyHidden>{` for ${dataFile.title}`}</VisuallyHidden>
        </ButtonText>
      }
    >
      <DataFileSummaryList
        dataFile={dataFile}
        releaseVersionId={releaseVersionId}
        onStatusChange={onStatusChange}
      />
    </Modal>
  );
}
