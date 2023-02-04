import EditableKeyStatDataBlock from '@admin/pages/release/content/components/EditableKeyStatDataBlock';
import EditableKeyStatText from '@admin/pages/release/content/components/EditableKeyStatText';
import useReleaseContentActions from '@admin/pages/release/content/contexts/useReleaseContentActions';
import { KeyStatisticDataBlockUpdateRequest } from '@admin/services/keyStatisticService';
import {
  KeyStatistic,
  KeyStatisticDataBlock,
  KeyStatisticText,
} from '@common/services/publicationService';
import React from 'react';

export interface KeyStatsFormValues {
  trend: string;
  guidanceTitle: string;
  guidanceText: string;
}

interface EditableKeyStatProps {
  keyStat: KeyStatistic;

  isEditing?: boolean;
  isReordering?: boolean;
  onRemove?: () => void;
  testId?: string;
}

const EditableKeyStat = ({
  isEditing = false,
  isReordering = false,
  keyStat,
  testId = 'keyStat',
  onRemove,
}: EditableKeyStatProps) => {
  const { updateKeyStatisticDataBlock } = useReleaseContentActions();
  if ((keyStat as KeyStatisticDataBlock).dataBlockId) {
    return (
      <EditableKeyStatDataBlock
        keyStat={keyStat as KeyStatisticDataBlock}
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
            releaseId: keyStat.releaseId,
            keyStatisticId: keyStat.id,
            request,
          });
        }}
      />
    );
  }

  if (
    (keyStat as KeyStatisticText).title &&
    (keyStat as KeyStatisticText).statistic
  ) {
    return (
      <EditableKeyStatText
        keyStat={keyStat as KeyStatisticText}
        testId={testId}
        isEditing={isEditing}
        isReordering={isReordering}
        onRemove={onRemove}
        // eslint-disable-next-line @typescript-eslint/no-unused-vars
        onSubmit={async values => {
          // EES-3913
        }}
      />
    );
  }

  return null;
};

export default EditableKeyStat;
