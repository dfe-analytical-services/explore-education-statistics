import AccordionSection from '@common/components/AccordionSection';
import Details from '@common/components/Details';
import RelatedInformation from '@common/components/RelatedInformation';
import themeService, {
  PublicationSummary,
  Theme,
} from '@common/services/themeService';
import Link from '@frontend/components/Link';
import Page from '@frontend/components/Page';
import PageSearchFormWithAnalytics from '@frontend/components/PageSearchFormWithAnalytics';
import { GetServerSideProps, NextPage } from 'next';
import React, { Fragment } from 'react';
import { logEvent } from '@frontend/services/googleAnalyticsService';
import Accordion from '@common/components/Accordion';
import { ReleaseType, releaseTypes } from '@common/services/types/releaseType';
import PublicationList from './components/PublicationList';

interface Props {
  themes: Theme[];
}

const FindStatisticsPage: NextPage<Props> = ({ themes = [] }) => {
  const {
    NationalStatistics,
    OfficialStatistics,
    ...releaseTypesExcludingNationalAndOfficalStatistics
  } = releaseTypes;

  const publicationTypes = {
    ...releaseTypesExcludingNationalAndOfficalStatistics,
    NationalAndOfficial: 'National and official statistics',
    Legacy: 'Not yet on this service',
  } as const;

  type PublicationType = keyof typeof publicationTypes;

  const releaseTypeToPublicationType = (
    releaseType?: ReleaseType,
  ): PublicationType => {
    switch (releaseType) {
      case 'NationalStatistics':
      case 'OfficialStatistics':
        return 'NationalAndOfficial';
      case undefined:
        return 'Legacy';
      default:
        return releaseType;
    }
  };

  const publicationTypeReferenceOrder: PublicationType[] = [
    'NationalAndOfficial',
    'ExperimentalStatistics',
    'AdHocStatistics',
    'ManagementInformation',
    'Legacy',
  ];

  const groupPublicationsByType = (publications: PublicationSummary[]) =>
    Object.entries(
      publications.reduce((acc, publication) => {
        const publicationType = releaseTypeToPublicationType(
          publication.latestReleaseType,
        );
        (acc[publicationType] ||= []).push(publication);
        return acc;
      }, {} as { [key in PublicationType]?: PublicationSummary[] }),
    )
      .map(([key, value]) => ({
        type: key as PublicationType,
        group: value,
      }))
      .sort(
        ({ type: a }, { type: b }) =>
          publicationTypeReferenceOrder.indexOf(a) -
          publicationTypeReferenceOrder.indexOf(b),
      );

  return (
    <Page title="Find statistics and data">
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <p className="govuk-body-l">
            Browse to find the statistics and data you’re looking for and open
            the section to get links to:
          </p>

          <ul className="govuk-!-margin-bottom-9">
            <li>
              up-to-date national statistical headlines, breakdowns and
              explanations
            </li>
            <li>
              charts and tables to help you compare contrast and view national
              and regional statistical data and trends
            </li>
          </ul>

          <PageSearchFormWithAnalytics inputLabel="Search to find the statistics and data you’re looking for." />
        </div>
        <div className="govuk-grid-column-one-third">
          <RelatedInformation>
            <ul className="govuk-list">
              <li>
                <Link to="/methodology">Education statistics: methodology</Link>
              </li>
              <li>
                <Link to="/glossary">Education statistics: glossary</Link>
              </li>
            </ul>
          </RelatedInformation>
        </div>
      </div>

      {themes.length > 0 ? (
        <Accordion
          id="themes"
          onSectionOpen={accordionSection => {
            logEvent({
              category: 'Find statistics and data',
              action: 'Publications accordion opened',
              label: accordionSection.title,
            });
          }}
        >
          {themes.map(
            ({
              id: themeId,
              title: themeTitle,
              summary: themeSummary,
              topics,
            }) => (
              <AccordionSection
                key={themeId}
                heading={themeTitle}
                caption={themeSummary}
              >
                {topics.map(
                  ({ id: topicId, title: topicTitle, publications }) => (
                    <Details
                      key={topicId}
                      summary={topicTitle}
                      id={`topic-${topicId}`}
                      detailsId="topic"
                    >
                      {groupPublicationsByType(publications).map(
                        ({ type, group }) => (
                          <Fragment key={type}>
                            <h3
                              id={`publication-type-heading-${type}`}
                              className="govuk-heading-s"
                            >
                              {publicationTypes[type]}
                            </h3>
                            <PublicationList
                              publications={group}
                              testId={`publications-list-${type}`}
                            />
                          </Fragment>
                        ),
                      )}
                    </Details>
                  ),
                )}
              </AccordionSection>
            ),
          )}
        </Accordion>
      ) : (
        <div className="govuk-inset-text">No data currently published.</div>
      )}
    </Page>
  );
};

export const getServerSideProps: GetServerSideProps<Props> = async () => {
  const themes = await themeService.listThemes();

  return {
    props: {
      themes,
    },
  };
};

export default FindStatisticsPage;
