import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import Link from '@frontend/components/Link';
import Page from '@frontend/components/Page';
import { NextContext } from 'next';
import React, { Component } from 'react';
import ContentBlock from '@frontend/modules/find-statistics/components/ContentBlock';
import FormattedDate from '@common/components/FormattedDate';
import PrintThisPage from '@common/components/PrintThisPage';
import methodologyService, {
  Methodology,
} from '@common/services/methodologyService';
import PageSearchForm from '@common/components/PageSearchForm';

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
              <dt className="govuk-caption-m">Published: </dt>
              <dd>
                <strong>
                  <FormattedDate>{data.published}</FormattedDate>{' '}
                </strong>
              </dd>
              {data.lastUpdated.length > 0 && (
                <>
                  <dt className="govuk-caption-m">Last updated: </dt>
                  <dd>
                    <FormattedDate>{data.lastUpdated}</FormattedDate>{' '}
                  </dd>
                </>
              )}
            </dl>
          </div>
          <div className="govuk-grid-column-one-third">
            <PageSearchForm />
          </div>
        </div>

        <hr />
        <div className="govuk-grid-row">
          <div className="govuk-grid-column-two-thirds">
            <p className="govuk-body-l">
              {`Find out about the methodology behind ${
                data.publication.title
              } statistics and
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

        {data.content.length > 0 && (
          <Accordion id="contents-sections">
            {data.content.map(({ heading, caption, order, content }) => {
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
        )}

        <h2 className="govuk-heading-l govuk-!-margin-top-9">Annexes</h2>

        {data.content.length > 0 && (
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
        )}

        <PrintThisPage />
      </Page>
    );
  }
}

export default MethodologyPage;
