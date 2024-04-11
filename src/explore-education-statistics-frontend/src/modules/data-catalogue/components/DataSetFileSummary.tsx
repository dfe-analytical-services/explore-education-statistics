import AccordionToggleButton from '@common/components/AccordionToggleButton';
import ButtonText from '@common/components/ButtonText';
import CollapsibleList from '@common/components/CollapsibleList';
import ContentHtml from '@common/components/ContentHtml';
import FormattedDate from '@common/components/FormattedDate';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import Tag from '@common/components/Tag';
import VisuallyHidden from '@common/components/VisuallyHidden';
import useToggle from '@common/hooks/useToggle';
import getTimePeriodString from '@common/modules/table-tool/utils/getTimePeriodString';
import downloadService from '@common/services/downloadService';
import Link from '@frontend/components/Link';
import styles from '@frontend/modules/data-catalogue/components/DataSetFileSummary.module.scss';
import { DataSetFileSummary as DataSetFileSummaryData } from '@frontend/services/dataSetFileService';
import { logEvent } from '@frontend/services/googleAnalyticsService';
import classNames from 'classnames';
import orderBy from 'lodash/orderBy';
import React, { createElement, useEffect } from 'react';

const maxContentLength = 300;

interface Props {
  dataSetFile: DataSetFileSummaryData;
  expanded?: boolean;
  headingTag?: 'h3' | 'h4';
  showLatestDataTag?: boolean;
}

export default function DataSetFileSummary({
  dataSetFile,
  expanded = false,
  headingTag = 'h3',
  showLatestDataTag = true,
}: Props) {
  const {
    id: dataSetFileId,
    content,
    fileId,
    filters = [],
    geographicLevels = [],
    indicators = [],
    latestData,
    publication,
    published,
    release,
    theme,
    timePeriods = {},
    title,
  } = dataSetFile;
  const [showMoreContent, toggleMoreContent] = useToggle(false);
  const [showDetails, toggleDetails] = useToggle(false);
  const id = `data-set-file-${dataSetFileId}`;
  const truncateContent = content.length > maxContentLength;

  useEffect(() => {
    toggleDetails(expanded);
  }, [expanded, toggleDetails]);

  const handleDownload = async () => {
    await downloadService.downloadFiles(release.id, [fileId]);

    logEvent({
      category: 'Data catalogue',
      action: 'Data set file download',
      label: `Publication: ${publication.title}, Release: ${release.title}, Data set: ${title}`,
    });
  };

  return (
    <li
      className="dfe-border-bottom govuk-!-margin-top-4 govuk-!-padding-bottom-2"
      data-testid={`data-set-file-summary-${title}`}
    >
      {createElement(
        headingTag,
        {
          className: `govuk-heading-m govuk-!-margin-bottom-2`,
          id: `${id}-heading`,
        },
        <Link to={`/data-catalogue/data-set/${dataSetFileId}`}>{title}</Link>,
      )}

      <ContentHtml
        className={classNames({
          [styles.content]: truncateContent,
          [styles.expanded]: showMoreContent,
        })}
        html={content}
      />

      {truncateContent && (
        <ButtonText onClick={toggleMoreContent}>
          {`Read ${showMoreContent ? 'less' : 'more'}`}
          <VisuallyHidden> about {title}</VisuallyHidden>
        </ButtonText>
      )}
      {showLatestDataTag && (
        <p className="govuk-!-margin-top-4">
          <Tag colour={latestData ? undefined : 'orange'}>
            {latestData
              ? 'This is the latest data'
              : 'This is not the latest data'}
          </Tag>
        </p>
      )}

      <SummaryList
        ariaLabel={`Details list for ${title}`}
        id={`${id}-details`}
        className="govuk-!-margin-bottom-4 govuk-!-margin-top-4"
        compact
        noBorder
      >
        <SummaryListItem term="Theme">{theme.title}</SummaryListItem>
        <SummaryListItem term="Published">
          <FormattedDate format="d MMM yyyy">{published}</FormattedDate>
        </SummaryListItem>
        <SummaryListItem term="Publication">
          {publication.title}
        </SummaryListItem>
        <SummaryListItem term="Release">{release.title}</SummaryListItem>
        {geographicLevels && geographicLevels.length > 0 && (
          <SummaryListItem
            className={classNames({
              'dfe-js-hidden': !showDetails,
            })}
            term="Geographic levels"
          >
            {orderBy(geographicLevels).join(', ')}
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
        {(timePeriods.from || timePeriods.to) && (
          <SummaryListItem
            className={classNames({
              'dfe-js-hidden': !showDetails,
            })}
            term="Time period"
          >
            {getTimePeriodString(timePeriods)}
          </SummaryListItem>
        )}
        <SummaryListItem
          className={classNames({
            'dfe-js-hidden': !showDetails,
          })}
          term="File"
        >
          <ButtonText onClick={handleDownload}>
            Download data set <VisuallyHidden>{`for ${title} `}</VisuallyHidden>
            (ZIP)
          </ButtonText>
        </SummaryListItem>
      </SummaryList>

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
          logEvent({
            category: 'Data catalogue',
            action: 'Data set details toggled',
            label: `Publication: ${publication.title}, Release: ${release.title}, Data set: ${title}`,
          });
        }}
      />
    </li>
  );
}
