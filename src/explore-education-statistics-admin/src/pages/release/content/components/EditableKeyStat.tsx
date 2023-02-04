import EditableKeyStatDataBlock from '@admin/pages/release/content/components/EditableKeyStatDataBlock';
import EditableKeyStatText from '@admin/pages/release/content/components/EditableKeyStatText';
import useReleaseContentActions from '@admin/pages/release/content/contexts/useReleaseContentActions';
import { KeyStatisticDataBlockUpdateRequest } from '@admin/services/keyStatisticService';
import { KeyStatistic } from '@common/services/publicationService';
import React from 'react';

interface EditableKeyStatProps {
  isEditing?: boolean;
  isReordering?: boolean;
  keyStat: KeyStatistic;
  releaseId: string;
  testId?: string;
  onRemove?: () => void;
}

const EditableKeyStat = ({
  isEditing = false,
  isReordering = false,
  keyStat,
  releaseId,
  testId = 'keyStat',
  onRemove,
}: EditableKeyStatProps) => {
  const { updateKeyStatisticDataBlock } = useReleaseContentActions();

  if (keyStat.type === 'KeyStatisticDataBlock') {
    return (
      <EditableKeyStatDataBlock
        keyStat={keyStat}
        releaseId={releaseId}
        testId={testId}
        isEditing={isEditing}
        isReordering={isReordering}
        onRemove={onRemove}
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
  }

  if (keyStat.type === 'KeyStatisticText') {
    return (
      <EditableKeyStatText
        keyStat={keyStat}
        testId={testId}
        isEditing={isEditing}
        isReordering={isReordering}
        onRemove={onRemove}
        // eslint-disable-next-line @typescript-eslint/no-unused-vars
        onSubmit={async values => {
          // TODO: EES-3913
        }}
      />
    );
  }

  return null;
};

export default EditableKeyStat;
