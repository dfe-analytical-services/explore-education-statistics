import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import Details from '@common/components/Details';
import useToggle from '@common/hooks/useToggle';
import styles from '@common/modules/find-statistics/components/KeyStat.module.scss';
import KeyStatTile from '@common/modules/find-statistics/components/KeyStatTile';
import React from 'react';
import ReactMarkdown from 'react-markdown';
import EditableKeyStatTextForm from '@admin/components/editable/EditableKeyStatTextForm';

interface KeyStatsFormValues {
  trend: string;
  guidanceTitle: string;
  guidanceText: string;
}

interface EditableKeyStatTextProps {
  keyStatId: string;
  title: string;
  statistic: string;
  trend?: string;
  guidanceTitle?: string;
  guidanceText?: string;
  isEditing?: boolean;
  isReordering?: boolean;
  onRemove?: () => void;
  onSubmit: (values: KeyStatsFormValues) => void;
  testId?: string;
}

const EditableKeyStatText = ({
  isEditing = false,
  isReordering = false,
  keyStatId,
  title,
  statistic,
  trend,
  guidanceTitle = 'Help',
  guidanceText,
  testId = 'keyStat',
  onRemove,
  onSubmit,
}: EditableKeyStatTextProps) => {
  const [showForm, toggleShowForm] = useToggle(false);
  const [removing, toggleRemoving] = useToggle(false);

  if (showForm) {
    return (
      <EditableKeyStatTextForm
        keyStatId={keyStatId}
        title={title}
        statistic={statistic}
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
        title={title}
        value={statistic}
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
          hiddenText={guidanceTitle === 'Help' ? `for ${title}` : undefined}
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

export default EditableKeyStatText;
