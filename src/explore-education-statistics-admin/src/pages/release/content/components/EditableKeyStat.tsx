import EditableKeyStatDataBlock from '@admin/pages/release/content/components/EditableKeyStatDataBlock';
import EditableKeyStatText from '@admin/pages/release/content/components/EditableKeyStatText';
import useReleaseContentActions from '@admin/pages/release/content/contexts/useReleaseContentActions';
import {
  KeyStatisticDataBlockUpdateRequest,
  KeyStatisticTextUpdateRequest,
} from '@admin/services/keyStatisticService';
import { KeyStatistic } from '@common/services/publicationService';
import React from 'react';

interface EditableKeyStatProps {
  isEditing?: boolean;
  isReordering?: boolean;
  keyStat: KeyStatistic;
  releaseVersionId: string;
  testId?: string;
}

const EditableKeyStat = ({
  isEditing = false,
  isReordering = false,
  keyStat,
  releaseVersionId,
  testId = 'keyStat',
}: EditableKeyStatProps) => {
  const {
    deleteKeyStatistic,
    updateUnattachedDataBlocks,
    updateKeyStatisticDataBlock,
    updateKeyStatisticText,
  } = useReleaseContentActions();

  switch (keyStat.type) {
    case 'KeyStatisticDataBlock':
      return (
        <EditableKeyStatDataBlock
          keyStat={keyStat}
          releaseVersionId={releaseVersionId}
          testId={testId}
          isEditing={isEditing}
          isReordering={isReordering}
          onRemove={async () => {
            await deleteKeyStatistic({
              releaseVersionId,
              keyStatisticId: keyStat.id,
            });
            await updateUnattachedDataBlocks({
              releaseVersionId,
            });
          }}
          onSubmit={async values => {
            const request: KeyStatisticDataBlockUpdateRequest = {
              ...values,
            };

            await updateKeyStatisticDataBlock({
              releaseVersionId,
              keyStatisticId: keyStat.id,
              request,
            });
          }}
        />
      );
    case 'KeyStatisticText':
      return (
        <EditableKeyStatText
          keyStat={keyStat}
          testId={testId}
          isEditing={isEditing}
          isReordering={isReordering}
          onRemove={async () => {
            await deleteKeyStatistic({
              releaseVersionId,
              keyStatisticId: keyStat.id,
            });
          }}
          onSubmit={async values => {
            const request: KeyStatisticTextUpdateRequest = {
              ...values,
            };
            await updateKeyStatisticText({
              releaseVersionId,
              keyStatisticId: keyStat.id,
              request,
            });
          }}
        />
      );
    default:
      return null;
  }
};

export default EditableKeyStat;
