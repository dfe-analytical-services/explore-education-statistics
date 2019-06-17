import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import Details from '@common/components/Details';
import PageSearchForm from '@common/components/PageSearchForm';
import RelatedInformation from '@common/components/RelatedInformation';
import { contentApi } from '@common/services/api';
import Link from '@frontend/components/Link';
import Page from '@frontend/components/Page';
import PageTitle from '@frontend/components/PageTitle';
import React, { Component } from 'react';
import MethodologyList from './components/MethodologyList';
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

class MethodologyIndexPage extends Component<Props> {
  public static defaultProps = {
    themes: [],
  };

  public static async getInitialProps() {
    const themes = await contentApi.get('/Download/tree');
    return { themes };
  }

  public render() {
    const { themes } = this.props;
    return (
      <Page
        breadcrumbs={[
          {
            link: '/methodology',
            name: 'Methodology',
          },
        ]}
      >
        <PageTitle title="Education statistics: methodology" />
        <div className="govuk-grid-row">
          <div className="govuk-grid-column-two-thirds">
            <p className="govuk-body-l">
              Browse to find out about the methodology behind specific education
              statistics and data and how and why they're collected and
              published.
            </p>
            <PageSearchForm />
          </div>
          <div className="govuk-grid-column-one-third">
            <RelatedInformation>
              <ul className="govuk-list">
                <li>
                  <Link to="/statistics">Find statistics and data</Link>
                </li>
                <li>
                  <Link to="/glossary">Education statistics: glossary</Link>
                </li>
              </ul>
            </RelatedInformation>
          </div>
        </div>

        {themes.length > 0 ? (
          <Accordion id="themesMethodology">
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
                      <Details key={topicId} summary={topicTitle}>
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
  }
}

export default MethodologyIndexPage;
