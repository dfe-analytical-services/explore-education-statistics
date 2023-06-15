import Link from '@frontend/components/Link';
import Page from '@frontend/components/Page';
import React from 'react';
import { NextPage } from 'next';

const PrivacyNoticePage: NextPage = () => {
  return (
    <Page title="Privacy notice">
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <section>
            <h2 className="govuk-heading-m">Who we are</h2>
            <p>
              The Explore education statistics service is operated by the
              Department for Education (DfE).
            </p>
            <p>
              For the purpose of data protection legislation, DfE is the data
              controller for the personal data processed as part of the explore
              education statistics service.
            </p>
          </section>
          <section className="govuk-section-break--xl">
            <h2 className="govuk-heading-m">How we'll use your information</h2>
            <p>
              We receive your personal data when you sign up and subscribe to
              receive notifications about the Explore education statistics
              service.
            </p>
            <p>
              We’ll process your data in order to email you updates to let you
              know when the service has been updated and keep you informed about
              any changes to the statistics and data it contains.
            </p>
            <p>
              Your use of our service means you agree to our use of your
              personal data as set out in this privacy notice.
            </p>
          </section>
          <section className="govuk-section-break--xl">
            <h2 className="govuk-heading-m">
              The nature of your personal data we'll be using
            </h2>
            <p>
              In terms of your personal data, we’ll be using your email address
              to send you notifications about the service.
            </p>
          </section>
          <section className="govuk-section-break--xl">
            <h2 className="govuk-heading-m">
              Why our use of your personal data is lawful
            </h2>
            <p>
              In order for our use of your personal data to be lawful, we need
              to meet 1 (or more) conditions in the data protection legislation.
            </p>
            <p>
              For the purpose of this service, the relevant condition we're
              relying on is Article 6(1)(e) of the General Data Protection
              Regulation (GDPR).
            </p>
          </section>
          <section className="govuk-section-break--xl">
            <h2 className="govuk-heading-m">
              Who we’ll make your personal data available to
            </h2>
            <p>
              We sometimes need to make personal data available to other
              organisations.
            </p>
            <p>
              These might include contracted partners (who we have employed to
              process your personal data on our behalf) and/or other
              organisations (with whom we need to share your personal data for
              specific purposes).
            </p>
            <p>
              Where we need to share your personal data with others, we make
              sure this data sharing complies with data protection legislation.
            </p>
            <p>
              For the purpose of this service we need to share your personal
              data with{' '}
              <Link to="https://www.notifications.service.gov.uk/">
                GOV.UK Notify
              </Link>{' '}
              (a service provided by the Government Digital Service which is
              part of the Cabinet Office) so we can send you email notifications
              about this service.
            </p>
          </section>
          <section className="govuk-section-break--xl">
            <h2 className="govuk-heading-m">Where your data is stored</h2>
            <p>
              We store your data on secure servers in the{' '}
              <Link to="https://www.gov.uk/eu-eea">
                European Economic Area (EEA)
              </Link>
              . By submitting your personal data, you agree to this.
            </p>
          </section>
          <section className="govuk-section-break--xl">
            <h2 className="govuk-heading-m">
              How long we'll keep your personal data
            </h2>
            <p>
              We’ll only keep your personal data for as long as we need it for
              the purpose of sending you notifications, after which your
              personal data will be securely destroyed.
            </p>
            <p>
              Under Data Protection legislation, and in compliance with the
              relevant data processing conditions, personal data can be kept for
              longer periods of time when processed purely for archiving in the
              public interest, statistical purposes and historical or scientific
              research. For further information refer to GDPR Article 6(1)(e).
            </p>
          </section>
          <section className="govuk-section-break--xl">
            <h2 className="govuk-heading-m">Your data protection rights</h2>
            <p>Under certain circumstances, you have the right to:</p>
            <ul>
              <li>ask us for access to information about you that we hold</li>
              <li>
                have your personal data rectified - if it's inaccurate or
                incomplete
              </li>
              <li>
                request the deletion or removal of personal data where there's
                no compelling reason for its continued processing
              </li>
              <li>
                restrict our processing of your personal data (ie permitting its
                storage but no further processing)
              </li>
              <li>
                object to direct marketing (including profiling) and processing
                for the purposes of scientific/historical research and
                statistics
              </li>
              <li>
                not to be subject to decisions based purely on automated
                processing where it produces a legal or similarly significant
                effect on you
              </li>
            </ul>
            <p>
              If you need to contact us regarding any of the above use the
              details on DfE's{' '}
              <Link to="https://www.gov.uk/contact-dfe">
                Contact the Department for Education (DfE)
              </Link>{' '}
              page.
            </p>
            <p>
              Further information about your data protection rights can be found
              on the{' '}
              <Link to="https://ico.org.uk/for-organisations/guide-to-data-protection/">
                Information Commissioner’s Office (ICO)
              </Link>{' '}
              website.
            </p>
          </section>
          <section className="govuk-section-break--xl">
            <h2 className="govuk-heading-m">
              Withdrawal of consent and the right to lodge a complaint
            </h2>
            <p>
              Where we’re processing your personal data with your consent - you
              have the right to withdraw that consent.
            </p>
            <p>
              If you change your mind, or you are unhappy with our use of your
              personal data, let us know by contacting the explore education
              statistics service at{' '}
              <Link to="mailto:explore.statistics@education.gov.uk">
                explore.statistics@education.gov.uk
              </Link>
              .
            </p>
            <p>
              You have the right to raise any concerns via the{' '}
              <Link to="https://ico.org.uk/make-a-complaint/">
                Information Commissioner’s Office (ICO)
              </Link>{' '}
              website.
            </p>
          </section>
          <section className="govuk-section-break--xl">
            <h2 className="govuk-heading-m">Last updated</h2>
            <p>
              We may need to update this privacy notice periodically so we
              recommend you revisit from time to time. This version was last
              updated on DD/MM/YY.
            </p>
          </section>
          <section className="govuk-section-break--xl">
            <h2 className="govuk-heading-m">Contact information</h2>
            <p>
              If you have any questions about how your personal information will
              be used contact the Explore education statistics service at
              <Link to="mailto:explore.statistics@education.gov.uk">
                explore.statistics@education.gov.uk
              </Link>{' '}
              or contact DfE's Data Protection Officer (DPO) via our{' '}
              <Link to="https://www.gov.uk/contact-dfe">
                Contact the Department for Education (DfE)
              </Link>{' '}
              page.
            </p>
          </section>
        </div>
      </div>
    </Page>
  );
};

export default PrivacyNoticePage;
