import Page from '@frontend/components/Page';
import React from 'react';
import Link from '@frontend/components/Link';

function AcccessibilityStatementPage() {
  return (
    <Page
      title="Accessibility statement for explore education statistics"
      breadcrumbLabel="Accessibility statement"
    >
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
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

          <section className="govuk-section-break--xl">
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

          <section className="govuk-section-break--xl">
            <h2 className="govuk-heading-l">
              What to do if you cannot access parts of this website
            </h2>
            <p className="govuk-body">
              If you need information on this website in a different format like
              accessible PDF, large print, easy read, audio recording or
              braille:
            </p>
            <ul className="govuk-list--bullet">
              <li>
                email:{' '}
                <a href="mailto:explore.statistics@education.gov.uk">
                  explore.statistics@education.gov.uk
                </a>
              </li>
            </ul>
          </section>

          <section className="govuk-section-break--xl">
            <h2 className="govuk-heading-l">
              Reporting accessibility problems with this website
            </h2>
            <p className="govuk-body">
              We’re always looking to improve the accessibility of this website.
              If you find any problems not listed on this page or think we’re
              not meeting accessibility requirements, contact us:
            </p>
            <ul className="govuk-list--bullet">
              <li>
                email:{' '}
                <a href="mailto:explore.statistics@education.gov.uk">
                  explore.statistics@education.gov.uk
                </a>
              </li>
            </ul>
          </section>

          <section className="govuk-section-break--xl">
            <h2 className="govuk-heading-l">Enforcement procedure</h2>
            <p className="govuk-body">
              The Equality and Human Rights Commission (EHRC) is responsible for
              enforcing the Public Sector Bodies (Websites and Mobile
              Applications) (No. 2) Accessibility Regulations 2018 (the
              ‘accessibility regulations’). If you’re not happy with how we
              respond to your complaint,{' '}
              <a rel="external" href="https://www.equalityadvisoryservice.com/">
                contact the Equality Advisory and Support Service (EASS)
              </a>
              .
            </p>
          </section>

          <section className="govuk-section-break--xl">
            <h2 className="govuk-heading-l">
              Technical information about this website’s accessibility
            </h2>
            <p className="govuk-body">
              The Department for Education (DfE) is committed to making its
              website accessible, in accordance with the Public Sector Bodies
              (Websites and Mobile Applications) (No. 2) Accessibility
              Regulations 2018.
            </p>
            <p className="govuk-body">
              This website is partially compliant with the{' '}
              <a rel="external" href="https://www.w3.org/TR/WCAG21/">
                Web Content Accessibility Guidelines version 2.1
              </a>{' '}
              AA standard, due to the non-compliances listed below.
            </p>
          </section>
          <section className="govuk-section-break--xl">
            <h2 className="govuk-heading-l">Non accessible content</h2>
            <p className="govuk-body">
              The content listed below is non-accessible for the following
              reasons.
            </p>
            <p className="govuk-body">
              The chart feature was not accessible to screen reader, voice
              activation users or users that rely on a keyboard to navigate. As
              a result, users were unable to access this feature without
              assistance.Users need to be informed that this information can be
              accessedin another area of the page.
            </p>
            <p>
              The map feature was not accessible to screen reader, voice
              activation users or users that rely on a keyboard to navigate. As
              a result, users were unableto access this feature without
              assistance.
            </p>
            <h3 className="govuk-heading-m">Disproportionate burden</h3>
            <p className="govuk-body">Not applicable</p>
          </section>
          <section className="govuk-section-break--xl">
            <h2 className="govuk-heading-l">How we tested this website</h2>
            <p className="govuk-body">
              This website was last tested on 23 September 2019 against{' '}
              <a href="https://www.w3.org/TR/WCAG21/" rel="external">
                Accessibility Guidelines WCAG2.1
              </a>
              .
            </p>
            <p className="govuk-body">
              The test was carried out by the{' '}
              <a href="https://digitalaccessibilitycentre.org/">
                Digital accessibility centre (DAC)
              </a>
              .
            </p>
            <p className="govuk-body">
              DAC tested a sample of pages to cover the core funcationality of
              the service including:
            </p>

            <ul className="govuk-list--bullet">
              <li>
                <Link to="/">the homepage</Link>
              </li>
              <li>
                <Link to="/data-tables">table tool page</Link>
              </li>
              <li>
                <Link to="/download-latest-data">download data page</Link>
              </li>
              <li>
                <Link to="/find-statistics">find statistics and data page</Link>
              </li>
              <li>
                <Link to="/find-statistics/pupil-absence-in-schools-in-england">
                  publication page
                </Link>
              </li>
              <li>
                <Link to="/methodology">methodology homepage</Link>
              </li>
              <li>
                <Link to="/methodology/pupil-absence-in-schools-in-england">
                  specific release methodology
                </Link>
              </li>
              <li>
                <Link to="/glossary">glossary page</Link>
              </li>
              <li>
                <Link to="/subscriptions?slug=pupil-absence-in-schools-in-england">
                  notify me page
                </Link>
              </li>
            </ul>
          </section>

          <section className="govuk-section-break govuk-section-break--xl">
            <h2 className="govuk-heading-l">
              What we’re doing to improve accessibility
            </h2>
            <p className="govuk-body">
              We plan to continually test the service for accessibility issues,
              and create a prioritised list of issues to resolve.
            </p>
            <p className="govuk-body">
              This statement was prepared on 4 December 2019.
            </p>
          </section>
        </div>
      </div>
    </Page>
  );
}

export default AcccessibilityStatementPage;
