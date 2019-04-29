import Page from '@frontend/components/Page';
import PageTitle from '@frontend/components/PageTitle';
import React from 'react';

function PrivacyPolicyPage() {
  return (
    <Page breadcrumbs={[{ name: 'Privacy Policy' }]}>
      <PageTitle title="Explore education statistics privacy policy" />

      <div>
        <h3>Who we are</h3>
        <p>
          The explore education statistics service is operated by the Department
          for Education ('we' or 'us'). For the purpose of data protection
          legislation, DfE is the data controller for the personal data
          processed as part of the explore education statistics service.
        </p>
        <p>
          This privacy policy sets out how we collect and process your personal
          data through the service. This policy applies to all users of the
          service ('you').
        </p>
      </div>
      <div>
        <h3>How we'll use your information</h3>
        <p>
          We receive your personal data when you visit our service, complete our
          feedback survey or tell us if something is wrong. We use this
          information to improve the service. You can find out more at:{' '}
          <a
            href="https://www.gov.uk/government/organisations/department-for-education"
            target="_blank"
            rel="noopener noreferrer"
          >
            www.gov.uk/government/organisations/department-for-education{' '}
            <span> opens in a new window </span>
          </a>
        </p>
        <p>
          Your use of our service means you accept and consent to our use of
          your personal data as set out in this privacy policy, which
          supplements our terms of use for the{' '}
          <a href="" target="_blank">
            explore education statistics service
            <span> opens in a new window </span>
          </a>
          .
        </p>
      </div>
      <div>
        <h3>The nature of your personal data we will be using</h3>
        <p>
          We’ll be using the following categories of your personal data for our
          service:
        </p>
        <ul>
          <li>type of personal data 1</li>
          <li>type of personal data 2</li>
        </ul>
      </div>
      <div>
        <h3>Why our use of your personal data is lawful</h3>
        <p>
          In order for our use of your personal data to be lawful, we need to
          meet 1 (or more) conditions in the data protection legislation. For
          the purpose of this service, the relevant condition(s) we're meeting
          are as set out in Article 6(1)(e) and Article 9(2)(e and g) of the
          General Data Protection Regulation (GDPR).
        </p>
      </div>
      <div>
        <h3>Where your data is stored</h3>
        <p>SECTION AND CONTENT TBC</p>
      </div>
      <div>
        <h3>Keeping your data secure</h3>
        <p>
          Transmitting information over the internet is generally not completely
          secure, and we cannot guarantee the security of your data. Any data
          you transmit is at your own risk.
        </p>
        <p>
          We have appropriate procedures and security features in place to keep
          your data secure once we receive it.
        </p>
        <p>
          We won't share your information with any other organisations for
          marketing, market research or commercial purposes, and we do not pass
          on your details to other websites.
        </p>
      </div>
      <div>
        <h3>How long we will keep your personal data</h3>
        <p>
          We'll keep your personal data for a maximum of 3 years, after which
          point it will be securely destroyed.
        </p>
        <p>
          We'll share your information with third parties only where the law
          allows it.
        </p>
      </div>
      <div>
        <h3>Your data protection rights</h3>
        <p>You have the right:</p>
        <ul>
          <li>to ask us for access to information about you that we hold</li>
          <li>
            to have your personal data rectified, if it's inaccurate or
            incomplete
          </li>
          <li>
            to request the deletion or removal of personal data where there is
            no compelling reason for its continued processing
          </li>
          <li>
            to restrict our processing of your personal data (ie permitting its
            storage but no further processing)
          </li>
          <li>
            to object to direct marketing (including profiling) and processing
            for the purposes of scientific/historical research and statistics
          </li>
          <li>
            not to be subject to decisions based purely on automated processing
            where it produces a legal or similarly significant effect on you
          </li>
        </ul>
        <p>
          Further information about{' '}
          <a
            href="https://ico.org.uk/for-organisations/guide-to-data-protection/principle-6-rights/"
            target="_blank"
            rel="noopener noreferrer"
          >
            your data protection rights <span> opens in a new window </span>
          </a>{' '}
          appears on the Information Commissioner's website.
        </p>
      </div>
      <div>
        <h3>The right to lodge a complaint</h3>
        <p>
          You have the right to raise any concerns with the Information
          Commissioner’s Office (ICO) via their website at{' '}
          <a
            href="https://ico.org.uk/concerns/"
            target="_blank"
            rel="noopener noreferrer"
          >
            https://ico.org.uk/concerns/ <span> opens in a new window </span>
          </a>
          .
        </p>
      </div>
      <div>
        <h3>Last updated</h3>
        <p>
          We may need to update this privacy notice periodically so we recommend
          that you revisit this information from time to time. This version was
          last updated on DD/MM/YY.
        </p>
      </div>
      <div>
        <h3>Following a link to CSP from another website</h3>
        <p>
          If you come to our service from another website, we may receive
          personal information about you from the other website. You should read
          the privacy policy of the website you came from to find out more about
          this.
        </p>
      </div>
      <div>
        <h3>Cookies</h3>
        <p>
          We put small files (known as 'cookies') onto your computer or other
          internet-enabled device when you visit our service. Cookies are stored
          by your browser and hard drive and are used to distinguish you from
          other visitors to our servie, to collect information about how you
          browse our service and for security and monitoring of our service.
        </p>
        <p>
          You consent to the storage of cookies by the Explore education
          statistics service on your computer's hard drive.
        </p>
      </div>
      <div>
        <h3>Contact information</h3>
        <p>
          If you have any questions about how your personal information will be
          processed, contact us at DETAILS TBC.
        </p>
      </div>
    </Page>
  );
}

export default PrivacyPolicyPage;
