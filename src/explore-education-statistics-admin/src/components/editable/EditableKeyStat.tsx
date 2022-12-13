import React from 'react';
import EditableKeyStatDataBlock from '@admin/components/editable/EditableKeyStatDataBlock';
import EditableKeyStatText from '@admin/components/editable/EditableKeyStatText';

export interface KeyStatsFormValues {
  trend: string;
  guidanceTitle: string;
  guidanceText: string;
}

interface EditableKeyStatProps {
  releaseId?: string;
  dataBlockId?: string;
  keyStatId: string;
  title?: string;
  statistic?: string;
  trend?: string;
  guidanceTitle?: string;
  guidanceText?: string;

  isEditing?: boolean;
  isReordering?: boolean;
  onRemove?: () => void;
  testId?: string;
}

const EditableKeyStat = ({
  isEditing = false,
  isReordering = false,
  releaseId,
  dataBlockId,
  keyStatId,
  title,
  statistic,
  trend,
  guidanceTitle = 'Help',
  guidanceText,
  testId = 'keyStat',
  onRemove,
}: EditableKeyStatProps) => {
  if (dataBlockId && releaseId) {
    return (
      <EditableKeyStatDataBlock
        releaseId={releaseId}
        dataBlockId={dataBlockId}
        keyStatId={keyStatId}
        trend={trend}
        guidanceTitle={guidanceTitle}
        guidanceText={guidanceText}
        testId={testId}
        isEditing={isEditing}
        isReordering={isReordering}
        onRemove={onRemove}
        onSubmit={async values => {
          // @MarkFix call keyStatisticService.Update here
          // previously used useReleaseContentActions#updateContentSectionDataBlock
        }}
      />
    );
  }

  if (title && statistic) {
    return (
      <EditableKeyStatText
        keyStatId={keyStatId}
        title={title}
        statistic={statistic}
        trend={trend}
        guidanceTitle={guidanceTitle}
        guidanceText={guidanceText}
        testId={testId}
        isEditing={isEditing}
        isReordering={isReordering}
        onRemove={onRemove}
        onSubmit={async values => {
          // @MarkFix call keyStatisticService.Update here
        }}
      />
    );
  }

  return null; // @MarkFix throw error or something
};

export default EditableKeyStat;
