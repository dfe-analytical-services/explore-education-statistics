import AccordionSection from '@common/components/AccordionSection';
import Details from '@common/components/Details';
import RelatedInformation from '@common/components/RelatedInformation';
import themeService, { MethodologyTheme } from '@common/services/themeService';
import Link from '@frontend/components/Link';
import Page from '@frontend/components/Page';
import PageSearchFormWithAnalytics from '@frontend/components/PageSearchFormWithAnalytics';
import MethodologyList from '@frontend/modules/methodologies/components/MethodologyList';
import { GetServerSideProps, NextPage } from 'next';
import React from 'react';
import { logEvent } from '@frontend/services/googleAnalyticsService';
import Accordion from '@common/components/Accordion';

interface Props {
  themes: MethodologyTheme[];
}

const MethodologyIndexPage: NextPage<Props> = ({ themes = [] }) => {
  return (
    <Page
      title="Methodologies"
      pageMeta={{
        description:
          'Browse to find out about the methodology behind specific education statistics and data',
      }}
    >
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <p className="govuk-body-l">
            Browse to find out about the methodology behind specific education
            statistics and data and how and why they're collected and published.
          </p>
          <PageSearchFormWithAnalytics inputLabel="Search to find the methodology behind specific education statistics and data." />
        </div>
        <div className="govuk-grid-column-one-third">
          <RelatedInformation>
            <ul className="govuk-list">
              <li>
                <Link to="/find-statistics">Find statistics and data</Link>
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
              category: 'Methodologies',
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
                      detailsId="topic"
                    >
                      <MethodologyList publications={publications} />
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
  const themes = await themeService.getMethodologyThemes();

  return {
    props: {
      themes,
    },
  };
};

export default MethodologyIndexPage;
