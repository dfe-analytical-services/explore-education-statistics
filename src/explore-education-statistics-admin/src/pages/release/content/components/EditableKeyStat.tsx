import EditableKeyStatDataBlock from '@admin/pages/release/content/components/EditableKeyStatDataBlock';
import EditableKeyStatText from '@admin/pages/release/content/components/EditableKeyStatText';
import useReleaseContentActions from '@admin/pages/release/content/contexts/useReleaseContentActions';
import {
  KeyStatisticDataBlockUpdateRequest,
  KeyStatisticTextUpdateRequest,
} from '@admin/services/keyStatisticService';
import {
  KeyStatistic,
  KeyStatisticType,
} from '@common/services/publicationService';
import React from 'react';

interface EditableKeyStatProps {
  isEditing?: boolean;
  isReordering?: boolean;
  keyStat: KeyStatistic;
  releaseId: string;
  testId?: string;
}

const EditableKeyStat = ({
  isEditing = false,
  isReordering = false,
  keyStat,
  releaseId,
  testId = 'keyStat',
}: EditableKeyStatProps) => {
  const {
    deleteKeyStatistic,
    updateUnattachedDataBlocks,
    updateKeyStatisticDataBlock,
    updateKeyStatisticText,
  } = useReleaseContentActions();

  switch (keyStat.type) {
    case KeyStatisticType.DATABLOCK:
      return (
        <EditableKeyStatDataBlock
          keyStat={keyStat}
          releaseId={releaseId}
          testId={testId}
          isEditing={isEditing}
          isReordering={isReordering}
          onRemove={async () => {
            await deleteKeyStatistic({
              releaseId,
              keyStatisticId: keyStat.id,
            });
            await updateUnattachedDataBlocks({
              releaseId,
            });
          }}
          onSubmit={async values => {
            const request: KeyStatisticDataBlockUpdateRequest = {
              trend: values.trend,
              guidanceTitle: values.guidanceTitle,
              guidanceText: values.guidanceText,
            };

            await updateKeyStatisticDataBlock({
              releaseId,
              keyStatisticId: keyStat.id,
              request,
            });
          }}
        />
      );
    case KeyStatisticType.TEXT:
      return (
        <EditableKeyStatText
          keyStat={keyStat}
          testId={testId}
          isEditing={isEditing}
          isReordering={isReordering}
          onRemove={async () => {
            await deleteKeyStatistic({
              releaseId,
              keyStatisticId: keyStat.id,
            });
          }}
          onSubmit={async values => {
            const request: KeyStatisticTextUpdateRequest = {
              ...values,
            };
            await updateKeyStatisticText({
              releaseId,
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
