import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import Details from '@common/components/Details';
import LoadingSpinner from '@common/components/LoadingSpinner';
import WarningMessage from '@common/components/WarningMessage';
import useToggle from '@common/hooks/useToggle';
import styles from '@common/modules/find-statistics/components/KeyStat.module.scss';
import KeyStatTile from '@common/modules/find-statistics/components/KeyStatTile';
import useKeyStatQuery from '@common/modules/find-statistics/hooks/useKeyStatQuery';
import React from 'react';
import ReactMarkdown from 'react-markdown';
import EditableKeyStatDataBlockForm from '@admin/components/editable/EditableKeyStatDataBlockForm';

interface KeyStatsDataFormFormValues {
  trend: string;
  guidanceTitle: string;
  guidanceText: string;
}

interface EditableKeyStatDataBlockProps {
  // NOTE: Cannot accept KeyStatistic as a prop because KeyStatSelectForm keystat preview
  keyStatId: string;
  releaseId: string;
  dataBlockId: string;
  trend?: string;
  guidanceTitle?: string;
  guidanceText?: string;

  isEditing?: boolean;
  isReordering?: boolean;
  onRemove?: () => void;
  onSubmit: (values: KeyStatsDataFormFormValues) => void;
  testId?: string;
}

const EditableKeyStatDataBlock = ({
  isEditing = false,
  isReordering = false,
  releaseId,
  dataBlockId,
  keyStatId,
  trend,
  guidanceTitle = 'Help',
  guidanceText,
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

  if (error) {
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

  const fetchedTitle = dataBlockValues?.title;
  const fetchedStatistic = dataBlockValues?.value;

  if (fetchedTitle === undefined || fetchedStatistic === undefined) {
    return null; // @MarkFix
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
    <>
      <KeyStatTile
        title={fetchedTitle}
        value={fetchedStatistic}
        testId={testId}
        isReordering={isReordering}
      >
        {trend && (
          <p className="govuk-body-s" data-testid={`${testId}-summary`}>
            {trend}
          </p>
        )}
      </KeyStatTile>

      {guidanceTitle && !isReordering && (
        <Details
          summary={guidanceTitle}
          className={styles.definition}
          hiddenText={
            guidanceTitle === 'Help' ? `for ${fetchedTitle}` : undefined
          }
        >
          <div data-testid={`${testId}-definition`}>
            <ReactMarkdown key={guidanceText}>{guidanceText}</ReactMarkdown>
          </div>
        </Details>
      )}

      {isEditing && !isReordering && (
        <ButtonGroup className="govuk-!-margin-top-2">
          <Button onClick={toggleShowForm.on}>Edit</Button>

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
      )}
    </>
  );
};

export default EditableKeyStatDataBlock;
