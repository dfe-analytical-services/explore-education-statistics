import useToggle from '@common/hooks/useToggle';
import React from 'react';
import EditableKeyStatTextForm from '@admin/components/editable/EditableKeyStatTextForm';
import { KeyStatisticText } from '@common/services/publicationService';
import EditableKeyStatDisplay from '@admin/components/editable/EditableKeyStatDisplay';

interface KeyStatsFormValues {
  trend: string;
  guidanceTitle: string;
  guidanceText: string;
}

export interface EditableKeyStatTextProps {
  keyStat: KeyStatisticText;
  isEditing?: boolean;
  isReordering?: boolean;
  onRemove?: () => void;
  onSubmit: (values: KeyStatsFormValues) => void;
  testId?: string;
}

const EditableKeyStatText = ({
  isEditing = false,
  isReordering = false,
  keyStat,
  testId = 'keyStat',
  onRemove,
  onSubmit,
}: EditableKeyStatTextProps) => {
  const [showForm, toggleShowForm] = useToggle(false);

  if (showForm) {
    return (
      <EditableKeyStatTextForm
        keyStat={keyStat}
        isReordering={isReordering}
        onSubmit={onSubmit}
        toggleShowFormOff={toggleShowForm.off}
        testId={testId}
      />
    );
  }

  return (
    <EditableKeyStatDisplay
      title={keyStat.title}
      statistic={keyStat.statistic}
      trend={keyStat.trend}
      guidanceTitle={keyStat.guidanceTitle ?? 'Help'}
      guidanceText={keyStat.guidanceText}
      testId={testId}
      isReordering={isReordering}
      isEditing={isEditing}
      onRemove={onRemove}
      toggleShowForm={toggleShowForm}
    />
  );
};

export default EditableKeyStatText;
