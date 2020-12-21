import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import RelatedInformation from '@common/components/RelatedInformation';
import themeService, { DownloadTheme } from '@common/services/themeService';
import Link from '@frontend/components/Link';
import Page from '@frontend/components/Page';
import PageSearchFormWithAnalytics from '@frontend/components/PageSearchFormWithAnalytics';
import TopicDownloadList from '@frontend/modules/download/components/TopicDownloadList';
import { logEvent } from '@frontend/services/googleAnalyticsService';
import { GetServerSideProps, NextPage } from 'next';
import React from 'react';

interface Props {
  themes: DownloadTheme[];
}

const DownloadIndexPage: NextPage<Props> = ({ themes = [] }) => {
  return (
    <Page
      title="Download latest data files"
      breadcrumbLabel="Download latest data files"
    >
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <p className="govuk-body-l">
            Find the latest data files behind our range of national and regional
            statistics for your own analysis.
          </p>
          <p>
            Previous release data can be found on their respective release
            pages.
          </p>
          <PageSearchFormWithAnalytics
            inputLabel="Search the latest data files behind our range of national and
              regional statistics for your own analysis."
          />
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
              <li>
                <Link to="/methodology">Education statistics: methodology</Link>
              </li>
            </ul>
          </RelatedInformation>
        </div>
      </div>

      {themes.length > 0 ? (
        <Accordion
          id="downloads"
          onSectionOpen={accordionSection => {
            logEvent(
              'Download index page',
              'Accordion opened',
              accordionSection.title,
            );
          }}
        >
          {themes.map(theme => (
            <AccordionSection
              key={theme.id}
              heading={theme.title}
              caption={theme.summary}
            >
              {theme.topics.map(topic => (
                <TopicDownloadList key={topic.id} topic={topic} />
              ))}
            </AccordionSection>
          ))}
        </Accordion>
      ) : (
        <div className="govuk-inset-text">No data currently published.</div>
      )}
    </Page>
  );
};

export const getServerSideProps: GetServerSideProps<Props> = async () => {
  const themes = await themeService.getDownloadThemes();

  return {
    props: {
      themes,
    },
  };
};

export default DownloadIndexPage;
