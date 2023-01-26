import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import LoadingSpinner from '@common/components/LoadingSpinner';
import WarningMessage from '@common/components/WarningMessage';
import useToggle from '@common/hooks/useToggle';
import useKeyStatQuery from '@common/modules/find-statistics/hooks/useKeyStatQuery';
import React from 'react';
import EditableKeyStatDataBlockForm from '@admin/components/editable/EditableKeyStatDataBlockForm';
import { KeyStatisticDataBlock } from '@common/services/publicationService';
import EditableKeyStatDisplay from '@admin/components/editable/EditableKeyStatDisplay';

interface KeyStatsDataFormValues {
  trend: string;
  guidanceTitle: string;
  guidanceText: string;
}

export interface EditableKeyStatDataBlockProps {
  keyStat: KeyStatisticDataBlock;
  isEditing?: boolean;
  isReordering?: boolean;
  onRemove?: () => void;
  onSubmit: (values: KeyStatsDataFormValues) => void;
  testId?: string;
}

const EditableKeyStatDataBlock = ({
  keyStat: {
    id: keyStatId,
    releaseId,
    dataBlockId,
    trend,
    guidanceTitle = 'Help',
    guidanceText,
  },
  isEditing = false,
  isReordering = false,
  testId = 'keyStat',
  onRemove,
  onSubmit,
}: EditableKeyStatDataBlockProps) => {
  const [showForm, toggleShowForm] = useToggle(false);
  const [removing, toggleRemoving] = useToggle(false);

  const { value: dataBlockValues, isLoading, error } = useKeyStatQuery(
    releaseId,
    dataBlockId,
  );

  if (isLoading) {
    return <LoadingSpinner />;
  }

  const fetchedTitle = dataBlockValues?.title;
  const fetchedStatistic = dataBlockValues?.value;

  if (error || !fetchedTitle || !fetchedStatistic) {
    return (
      <>
        <WarningMessage>Could not load key statistic</WarningMessage>

        <ButtonGroup>
          {onRemove && (
            <Button
              disabled={removing}
              variant="secondary"
              onClick={() => {
                toggleRemoving.on();
                onRemove();
              }}
            >
              Remove
            </Button>
          )}
        </ButtonGroup>
      </>
    );
  }

  if (showForm) {
    return (
      <EditableKeyStatDataBlockForm
        keyStatId={keyStatId}
        title={fetchedTitle}
        statistic={fetchedStatistic}
        trend={trend}
        guidanceTitle={guidanceTitle}
        guidanceText={guidanceText}
        isReordering={isReordering}
        onSubmit={onSubmit}
        toggleShowFormOff={toggleShowForm.off}
        testId={testId}
      />
    );
  }

  return (
    <EditableKeyStatDisplay
      title={fetchedTitle}
      statistic={fetchedStatistic}
      trend={trend}
      guidanceTitle={guidanceTitle}
      guidanceText={guidanceText}
      testId={testId}
      isReordering={isReordering}
      isEditing={isEditing}
      onRemove={onRemove}
      toggleShowForm={toggleShowForm}
    />
  );
};

export default EditableKeyStatDataBlock;
