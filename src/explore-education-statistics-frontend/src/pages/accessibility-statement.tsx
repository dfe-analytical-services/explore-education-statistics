import Page from '@frontend/components/Page';
import React from 'react';

function AcccessibilityStatementPage() {
  return (
    <Page
      title="Accessibility statement for explore education statistics"
      breadcrumbLabel="Accessibility statement"
    >
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-three-quarters">
          <section>
            <p className="govuk-body">
              This website is run by{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education">
                the Department for Education (DfE)
              </a>
              . We want as many people as possible to be able to use this
              website. For example, that means you should be able to:
            </p>

            <ul className="govuk-list--bullet">
              <li>change colours, contrast levels and fonts</li>
              <li>
                zoom in up to 300% without the text spilling off the screen
              </li>
              <li>navigate most of the website using just a keyboard</li>
              <li>
                navigate most of the website using speech recognition software
              </li>
              <li>
                listen to most of the website using a screen reader (including
                the most recent versions of JAWS, NVDA and VoiceOver)
              </li>
            </ul>
          </section>

          <section className="govuk-section-break govuk-section-break--xl">
            <h2 className="govuk-heading-l">How accessible this website is</h2>
            <p className="govuk-body">
              We know some parts of this website are not fully accessible:
            </p>
            <ul className="govuk-list--bullet">
              <li>
                the drag and drop feature is difficult to use for screen reader,
                voice activation or keyboard only users
              </li>
              <li>some form labels are not descriptive enough</li>
              <li>some buttons do not have descriptive text</li>
              <li>
                the in page search does not read back the amount of results that
                have been found when users perform a search with JAWS activated
              </li>
              <li>
                the chart feature is not accessible to screen reader, voice
                activation users or users that rely on a keyboard to navigate
              </li>
            </ul>
          </section>
        </div>
      </div>
    </Page>
  );
}

export default AcccessibilityStatementPage;
