import { useTenant } from '@common/contexts/TenantContext';
import FeedbackSection from '@common/components/FeedbackSection';
import InsetText from '@common/components/InsetText';
import Link from '@frontend/components/Link';
import Page from '@frontend/components/Page';
import React from 'react';

function ContactPage() {
  const {
    service: { nameMidSentence },
    organisation,
    contactEmail,
    headOfProfession,
    freedomOfInformationUrl,
  } = useTenant();

  return (
    <Page title={`Contact ${nameMidSentence}`} breadcrumbLabel="Contact">
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <section>
            <h2 className="govuk-heading-m">
              Need help using this service or have a question about the data?
            </h2>
            <p>
              If you have any specific statistical or subject-related queries,
              contact the team or named statistician listed in the 'Contact us'
              sections of the pages found under{' '}
              <Link to="/find-statistics">Find statistics and data</Link>.
            </p>
            <p>For any further queries, contact the {nameMidSentence} team:</p>
            <InsetText>
              Email: <a href={`mailto:${contactEmail}`}>{contactEmail}</a>
            </InsetText>

            {organisation.publicEnquiries && (
              <>
                <h2 className="govuk-heading-m">Have a general enquiry?</h2>
                <p>{organisation.publicEnquiries.question}</p>
                <p>{organisation.publicEnquiries.telephoneCallToAction}</p>
                <InsetText>
                  {organisation.publicEnquiries.telephone}
                  <br />
                  Opening hours: {organisation.publicEnquiries.openingHours}
                </InsetText>
              </>
            )}
          </section>

          {headOfProfession && (
            <section className="govuk-section-break--xl">
              <h2 className="govuk-heading-m">Statistical policy enquiries</h2>
              <p>
                If you have a question about statistical policies such as about
                the Code of Practice, revisions and confidentiality contact:
              </p>
              <h3 className="govuk-heading-s">{headOfProfession.title}</h3>
              <p>
                Email:{' '}
                <a href={`mailto:${headOfProfession.email}`}>
                  {headOfProfession.email}
                </a>
              </p>
            </section>
          )}

          {freedomOfInformationUrl && (
            <section className="govuk-section-break--xl">
              <h2 className="govuk-heading-m">
                Freedom of information (FOI) requests
              </h2>
              <p>
                If you want access to data or statistics which are not available
                within this service submit a{' '}
                <a
                  href={freedomOfInformationUrl}
                  rel="noopener noreferrer nofollow"
                  target="_blank"
                >
                  freedom of information (FOI) request (opens in new tab)
                </a>
                .
              </p>
            </section>
          )}

          <section className="govuk-section-break--xl">
            <h2 className="govuk-heading-m">Provide us with feedback</h2>
            <FeedbackSection />
          </section>
        </div>
      </div>
    </Page>
  );
}

export default ContactPage;
