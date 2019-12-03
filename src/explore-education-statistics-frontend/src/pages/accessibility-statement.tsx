import Page from '@frontend/components/Page';
import React from 'react';

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
            <p className="govuk-body">[TO BE ADDED]</p>
            <h3 className="govuk-heading-m">
              Non compliance with the accessibility regulations
            </h3>
            <p className="govuk-body">[TO BE ADDED]</p>
            <h3 className="govuk-heading-m">Disproportionate burden</h3>
            <p className="govuk-body">
              [TO BE ADDED - Charts are not accessible, but always sit alongside
              accessible tables showing the same information]
            </p>
            <h3 className="govuk-heading-m">
              Content that’s not within the scope of the accessibility
              regulations
            </h3>
            <p className="govuk-body">[TO BE ADDED]</p>
          </section>
          <section className="govuk-section-break--xl">
            <h2 className="govuk-heading-l">How we tested this website</h2>
            <p className="govuk-body">
              This website was last tested on 23 September 2019. The test was
              carried out by dac digital accessibility centre.
            </p>

            <p className="govuk-body">
              All the functionality of the public facing service was assessed
              against the Web Content{' '}
              <a href="https://www.w3.org/TR/WCAG21/" rel="external">
                Accessibility Guidelines WCAG2.1
              </a>
            </p>

            <p className="govuk-body">[LINK TO ACCESSIBILITY REPORT??]</p>
          </section>

          <section className="govuk-section-break govuk-section-break--xl">
            <h2 className="govuk-heading-l">
              What we’re doing to improve accessibility
            </h2>
            <p className="govuk-body">
              Our accessibility roadmap [add link to roadmap] shows how and when
              we plan to improve accessibility on this website.
            </p>
            <p className="govuk-body">
              This statement was prepared on [date when it was first published].
              It was last updated on [date when it was last updated].
            </p>
          </section>
        </div>
      </div>
    </Page>
  );
}

export default AcccessibilityStatementPage;
