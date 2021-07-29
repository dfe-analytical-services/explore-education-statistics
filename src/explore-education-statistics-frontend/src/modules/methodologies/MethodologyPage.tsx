import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import FormattedDate from '@common/components/FormattedDate';
import RelatedAside from '@common/components/RelatedAside';
import methodologyService, {
  Methodology,
} from '@common/services/methodologyService';
import Link from '@frontend/components/Link';
import Page from '@frontend/components/Page';
import PageSearchFormWithAnalytics from '@frontend/components/PageSearchFormWithAnalytics';
import PrintThisPage from '@frontend/components/PrintThisPage';
import MethodologyContentSection from '@frontend/modules/methodologies/components/MethodologyContentSection';
import MethodologySectionBlocks from '@frontend/modules/methodologies/components/MethodologySectionBlocks';
import { logEvent } from '@frontend/services/googleAnalyticsService';
import { GetServerSideProps, NextPage } from 'next';
import React from 'react';

interface Props {
  methodologySlug: string;
  data: Methodology;
}

const MethodologyPage: NextPage<Props> = ({ data }) => {
  return (
    <Page
      title={data.title}
      description="Find out how and why we collect, process and publish these statistics"
      breadcrumbs={[{ name: 'Methodologies', link: '/methodology' }]}
      caption="Methodology"
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
            onClick={() => {
              logEvent({
                category: 'Page print',
                action: 'Print this page link selected',
                label: window.location.pathname,
              });
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
            <RelatedAside>
              <h2 className="govuk-heading-m" id="subsection-title">
                Related content
              </h2>
              <ul className="govuk-list">
                <li>
                  <Link
                    to="/find-statistics/[publication]"
                    as={`/find-statistics/${data.publication.slug}`}
                  >
                    {data.publication.title}
                  </Link>{' '}
                </li>
              </ul>
            </RelatedAside>
          </div>
        </div>
      )}

      {data.content && (
        <Accordion
          id="content"
          onSectionOpen={accordionSection => {
            logEvent({
              category: `${data.title} methodology`,
              action: `Content accordion opened`,
              label: accordionSection.title,
            });
          }}
        >
          {data.content.map(({ heading, caption, order, content }) => {
            return (
              <AccordionSection
                id={`content-section-${order}`}
                heading={heading}
                caption={caption}
                key={order}
              >
                {({ open, contentId }) => (
                  <MethodologyContentSection
                    id={contentId}
                    open={open}
                    content={content}
                    methodologyId={data.id}
                  />
                )}
              </AccordionSection>
            );
          })}
        </Accordion>
      )}

      {data.annexes && data.annexes.length > 0 && (
        <>
          <h2 className="govuk-heading-l govuk-!-margin-top-9">Annexes</h2>

          <Accordion
            id="annexes"
            onSectionOpen={accordionSection => {
              logEvent({
                category: `${data.title} methodology`,
                action: `Annexes accordion opened`,
                label: accordionSection.title,
              });
            }}
          >
            {data.annexes.map(({ heading, caption, order, content }) => {
              return (
                <AccordionSection
                  heading={heading}
                  caption={caption}
                  key={order}
                >
                  <MethodologySectionBlocks
                    blocks={content}
                    methodologyId={data.id}
                  />
                </AccordionSection>
              );
            })}
          </Accordion>
        </>
      )}

      <PrintThisPage
        onClick={() => {
          logEvent({
            category: 'Page print',
            action: 'Print this page link selected',
            label: window.location.pathname,
          });
        }}
      />
    </Page>
  );
};

export const getServerSideProps: GetServerSideProps<Props> = async ({
  query,
}) => {
  const { methodology: methodologySlug } = query;

  const data = await methodologyService.getMethodology(
    methodologySlug as string,
  );

  return {
    props: {
      data,
      methodologySlug: methodologySlug as string,
    },
  };
};

export default MethodologyPage;
