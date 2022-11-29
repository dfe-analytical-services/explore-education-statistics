import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import Details from '@common/components/Details';
import FormattedDate from '@common/components/FormattedDate';
import RelatedInformation from '@common/components/RelatedInformation';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
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
      <div className="govuk-grid-row govuk-!-margin-bottom-3">
        <div className="govuk-grid-column-two-thirds">
          <SummaryList>
            <SummaryListItem term="Published">
              <FormattedDate>{data.published}</FormattedDate>
            </SummaryListItem>

            {data.notes?.length > 0 && (
              <SummaryListItem term="Last updated">
                <FormattedDate>{data.notes[0].displayDate}</FormattedDate>

                <Details
                  id="methodologyNotes"
                  summary={`See all notes (${data.notes.length})`}
                  onToggle={open => {
                    if (open) {
                      logEvent({
                        category: 'Methodology Notes',
                        action: 'Methodology page notes dropdown opened',
                        label: window.location.pathname,
                      });
                    }
                  }}
                >
                  <ol className="govuk-list" data-testid="notes">
                    {data.notes.map(note => (
                      <li key={note.id}>
                        <FormattedDate
                          className="govuk-body govuk-!-font-weight-bold"
                          testId="note-displayDate"
                        >
                          {note.displayDate}
                        </FormattedDate>
                        <p data-testid="note-content">{note.content}</p>
                      </li>
                    ))}
                  </ol>
                </Details>
              </SummaryListItem>
            )}
          </SummaryList>

          <PageSearchFormWithAnalytics
            inputLabel="Search in this methodology page."
            className="govuk-!-margin-top-3 govuk-!-margin-bottom-3"
          />

          <PrintThisPage
            onClick={() => {
              logEvent({
                category: 'Page print',
                action: 'Print this page link selected',
                label: window.location.pathname,
              });
            }}
          />
        </div>
        <div className="govuk-grid-column-one-third">
          <RelatedInformation>
            {data.publications?.length > 0 && (
              <>
                <h3
                  className="govuk-heading-s govuk-!-margin-top-6"
                  id="publications"
                >
                  Publications
                </h3>

                <ul className="govuk-list govuk-list--spaced">
                  {data.publications.map(publication => (
                    <li key={publication.id}>
                      <Link to={`/find-statistics/${publication.slug}`}>
                        {publication.title}
                      </Link>{' '}
                    </li>
                  ))}
                </ul>
              </>
            )}

            <h3 className="govuk-heading-s" id="related-pages">
              Related pages
            </h3>

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
                anchorLinkIdPrefix="content"
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

      {data.annexes?.length > 0 && (
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
                  anchorLinkIdPrefix="annexes"
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
    },
  };
};

export default MethodologyPage;
