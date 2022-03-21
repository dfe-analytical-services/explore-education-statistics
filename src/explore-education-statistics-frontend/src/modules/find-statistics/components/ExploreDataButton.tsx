import LoadingSpinner from '@common/components/LoadingSpinner';
import useToggle from '@common/hooks/useToggle';
import ButtonLink from '@frontend/components/ButtonLink';
import { DataBlock } from '@common/services/types/blocks';
import { logEvent } from '@frontend/services/googleAnalyticsService';
import React from 'react';

interface Props {
  block: DataBlock;
}

const ExploreDataButton = ({ block }: Props) => {
  const [buttonClicked, toggleButtonClicked] = useToggle(false);

  return (
    <>
      <ButtonLink
        to={`/data-tables/fast-track/${block.id}`}
        disabled={buttonClicked}
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
      </ButtonLink>
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
  );
};

export default ExploreDataButton;
