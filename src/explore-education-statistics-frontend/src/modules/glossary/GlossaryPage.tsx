import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import RelatedInformation from '@common/components/RelatedInformation';
import Link from '@frontend/components/Link';
import Page from '@frontend/components/Page';
import PageSearchFormWithAnalytics from '@frontend/components/PageSearchFormWithAnalytics';
import { logEvent } from '@frontend/services/googleAnalyticsService';
import React from 'react';

export interface GlossaryEntry {
  heading: string;
  content: string;
}

export interface GlossaryPageProps {
  entries: GlossaryEntry[];
}

function GlossaryPage({ entries }: GlossaryPageProps) {
  return (
    <Page title="Glossary">
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <p className="govuk-body-l">
            Browse our A to Z list of definitions for terms used across
            education statistics and data.
          </p>
          <p className="govuk-body">
            The glossary is intended to grow over time as the service is
            populated.
          </p>

          <PageSearchFormWithAnalytics
            inputLabel="Search our A to Z list of definitions for terms used across
            education statistics and data."
          />
        </div>
        <div className="govuk-grid-column-one-third">
          <RelatedInformation>
            <ul className="govuk-list">
              <li>
                <Link to="/methodology">Education statistics: methodology</Link>
              </li>
            </ul>
          </RelatedInformation>
        </div>
      </div>
      <Accordion
        id="glossary"
        onSectionOpen={accordionSection => {
          logEvent('Glossary', 'Accordion opened', accordionSection.title);
        }}
      >
        {entries.map(entry => (
          <AccordionSection
            key={entry.heading}
            heading={entry.heading}
            id={`glossary-${entry.heading}`}
          >
            {entry.content ? (
              <div
                // eslint-disable-next-line react/no-danger
                dangerouslySetInnerHTML={{
                  __html: entry.content,
                }}
              />
            ) : (
              <p className="govuk-inset-text">
                There are currently no entries under this section
              </p>
            )}
          </AccordionSection>
        ))}
      </Accordion>
    </Page>
  );
}

export default GlossaryPage;
