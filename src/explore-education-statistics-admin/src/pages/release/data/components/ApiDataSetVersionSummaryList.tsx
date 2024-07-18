import Link from '@admin/components/Link';
import getDataSetVersionStatusTagColour from '@admin/pages/release/data/components/utils/getDataSetVersionStatusColour';
import getDataSetVersionStatusText from '@admin/pages/release/data/components/utils/getDataSetVersionStatusText';
import {
  ReleaseRouteParams,
  releaseSummaryRoute,
} from '@admin/routes/releaseRoutes';
import {
  ApiDataSetDraftVersion,
  ApiDataSetLiveVersion,
} from '@admin/services/apiDataSetService';
import CollapsibleList from '@common/components/CollapsibleList';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import Tag from '@common/components/Tag';
import getTimePeriodString from '@common/modules/table-tool/utils/getTimePeriodString';
import React, { ReactNode } from 'react';
import { generatePath } from 'react-router';

interface ApiDataSetVersionSummaryListProps {
  actions?: ReactNode;
  collapsibleButtonHiddenText?: string;
  dataSetVersion: ApiDataSetDraftVersion | ApiDataSetLiveVersion;
  id: string;
  publicationId: string;
  testId?: string;
}

export default function ApiDataSetVersionSummaryList({
  actions,
  collapsibleButtonHiddenText,
  dataSetVersion,
  id,
  publicationId,
  testId = id,
}: ApiDataSetVersionSummaryListProps) {
  return (
    <SummaryList id={id} testId={testId}>
      <SummaryListItem term="Version">
        <Tag
          colour={getDataSetVersionStatusTagColour(dataSetVersion.status)}
        >{`v${dataSetVersion.version}`}</Tag>
      </SummaryListItem>
      <SummaryListItem term="Status">
        <Tag colour={getDataSetVersionStatusTagColour(dataSetVersion.status)}>
          {getDataSetVersionStatusText(dataSetVersion.status)}
        </Tag>
      </SummaryListItem>
      <SummaryListItem term="Release">
        <Link
          to={generatePath<ReleaseRouteParams>(releaseSummaryRoute.path, {
            releaseVersionId: dataSetVersion.releaseVersion.id,
            publicationId,
          })}
        >
          {dataSetVersion.releaseVersion.title}
        </Link>
      </SummaryListItem>
      <SummaryListItem term="Data set file">
        {dataSetVersion.file.title}
      </SummaryListItem>
      {dataSetVersion.geographicLevels && (
        <SummaryListItem term="Geographic levels">
          {dataSetVersion.geographicLevels.join(', ')}
        </SummaryListItem>
      )}
      {dataSetVersion.timePeriods && (
        <SummaryListItem term="Time periods">
          {getTimePeriodString({
            from: dataSetVersion.timePeriods.start,
            to: dataSetVersion.timePeriods.end,
          })}
        </SummaryListItem>
      )}
      {dataSetVersion.indicators && dataSetVersion.indicators.length > 0 && (
        <SummaryListItem term="Indicators">
          <CollapsibleList
            buttonClassName="govuk-!-margin-bottom-1"
            buttonHiddenText={collapsibleButtonHiddenText}
            collapseAfter={3}
            id={`${id}-indicators`}
            itemName="indicator"
            itemNamePlural="indicators"
            listClassName="govuk-!-margin-top-0 govuk-!-margin-bottom-1"
          >
            {dataSetVersion.indicators.map((indicator, index) => (
              <li key={`indicator-${index.toString()}`}>{indicator}</li>
            ))}
          </CollapsibleList>
        </SummaryListItem>
      )}
      {dataSetVersion.filters && dataSetVersion.filters.length > 0 && (
        <SummaryListItem term="Filters">
          <CollapsibleList
            buttonClassName="govuk-!-margin-bottom-1"
            buttonHiddenText={collapsibleButtonHiddenText}
            collapseAfter={3}
            id={`${id}-filters`}
            itemName="filter"
            itemNamePlural="filters"
            listClassName="govuk-!-margin-top-0 govuk-!-margin-bottom-1"
          >
            {dataSetVersion.filters.map((filter, index) => (
              <li key={`filter-${index.toString()}`}>{filter}</li>
            ))}
          </CollapsibleList>
        </SummaryListItem>
      )}
      {actions ? (
        <SummaryListItem term="Actions">{actions}</SummaryListItem>
      ) : null}
    </SummaryList>
  );
}
