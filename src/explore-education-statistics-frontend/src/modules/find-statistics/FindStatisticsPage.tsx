import Accordion, { generateIdList } from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import Details from '@common/components/Details';
import RelatedInformation from '@common/components/RelatedInformation';
import { contentApi } from '@common/services/api';
import Link from '@frontend/components/Link';
import Page from '@frontend/components/Page';
import PageSearchFormWithAnalytics from '@frontend/components/PageSearchFormWithAnalytics';
import React, { Component } from 'react';
import PublicationList from './components/PublicationList';
import { Topic } from './components/TopicList';

interface Props {
  themes: {
    id: string;
    slug: string;
    title: string;
    summary: string;
    topics: Topic[];
  }[];
}

class FindStatisticsPage extends Component<Props> {
  public static defaultProps = {
    themes: [],
  };

  public static async getInitialProps() {
    const themes = await contentApi.get('/Content/tree');

    return {
      themes,
    };
  }

  private accId: string[] = generateIdList(1);

  public render() {
    const { themes } = this.props;

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
                  <Link to="/methodology">
                    Education statistics: methodology
                  </Link>
                </li>
                <li>
                  <Link to="/glossary">Education statistics: glossary</Link>
                </li>
              </ul>
            </RelatedInformation>
          </div>
        </div>

        {themes.length > 0 ? (
          <Accordion id={this.accId[0]}>
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
  }
}

export default FindStatisticsPage;
