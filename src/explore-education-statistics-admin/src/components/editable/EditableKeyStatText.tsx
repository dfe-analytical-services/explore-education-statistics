import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import Details from '@common/components/Details';
import useToggle from '@common/hooks/useToggle';
import styles from '@common/modules/find-statistics/components/KeyStat.module.scss';
import KeyStatTile from '@common/modules/find-statistics/components/KeyStatTile';
import React from 'react';
import ReactMarkdown from 'react-markdown';
import EditableKeyStatTextForm from '@admin/components/editable/EditableKeyStatTextForm';
import { KeyStatisticText } from '@common/services/publicationService';

interface KeyStatsFormValues {
  trend: string;
  guidanceTitle: string;
  guidanceText: string;
}

interface EditableKeyStatTextProps {
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
  const [removing, toggleRemoving] = useToggle(false);

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
    <>
      <KeyStatTile
        title={keyStat.title}
        value={keyStat.statistic}
        testId={testId}
        isReordering={isReordering}
      >
        {keyStat.trend && (
          <p className="govuk-body-s" data-testid={`${testId}-summary`}>
            {keyStat.trend}
          </p>
        )}
      </KeyStatTile>

      {keyStat.guidanceTitle && !isReordering && (
        <Details
          summary={keyStat.guidanceTitle}
          className={styles.definition}
          hiddenText={
            keyStat.guidanceTitle === 'Help'
              ? `for ${keyStat.title}`
              : undefined
          }
        >
          <div data-testid={`${testId}-definition`}>
            <ReactMarkdown key={keyStat.guidanceText}>
              {keyStat.guidanceText}
            </ReactMarkdown>
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
