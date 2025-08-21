import Page from '@admin/components/Page';
import React from 'react';

const PublishersGuide = () => {
  return (
    <Page
      title="Publisher's guide"
      caption="Publishing on Explore education statistics"
      wide
      breadcrumbs={[{ name: "Publisher's guide" }]}
    >
      <p>
        This page includes all of the support available for publishing on
        Explore education statistics, from written guidance to details of show
        and tells and the teams channels.
      </p>

      <h2>Guidance</h2>
      <p>
        Detailed guidance on publishing statistics, covering everything from how
        to implement Reproducible Analytical Pipelines, through to Open Data
        Standards and details of using the Explore education statistics service
        are available on the{' '}
        <a
          href="https://dfe-analytical-services.github.io/analysts-guide/"
          target="_blank"
          rel="noopener noreferrer nofollow"
        >
          Analysts' Guide (opens in new tab)
        </a>
        .
      </p>

      <h2>Community support and updates</h2>
      <p>
        We run show and tell sessions that give updates on developments to the
        service every four weeks on a Tuesday. If you would like to be added to
        invite list contact us at{' '}
        <a href="mailto:explore.statistics@education.gov.uk">
          explore.statistics@education.gov.uk
        </a>
        .
      </p>
      <p>
        If you work in Department for Education you will also have access to the
        following two Microsoft Teams channels in the{' '}
        <a
          href="https://teams.microsoft.com/l/team/19%3Ae3c8551e86094e259a60848fcff4dbc1%40thread.skype/conversations?groupId=679b2376-8c8c-4062-a1c9-0744ce5ac88f&amp;tenantId=fad277c9-c60a-4da1-b5f3-b3b8b34a82f9"
          target="_blank"
          rel="noopener noreferrer nofollow"
        >
          Statistics production and publishing teams area (opens in new tab)
        </a>
        .
      </p>

      <ol>
        <li>
          <a
            href="https://teams.microsoft.com/l/channel/19%3A1bdf09280fd94df09f0d42e19cb251fb%40thread.skype/Explore%20Education%20Statistics?groupId=679b2376-8c8c-4062-a1c9-0744ce5ac88f&amp;tenantId=fad277c9-c60a-4da1-b5f3-b3b8b34a82f9"
            target="_blank"
            rel="noopener noreferrer nofollow"
          >
            Explore education statistics (opens in new tab)
          </a>{' '}
          - for all major updates relating to the service
        </li>
        <li>
          <a
            href="https://teams.microsoft.com/l/channel/19%3A66b7e8767c084396b7f86f7796bc54e4%40thread.skype/Top%20tips?groupId=679b2376-8c8c-4062-a1c9-0744ce5ac88f&amp;tenantId=fad277c9-c60a-4da1-b5f3-b3b8b34a82f9"
            target="_blank"
            rel="noopener noreferrer nofollow"
          >
            Top tips (opens in new tab)
          </a>{' '}
          - regular tips and reminders on features available in the service to
          help you get the best out of it
        </li>
      </ol>

      <h2 className="govuk-!-padding-top-4">Contact and questions</h2>
      <p>
        If you have any questions or would like any advice with publishing using
        the service, please contact our support team at{' '}
        <a href="mailto:explore.statistics@education.gov.uk">
          explore.statistics@education.gov.uk
        </a>
        .
      </p>
    </Page>
  );
};

export default PublishersGuide;
