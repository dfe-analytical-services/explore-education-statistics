import Accordion, { generateIdList } from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import ContentSectionIndex from '@common/components/ContentSectionIndex';
import FormattedDate from '@common/components/FormattedDate';
import RelatedAside from '@common/components/RelatedAside';
import SectionBlocks from '@common/modules/find-statistics/components/SectionBlocks';
import methodologyService, {
  Methodology,
} from '@common/services/methodologyService';
import Link from '@frontend/components/Link';
import Page from '@frontend/components/Page';
import PageSearchFormWithAnalytics from '@frontend/components/PageSearchFormWithAnalytics';
import PrintThisPage from '@frontend/components/PrintThisPage';
import MethodologyHeader from '@frontend/modules/methodologies/components/MethodologyHeader';
import { GetServerSideProps, NextPage } from 'next';
import React from 'react';

const accordionIds: string[] = generateIdList(2);

interface Props {
  methodologySlug: string;
  data: Methodology;
}

const MethodologyPage: NextPage<Props> = ({ data }) => {
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
        <Accordion id={accordionIds[0]}>
          {data.content.map(({ heading, caption, order, content }) => {
            return (
              <AccordionSection
                id={`${accordionIds[0]}-${order}`}
                heading={heading}
                caption={caption}
                key={order}
              >
                <div className="govuk-grid-row">
                  <div className="govuk-grid-column-one-quarter">
                    <MethodologyHeader>
                      <ContentSectionIndex
                        fromId={`${accordionIds[0]}-${order}-content`}
                      />
                    </MethodologyHeader>
                  </div>

                  <div className="govuk-grid-column-three-quarters">
                    <SectionBlocks content={content} />
                  </div>
                </div>
              </AccordionSection>
            );
          })}
        </Accordion>
      )}

      <PrintThisPage
        analytics={{
          category: 'Page print',
          action: 'Print this page link selected',
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
