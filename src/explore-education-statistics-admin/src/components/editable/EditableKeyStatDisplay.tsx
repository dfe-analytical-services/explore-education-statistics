import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import Details from '@common/components/Details';
import useToggle, { Toggle } from '@common/hooks/useToggle';
import styles from '@common/modules/find-statistics/components/KeyStat.module.scss';
import KeyStatTile from '@common/modules/find-statistics/components/KeyStatTile';
import React from 'react';
import ReactMarkdown from 'react-markdown';

export interface EditableKeyStatDisplayProps {
  title: string;
  statistic: string;
  trend?: string;
  guidanceTitle?: string;
  guidanceText?: string;
  testId: string;
  isReordering: boolean;
  isEditing: boolean;
  onRemove?: () => void;
  toggleShowForm: Toggle;
}

const EditableKeyStatDisplay = ({
  title,
  statistic,
  trend,
  guidanceTitle,
  guidanceText,
  testId,
  isReordering,
  isEditing,
  onRemove,
  toggleShowForm,
}: EditableKeyStatDisplayProps) => {
  const [removing, toggleRemoving] = useToggle(false);

  return (
    <>
      <KeyStatTile
        title={title}
        value={statistic}
        testId={testId}
        isReordering={isReordering}
      >
        {trend && (
          <p className="govuk-body-s" data-testid={`${testId}-trend`}>
            {trend}
          </p>
        )}
      </KeyStatTile>

      {guidanceTitle && guidanceText && !isReordering && (
        <Details
          summary={guidanceTitle}
          className={styles.guidanceTitle}
          hiddenText={guidanceTitle === 'Help' ? `for ${title}` : undefined}
        >
          <div data-testid={`${testId}-guidanceText`}>
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

export default EditableKeyStatDisplay;
