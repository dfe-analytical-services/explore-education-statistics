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
  modalTitle = 'Remove this draft API data set version',
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
      confirmText="Remove this API data set version"
      submitButtonVariant="warning"
      title={modalTitle}
      triggerButton={<ButtonText variant="warning">{children}</ButtonText>}
      hiddenConfirmingText="Removing draft version"
      onConfirm={handleConfirm}
    >
      <p>Are you sure you want to remove the selected API data set version?</p>
      <p>
        <strong>{dataSet.title}</strong>
      </p>
      <p>
        Please note this doesn't affect the current live API data set in any
        way. You can reassign a data set version at any time prior to this
        release being published.
      </p>
    </ModalConfirm>
  );
}
