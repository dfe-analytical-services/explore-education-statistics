import ContentHtml from '@common/components/ContentHtml';
import CollapsibleList from '@common/components/CollapsibleList';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import getTimePeriodString from '@common/modules/table-tool/utils/getTimePeriodString';
import { Subject } from '@common/services/tableBuilderService';
import orderBy from 'lodash/orderBy';
import React from 'react';

interface Props {
  subject: Subject;
}

export default function DataSetDetailsList({ subject }: Props) {
  const { content, filters, geographicLevels, indicators, name } = subject;
  const timePeriod = getTimePeriodString(subject.timePeriods);

  return (
    <SummaryList className="govuk-!-margin-bottom-4" noBorder>
      <SummaryListItem term="Selected dataset">{name}</SummaryListItem>
      {geographicLevels.length > 0 && (
        <SummaryListItem term="Geographic levels">
          {orderBy(geographicLevels).join('; ')}
        </SummaryListItem>
      )}
      {timePeriod && (
        <SummaryListItem term="Time period">{timePeriod}</SummaryListItem>
      )}
      {indicators && indicators.length > 0 && (
        <SummaryListItem term="Indicators">
          <CollapsibleList
            id="indicators"
            collapseAfter={3}
            itemName="indicator"
            itemNamePlural="indicators"
            testId="indicators"
          >
            {indicators.map((indicator, index) => (
              // eslint-disable-next-line react/no-array-index-key
              <li key={index.toString()}>{indicator}</li>
            ))}
          </CollapsibleList>
        </SummaryListItem>
      )}
      {filters && filters.length > 0 && (
        <SummaryListItem term="Filters">
          <CollapsibleList
            id="filters"
            collapseAfter={3}
            itemName="filter"
            itemNamePlural="filters"
            testId="filters"
          >
            {filters.map((filter, index) => (
              // eslint-disable-next-line react/no-array-index-key
              <li key={index.toString()}>{filter}</li>
            ))}
          </CollapsibleList>
        </SummaryListItem>
      )}
      {content && (
        <SummaryListItem term="Content">
          <ContentHtml html={content} />
        </SummaryListItem>
      )}
    </SummaryList>
  );
}
