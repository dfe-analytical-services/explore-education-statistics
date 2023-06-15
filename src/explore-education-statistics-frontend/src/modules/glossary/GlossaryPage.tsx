import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import RelatedInformation from '@common/components/RelatedInformation';
import Link from '@frontend/components/Link';
import Page from '@frontend/components/Page';
import PageSearchFormWithAnalytics from '@frontend/components/PageSearchFormWithAnalytics';
import { logEvent } from '@frontend/services/googleAnalyticsService';
import { GetServerSideProps, NextPage } from 'next';
import sanitizeHtml, {
  defaultSanitizeOptions,
  SanitizeHtmlOptions,
} from '@common/utils/sanitizeHtml';
import { GlossaryCategory } from '@common/services/types/glossary';
import glossaryService from '@frontend/services/glossaryService';
import React from 'react';

export interface Props {
  categories: GlossaryCategory[];
}

const sanitizeHtmlOptions: SanitizeHtmlOptions = {
  ...defaultSanitizeOptions,
  allowedAttributes: {
    ...defaultSanitizeOptions.allowedAttributes,
    a: ['href', 'rel', 'target'],
  },
};

const GlossaryPage: NextPage<Props> = ({ categories = [] }) => {
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
          logEvent({
            category: 'Glossary',
            action: 'Accordion opened',
            label: accordionSection.title,
          });
        }}
      >
        {categories.map(category => (
          <AccordionSection
            key={category.heading}
            heading={category.heading}
            id={`glossary-${category.heading}`}
          >
            {category.entries.length ? (
              category.entries.map(entry => (
                <div
                  id={entry.slug}
                  key={entry.slug}
                  className="govuk-!-margin-bottom-7"
                >
                  <h3>{entry.title}</h3>
                  <div
                    // eslint-disable-next-line react/no-danger
                    dangerouslySetInnerHTML={{
                      __html: sanitizeHtml(entry.body, sanitizeHtmlOptions),
                    }}
                  />
                </div>
              ))
            ) : (
              <p className="govuk-inset-text">
                There are currently no entries under this section.
              </p>
            )}
          </AccordionSection>
        ))}
      </Accordion>
    </Page>
  );
};

export const getServerSideProps: GetServerSideProps<Props> = async () => {
  const categories = await glossaryService.listEntries();

  return {
    props: {
      categories,
    },
  };
};

export default GlossaryPage;
