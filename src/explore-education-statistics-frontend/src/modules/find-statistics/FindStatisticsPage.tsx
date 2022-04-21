import AccordionSection from '@common/components/AccordionSection';
import Details from '@common/components/Details';
import RelatedInformation from '@common/components/RelatedInformation';
import themeService, { Theme } from '@common/services/themeService';
import Link from '@frontend/components/Link';
import Page from '@frontend/components/Page';
import PageSearchFormWithAnalytics from '@frontend/components/PageSearchFormWithAnalytics';
import { GetServerSideProps, NextPage } from 'next';
import React from 'react';
import { logEvent } from '@frontend/services/googleAnalyticsService';
import Accordion from '@common/components/Accordion';
import PublicationList from './components/PublicationList';

interface Props {
  themes: Theme[];
}

const FindStatisticsPage: NextPage<Props> = ({ themes = [] }) => {
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
                      <PublicationList publications={publications} />
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
  const themes = await themeService.listThemes({
    publicationFilter: 'FindStatistics',
  });

  return {
    props: {
      themes,
    },
  };
};

export default FindStatisticsPage;
