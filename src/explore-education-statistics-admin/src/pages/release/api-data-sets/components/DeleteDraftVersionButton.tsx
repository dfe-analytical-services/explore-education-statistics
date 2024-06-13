import apiDataSetQueries from '@admin/queries/apiDataSetQueries';
import {
  ApiDataSet,
  ApiDataSetVersion,
} from '@admin/services/apiDataSetService';
import apiDataSetVersionService from '@admin/services/apiDataSetVersionService';
import ButtonText from '@common/components/ButtonText';
import ModalConfirm from '@common/components/ModalConfirm';
import { useQueryClient } from '@tanstack/react-query';
import React, { ReactNode, useCallback } from 'react';

export interface DeleteDraftVersionButtonProps {
  children: ReactNode;
  dataSet: Pick<ApiDataSet, 'id' | 'title'>;
  dataSetVersion: Pick<ApiDataSetVersion, 'id' | 'version'>;
  modalTitle?: string;
  onDeleted?: () => void;
}

export default function DeleteDraftVersionButton({
  children,
  dataSet,
  dataSetVersion,
  modalTitle = 'Remove draft version',
  onDeleted,
}: DeleteDraftVersionButtonProps) {
  const queryClient = useQueryClient();

  const handleConfirm = useCallback(async () => {
    await apiDataSetVersionService.deleteVersion(dataSetVersion.id);

    onDeleted?.();

    queryClient.removeQueries({
      queryKey: apiDataSetQueries.get(dataSet.id).queryKey,
    });
    await queryClient.invalidateQueries({
      queryKey: apiDataSetQueries.list._def,
    });
  }, [dataSet.id, dataSetVersion.id, onDeleted, queryClient]);

  return (
    <ModalConfirm
      title={modalTitle}
      triggerButton={<ButtonText variant="warning">{children}</ButtonText>}
      hiddenConfirmingText="Removing draft version"
      onConfirm={handleConfirm}
    >
      <p data-testid="confirm-text">
        Confirm that you want to delete the draft version{' '}
        <strong>{dataSetVersion.version}</strong> for API data set: <br />
        <strong>{dataSet.title}</strong>
      </p>
    </ModalConfirm>
  );
}
