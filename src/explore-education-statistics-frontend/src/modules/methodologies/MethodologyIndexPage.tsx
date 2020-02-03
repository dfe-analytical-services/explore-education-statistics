import Accordion, { generateIdList } from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import PageSearchFormWithAnalytics from '@frontend/components/PageSearchFormWithAnalytics';
import RelatedInformation from '@common/components/RelatedInformation';
import methodologyService, { Theme } from '@common/services/methodologyService';
import Link from '@frontend/components/Link';
import Page from '@frontend/components/Page';
import React, { Component, useState } from 'react';
import Details from '@common/components/Details';
import classNames from 'classnames';
import MethodologyList from '@frontend/modules/methodologies/components/MethodologyList';

interface Props {
  themes: Theme[];
}

interface Methodology {
  title: string;
  slug: string;
}

class MethodologyIndexPage extends Component<Props> {
  private accId: string[] = generateIdList(1);

  public static defaultProps = {
    themes: [],
  };

  public static async getInitialProps() {
    const themes = await methodologyService.getMethodologies();
    return { themes };
  }

  public render() {
    const { themes } = this.props;
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
              statistics and data and how and why they're collected and
              published.
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
                      <>
                        <Details
                          key={topicId}
                          id={topicId}
                          summary={topicTitle}
                        >
                          <MethodologyList publications={publications} />
                        </Details>
                      </>
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
