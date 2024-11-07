import EditableKeyStatDataBlockForm, {
  KeyStatDataBlockFormValues,
} from '@admin/pages/release/content/components/EditableKeyStatDataBlockForm';
import EditableKeyStatPreview from '@admin/pages/release/content/components/EditableKeyStatPreview';
import Button from '@common/components/Button';
import LoadingSpinner from '@common/components/LoadingSpinner';
import VisuallyHidden from '@common/components/VisuallyHidden';
import WarningMessage from '@common/components/WarningMessage';
import useToggle from '@common/hooks/useToggle';
import tableBuilderQueries from '@common/modules/find-statistics/queries/tableBuilderQueries';
import { KeyStatisticDataBlock } from '@common/services/publicationService';
import { useQuery } from '@tanstack/react-query';
import React, { useCallback } from 'react';

export interface EditableKeyStatDataBlockProps {
  isEditing?: boolean;
  isReordering?: boolean;
  keyStat: KeyStatisticDataBlock;
  keyStatisticGuidanceTitles?: (string | undefined)[];
  releaseId: string;
  testId?: string;
  onRemove?: () => void;
  onSubmit?: (values: KeyStatDataBlockFormValues) => void;
}

export default function EditableKeyStatDataBlock({
  isEditing = false,
  isReordering = false,
  keyStat,
  keyStatisticGuidanceTitles,
  releaseId,
  testId = 'keyStat',
  onRemove,
  onSubmit,
}: EditableKeyStatDataBlockProps) {
  const [showForm, toggleShowForm] = useToggle(false);

  const {
    data: dataBlockValues,
    isLoading,
    error,
  } = useQuery(
    tableBuilderQueries.getKeyStat(releaseId, keyStat.dataBlockParentId),
  );

  const handleSubmit = useCallback(
    async (values: KeyStatDataBlockFormValues) => {
      await onSubmit?.(values);
      toggleShowForm.off();
    },
    [onSubmit, toggleShowForm],
  );

  if (isLoading) {
    return <LoadingSpinner />;
  }

  const title = dataBlockValues?.title;
  const statistic = dataBlockValues?.value;

  if (error || !title || !statistic) {
    return (
      <>
        <WarningMessage>Could not load key statistic</WarningMessage>

        {onRemove && (
          <Button variant="secondary" onClick={onRemove}>
            Remove <VisuallyHidden> key statistic</VisuallyHidden>
          </Button>
        )}
      </>
    );
  }

  if (showForm) {
    return (
      <EditableKeyStatDataBlockForm
        keyStat={keyStat}
        keyStatisticGuidanceTitles={keyStatisticGuidanceTitles?.filter(
          keyStatTitle => keyStatTitle === keyStat.guidanceTitle,
        )}
        title={title}
        statistic={statistic}
        testId={testId}
        onSubmit={handleSubmit}
        onCancel={toggleShowForm.off}
      />
    );
  }

  return (
    <EditableKeyStatPreview
      title={title}
      statistic={statistic}
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
