import Page from '@frontend/components/Page';
import PageTitle from '@frontend/components/PageTitle';
import React from 'react';

function ContactPage() {
  return (
    <Page breadcrumbs={[{ name: 'Contact' }]}>
      <PageTitle title="Contact explore education statistics" />
      <h2>General enquiries</h2>
      <p>
        If you need help and support or have a question about education
        statistics and data contact:
      </p>

      <p>
        <strong>Explore education statistics team</strong>
      </p>

      <p>
        Email
        <br />
        <a href="mailto:explore.statistics@education.gov.uk">
          explore.statistics@education.gov.uk
        </a>
      </p>
      <h2>Policy enquiries</h2>
      <p>
        If you have a question about statistical policies such as about the Code
        of Practice, revisions and confidentiality contact:
      </p>

      <p>
        <strong>DfE Head of Profession for Statistics</strong>
      </p>

      <p>
        Email
        <br />
        <a href="mailto:hop.statistics@education.gov.uk">
          hop.statistics@education.gov.uk
        </a>
      </p>
      <h2>Data and methodology enquiries</h2>
      <p>
        If you have a question about the data or methods used in any of our
        statistics contact the team or statistician listed within the ‘Contact
        us’ section of the relevant page.
      </p>
      <h2>Freedom of information (FOI) requests</h2>
      <p>
        If you want access to data or statistics which are not available within
        this service submit a{' '}
        <a href="https://form.education.gov.uk/en/AchieveForms/?form_uri=sandbox-publish://AF-Process-f1453496-7d8a-463f-9f33-1da2ac47ed76/AF-Stage-1e64d4cc-25fb-499a-a8d7-74e98203ac00/definition.json&redirectlink=%2Fen&cancelRedirectLink=%2Fen">
          freedom of information (FOI) request
        </a>
        .
      </p>
    </Page>
  );
}

export default ContactPage;
