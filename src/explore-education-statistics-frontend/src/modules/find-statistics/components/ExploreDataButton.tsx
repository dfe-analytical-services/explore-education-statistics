import LoadingSpinner from '@common/components/LoadingSpinner';
import useToggle from '@common/hooks/useToggle';
import ButtonLink from '@frontend/components/ButtonLink';
import { DataBlock } from '@common/services/types/blocks';
import { logEvent } from '@frontend/services/googleAnalyticsService';
import React from 'react';
import VisuallyHidden from '@common/components/VisuallyHidden';
import Button from '@common/components/Button';

interface Props {
  block: DataBlock;
  hiddenText?: string;
}

const ExploreDataButton = ({ block, hiddenText }: Props) => {
  const [buttonClicked, toggleButtonClicked] = useToggle(false);
  return (
    <>
      {buttonClicked ? (
        <>
          <Button disabled>
            Explore data
            {hiddenText && <VisuallyHidden>{` ${hiddenText}`}</VisuallyHidden>}
          </Button>
          <LoadingSpinner
            alert
            inline
            hideText
            loading={buttonClicked}
            size="md"
            text="Loading data"
            className="govuk-!-margin-left-2"
          />
        </>
      ) : (
        <ButtonLink
          to={`/data-tables/fast-track/${block.dataBlockParentId}`}
          onClick={() => {
            toggleButtonClicked.on();
            logEvent({
              category: `Publication Release Data Tabs`,
              action: `Explore data button clicked`,
              label: `Explore data block name: ${block.name}`,
            });
          }}
        >
          Explore data
          {hiddenText && <VisuallyHidden>{` ${hiddenText}`}</VisuallyHidden>}
        </ButtonLink>
      )}
    </>
  );
};

export default ExploreDataButton;
