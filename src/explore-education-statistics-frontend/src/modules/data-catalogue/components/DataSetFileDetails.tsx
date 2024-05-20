import ButtonText from '@common/components/ButtonText';
import DataSetFilePageSection from '@frontend/modules/data-catalogue/components/DataSetFilePageSection';
import { pageSections } from '@frontend/modules/data-catalogue/DataSetFilePage';
import Link from '@frontend/components/Link';
import SummaryListItem from '@common/components/SummaryListItem';
import CollapsibleList from '@common/components/CollapsibleList';
import SummaryList from '@common/components/SummaryList';
import { DataSetFile } from '@frontend/services/dataSetFileService';
import Modal from '@common/components/Modal';
import { releaseTypes } from '@common/services/types/releaseType';
import InfoIcon from '@common/components/InfoIcon';
import ReleaseTypeSection from '@common/modules/release/components/ReleaseTypeSection';
import getTimePeriodString from '@common/modules/table-tool/utils/getTimePeriodString';
import orderBy from 'lodash/orderBy';
import React from 'react';

interface Props {
  dataSetFile: DataSetFile;
}

export default function DataSetFileDetails({ dataSetFile }: Props) {
  const {
    release,
    meta: { timePeriod, filters, geographicLevels, indicators },
    title,
  } = dataSetFile;

  return (
    <DataSetFilePageSection
      heading={pageSections.dataSetDetails}
      id="dataSetDetails"
    >
      <SummaryList noBorder>
        <SummaryListItem term="Theme">
          {release.publication.themeTitle}
        </SummaryListItem>
        <SummaryListItem term="Publication">
          {release.publication.title}
        </SummaryListItem>
        <SummaryListItem term="Release">
          <Link
            to={`/find-statistics/${release.publication.slug}/${release.slug}`}
          >
            {release.title}
          </Link>
        </SummaryListItem>
        <SummaryListItem term="Release type">
          <Modal
            showClose
            title={releaseTypes[release.type]}
            triggerButton={
              <ButtonText>
                {releaseTypes[release.type]}{' '}
                <InfoIcon
                  description={`Information on ${releaseTypes[release.type]}`}
                />
              </ButtonText>
            }
          >
            <ReleaseTypeSection showHeading={false} type={release.type} />
          </Modal>
        </SummaryListItem>

        {geographicLevels && geographicLevels.length > 0 && (
          <SummaryListItem term="Geographic levels">
            {orderBy(geographicLevels).join(', ')}
          </SummaryListItem>
        )}
        {indicators && indicators.length > 0 && (
          <SummaryListItem term="Indicators">
            <CollapsibleList
              buttonClassName="govuk-!-margin-bottom-1"
              buttonHiddenText={`for ${title}`}
              collapseAfter={3}
              id="indicators"
              itemName="indicator"
              itemNamePlural="indicators"
              listClassName="govuk-!-margin-top-0 govuk-!-margin-bottom-1"
            >
              {indicators.map((indicator, index) => (
                <li key={`indicator-${index.toString()}`}>{indicator}</li>
              ))}
            </CollapsibleList>
          </SummaryListItem>
        )}
        {filters && filters.length > 0 && (
          <SummaryListItem term="Filters">
            <CollapsibleList
              buttonClassName="govuk-!-margin-bottom-1"
              buttonHiddenText={`for ${title}`}
              collapseAfter={3}
              id="filters"
              itemName="filter"
              itemNamePlural="filters"
              listClassName="govuk-!-margin-top-0 govuk-!-margin-bottom-1"
            >
              {filters.map((filter, index) => (
                <li key={`filter-${index.toString()}`}>{filter}</li>
              ))}
            </CollapsibleList>
          </SummaryListItem>
        )}
        {timePeriod && (timePeriod.from || timePeriod.to) && (
          <SummaryListItem term="Time period">
            {getTimePeriodString(timePeriod)}
          </SummaryListItem>
        )}
      </SummaryList>
    </DataSetFilePageSection>
  );
}
