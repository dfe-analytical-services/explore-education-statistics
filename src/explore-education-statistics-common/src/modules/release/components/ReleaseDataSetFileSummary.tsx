import AccordionToggleButton from '@common/components/AccordionToggleButton';
import CollapsibleList from '@common/components/CollapsibleList';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import VisuallyHidden from '@common/components/VisuallyHidden';
import useToggle from '@common/hooks/useToggle';
import getTimePeriodString from '@common/modules/table-tool/utils/getTimePeriodString';
import { DataSetItem } from '@common/services/publicationService';
import formatPretty from '@common/utils/number/formatPretty';
import classNames from 'classnames';
import React, { ReactNode, useEffect } from 'react';

interface Props {
  dataSetFile: DataSetItem;
  expanded?: boolean;
  renderLink?: ReactNode;
}

export default function ReleaseDataSetFileSummary({
  dataSetFile,
  expanded = false,
  renderLink,
}: Props) {
  const {
    dataSetFileId,
    meta: {
      numDataFileRows = undefined,
      timePeriodRange = {
        start: undefined,
        end: undefined,
      },
      filters = [],
      indicators = [],
    },
    title,
  } = dataSetFile;
  const [showDetails, toggleDetails] = useToggle(false);
  const id = `data-set-file-${dataSetFileId}`;

  useEffect(() => {
    toggleDetails(expanded);
  }, [expanded, toggleDetails]);

  return (
    <div
      className={classNames('govuk-!-margin-top-2', {
        'govuk-!-margin-bottom-2': showDetails,
      })}
    >
      <AccordionToggleButton
        ariaControls={`${id}-details`}
        expanded={showDetails}
        label={
          <>
            {`${showDetails ? 'Hide details' : 'Show more details'}`}
            <VisuallyHidden>{` about ${title}`}</VisuallyHidden>
          </>
        }
        onClick={() => {
          toggleDetails();
        }}
      />
      <SummaryList
        ariaLabel={`Details list for ${title}`}
        className={classNames({
          'govuk-!-margin-bottom-3': showDetails,
          'govuk-!-margin-bottom-0': !showDetails,
        })}
        id={`${id}-details`}
        compact
        noBorder
      >
        {numDataFileRows && (
          <SummaryListItem
            className={classNames({ 'dfe-js-hidden': !showDetails })}
            term="Number of rows"
          >
            {formatPretty(numDataFileRows)}
          </SummaryListItem>
        )}
        {indicators && indicators.length > 0 && (
          <SummaryListItem
            className={classNames({
              'dfe-js-hidden': !showDetails,
            })}
            term="Indicators"
          >
            <CollapsibleList
              buttonClassName="govuk-!-margin-bottom-1"
              buttonHiddenText={`for ${title}`}
              collapseAfter={3}
              id={`${id}-indicators`}
              itemName="indicator"
              itemNamePlural="indicators"
              listClassName="govuk-!-margin-top-0 govuk-!-margin-bottom-1"
              testId="indicators"
            >
              {indicators.map((indicator, index) => (
                <li key={`indicator-${index.toString()}`}>{indicator}</li>
              ))}
            </CollapsibleList>
          </SummaryListItem>
        )}
        {filters && filters.length > 0 && (
          <SummaryListItem
            className={classNames({
              'dfe-js-hidden': !showDetails,
            })}
            term="Filters"
          >
            <CollapsibleList
              buttonClassName="govuk-!-margin-bottom-1"
              buttonHiddenText={`for ${title}`}
              collapseAfter={3}
              id={`${id}-filters`}
              itemName="filter"
              itemNamePlural="filters"
              listClassName="govuk-!-margin-top-0 govuk-!-margin-bottom-1"
              testId="filters"
            >
              {filters.map((filter, index) => (
                <li key={`filter-${index.toString()}`}>{filter}</li>
              ))}
            </CollapsibleList>
          </SummaryListItem>
        )}
        {(timePeriodRange.start || timePeriodRange.end) && (
          <SummaryListItem
            className={classNames({
              'dfe-js-hidden': !showDetails,
            })}
            term="Time period"
          >
            {getTimePeriodString({
              from: timePeriodRange.start,
              to: timePeriodRange.end,
            })}
          </SummaryListItem>
        )}
      </SummaryList>

      {renderLink && (
        <div
          className={classNames({
            'dfe-js-hidden': !showDetails,
          })}
        >
          {renderLink}
        </div>
      )}
    </div>
  );
}
