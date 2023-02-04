import EditableKeyStatPreview from '@admin/pages/release/content/components/EditableKeyStatPreview';
import EditableKeyStatTextForm, {
  KeyStatTextFormValues,
} from '@admin/pages/release/content/components/EditableKeyStatTextForm';
import useToggle from '@common/hooks/useToggle';
import { KeyStatisticText } from '@common/services/publicationService';
import React, { useCallback } from 'react';

export interface EditableKeyStatTextProps {
  isEditing?: boolean;
  isReordering?: boolean;
  keyStat: KeyStatisticText;
  testId?: string;
  onRemove?: () => void;
  onSubmit: (values: KeyStatTextFormValues) => void;
}

export default function EditableKeyStatText({
  isEditing = false,
  isReordering = false,
  keyStat,
  testId = 'keyStat',
  onRemove,
  onSubmit,
}: EditableKeyStatTextProps) {
  const [showForm, toggleShowForm] = useToggle(false);

  const handleSubmit = useCallback(
    async (values: KeyStatTextFormValues) => {
      await onSubmit(values);
      toggleShowForm.off();
    },
    [onSubmit, toggleShowForm],
  );

  if (showForm) {
    return (
      <EditableKeyStatTextForm
        keyStat={keyStat}
        isReordering={isReordering}
        testId={testId}
        onSubmit={handleSubmit}
        onCancel={toggleShowForm.off}
      />
    );
  }

  return (
    <EditableKeyStatPreview
      title={keyStat.title}
      statistic={keyStat.statistic}
      trend={keyStat.trend}
      guidanceTitle={keyStat.guidanceTitle}
      guidanceText={keyStat.guidanceText}
      testId={testId}
      isReordering={isReordering}
      isEditing={isEditing}
      onRemove={onRemove}
      onEdit={toggleShowForm.on}
    />
  );
}
