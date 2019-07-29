import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import ContentSectionIndex from '@common/components/ContentSectionIndex';
import FormattedDate from '@common/components/FormattedDate';
import methodologyService, {
  Methodology,
} from '@common/services/methodologyService';
import PageSearchFormWithAnalytics from '@frontend/components/PageSearchFormWithAnalytics';
import PrintThisPage from '@frontend/components/PrintThisPage';
import Link from '@frontend/components/Link';
import Page from '@frontend/components/Page';
import ContentBlock from '@frontend/modules/find-statistics/components/ContentBlock';
import MethodologyContent from '@frontend/prototypes/methodology/components/MethodologyContent';
import MethodologyHeader from '@frontend/prototypes/methodology/components/MethodologyHeader';
import { NextContext } from 'next';
import React, { Component } from 'react';

interface Props {
  publication: string;
  data: Methodology;
}

class MethodologyPage extends Component<Props> {
  public static async getInitialProps({
    query,
  }: NextContext<{
    publication: string;
  }>) {
    const { publication } = query;

    const request = methodologyService.getMethodology(publication);

    const data = await request;

    return {
      data,
      publication,
    };
  }

  public render() {
    const { data } = this.props;

    return (
      <Page
        title={data.title}
        caption="Methodology"
        breadcrumbs={[{ name: 'Methodologies', link: '/methodologies' }]}
      >
        <div className="govuk-grid-row">
          <div className="govuk-grid-column-two-thirds">
            <dl className="dfe-meta-content govuk-!-margin-0">
              <div>
                <dt className="govuk-caption-m">Published: </dt>
                <dd data-testid="published-date">
                  <strong>
                    <FormattedDate>{data.published}</FormattedDate>{' '}
                  </strong>
                </dd>
              </div>
              {data.lastUpdated && data.lastUpdated.length > 0 && (
                <>
                  <dt className="govuk-caption-m">Last updated: </dt>
                  <dd data-testid="last-updated">
                    <strong>
                      <FormattedDate>{data.lastUpdated}</FormattedDate>{' '}
                    </strong>
                  </dd>
                </>
              )}
            </dl>
          </div>
          <div className="govuk-grid-column-one-third">
            <PageSearchFormWithAnalytics />
          </div>
        </div>

        <hr />
        {data.publication && (
          <div className="govuk-grid-row">
            <div className="govuk-grid-column-two-thirds">
              <p className="govuk-body-l">
                {`Find out about the methodology behind ${data.publication.title} statistics and
              data and how and why they're collected and published.`}
              </p>
            </div>

            <div className="govuk-grid-column-one-third">
              <aside className="app-related-items">
                <h2 className="govuk-heading-m" id="subsection-title">
                  Related content
                </h2>
                <ul className="govuk-list">
                  <li>
                    <Link to={`/statistics/${data.publication.slug}`}>
                      {data.publication.title}
                    </Link>{' '}
                  </li>
                </ul>
              </aside>
            </div>
          </div>
        )}

        {data.content && (
          <Accordion id="contents-sections">
            {data.content.map(({ heading, caption, order, content }) => {
              return (
                <AccordionSection
                  heading={heading}
                  caption={caption}
                  key={order}
                >
                  <MethodologyHeader>
                    <ContentSectionIndex
                      fromId={`contents-sections-${order}-content`}
                    />
                  </MethodologyHeader>

                  <MethodologyContent>
                    <ContentBlock
                      content={content}
                      id={`content_${order}`}
                      publication={data.publication}
                    />
                  </MethodologyContent>
                </AccordionSection>
              );
            })}
          </Accordion>
        )}

        {data.annexes && data.annexes.length > 0 && (
          <>
            <h2 className="govuk-heading-l govuk-!-margin-top-9">Annexes</h2>

            <Accordion id="contents-sections">
              {data.annexes.map(({ heading, caption, order, content }) => {
                return (
                  <AccordionSection
                    heading={heading}
                    caption={caption}
                    key={order}
                  >
                    <ContentBlock
                      content={content}
                      id={`content_${order}`}
                      publication={data.publication}
                    />
                  </AccordionSection>
                );
              })}
            </Accordion>
          </>
        )}

        <PrintThisPage
          analytics={{
            category: 'Page print',
            action: 'Print this page link selected',
          }}
        />
      </Page>
    );
  }
}

export default MethodologyPage;
