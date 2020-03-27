import Accordion, { generateIdList } from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import ContentSectionIndex from '@common/components/ContentSectionIndex';
import FormattedDate from '@common/components/FormattedDate';
import SectionBlocks from '@common/modules/find-statistics/components/SectionBlocks';
import methodologyService, {
  Methodology,
} from '@common/services/methodologyService';
import Link from '@frontend/components/Link';
import Page from '@frontend/components/Page';
import PageSearchFormWithAnalytics from '@frontend/components/PageSearchFormWithAnalytics';
import PrintThisPage from '@frontend/components/PrintThisPage';
import MethodologyContent from '@frontend/modules/methodologies/components/MethodologyContent';
import MethodologyHeader from '@frontend/modules/methodologies/components/MethodologyHeader';
import { NextPageContext } from 'next';
import React, { Component } from 'react';

interface Props {
  publication: string;
  data: Methodology;
}

class MethodologyPage extends Component<Props> {
  private accId: string[] = generateIdList(2);

  public static async getInitialProps({ query }: NextPageContext) {
    const { publication } = query;
    const request = methodologyService.getMethodology(publication as string);

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
        description={data.summary}
        breadcrumbs={[{ name: 'Methodologies', link: '/methodology' }]}
      >
        <div className="govuk-grid-row">
          <div className="govuk-grid-column-two-thirds">
            <dl className="dfe-meta-content govuk-!-margin-top-0">
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
            <PrintThisPage
              analytics={{
                category: 'Page print',
                action: 'Print this page link selected',
              }}
            />
            <PageSearchFormWithAnalytics inputLabel="Search in this methodology page." />
          </div>
        </div>

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
                    <Link to={`/find-statistics/${data.publication.slug}`}>
                      {data.publication.title}
                    </Link>{' '}
                  </li>
                </ul>
              </aside>
            </div>
          </div>
        )}

        {data.content && (
          <Accordion id={this.accId[0]}>
            {data.content.map(({ heading, caption, order, content }) => {
              return (
                <AccordionSection
                  heading={heading}
                  caption={caption}
                  key={order}
                >
                  <MethodologyHeader>
                    <ContentSectionIndex
                      fromId={`${this.accId[0]}-${order}-content`}
                    />
                  </MethodologyHeader>

                  <MethodologyContent>
                    <SectionBlocks content={content} />
                  </MethodologyContent>
                </AccordionSection>
              );
            })}
          </Accordion>
        )}

        {data.annexes && data.annexes.length > 0 && (
          <>
            <h2 className="govuk-heading-l govuk-!-margin-top-9">Annexes</h2>

            <Accordion id={this.accId[1]}>
              {data.annexes.map(({ heading, caption, order, content }) => {
                return (
                  <AccordionSection
                    heading={heading}
                    caption={caption}
                    key={order}
                  >
                    <SectionBlocks content={content} />
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
