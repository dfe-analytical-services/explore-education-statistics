import Page from '@frontend/components/Page';
import Link from '@frontend/components/Link';
import React from 'react';
import { NextPage } from 'next';

const AcccessibilityStatementPage: NextPage = () => {
  return (
    <Page
      title="Accessibility statement for Explore education statistics"
      breadcrumbLabel="Accessibility statement"
    >
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-three-quarters">
          <section>
            <p className="govuk-body">
              This website is run by{' '}
              <Link to="https://www.gov.uk/government/organisations/department-for-education">
                the Department for Education (DfE)
              </Link>
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
                the 'in page' search feature does not function as expected using
                screen reader and keyboard
              </li>
              <li>
                the drag and drop feature for table reordering is difficult to
                use for screen reader, voice activation or keyboard only users
              </li>
              <li>
                the focus order of hidden links in the charts tab when using
                keyboard leads to unexpected selections
              </li>
              <li>
                when opening some collapsible sections unexpected elements can
                be read out to screen reader users
              </li>
              <li>some tables are missing title captions</li>
              <li>
                some buttons, links, and headings are not descriptive enough
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
              <Link to="https://www.equalityadvisoryservice.com/">
                contact the Equality Advisory and Support Service (EASS)
              </Link>
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
              reasons. We will address these issues during our public beta phase
              to ensure our content is accessible.
            </p>
            <ol>
              <li>
                The 'in page' search results are not appropriately announced.
                This means when keyboard focus is on an actual result the text
                is not announced to screen reader users. This doesn’t meet WCAG
                2.1 success criterion 1.3.1 (info and relationships).
              </li>
              <li>
                The drag and drop feature for reordering tables has nested
                active form elements. This means a user could tab to an element,
                but the screen reader will not announce its name, role or state.
                This doesn’t meet WCAG 2.1 success criterion 3.3.2
                (Understanding labels or instructions) and 4.1.2 (Understanding
                name, role, value)
              </li>
              <li>
                Nested fieldsets used in the table tool. This means screen
                reader users can find it difficult to traverse through the form
                elements, and select radio buttons using the keyboard. This
                doesn’t meet WCAG 2.1 success criterion 2.4.3 (Understanding
                focus order)
              </li>
              <li>
                Some collapsible sections can trigger an unexpected
                announcement. This means when a screen reader user opens a
                collapsible section they will sometimes hear an unexpected
                announcement that could cause confusion (occurs whenever there
                is an element inside the section using aria-live='assertive').
                This doesn’t meet WCAG 2.1 success criterion 4.1.3
                (understanding status messages).
              </li>
              <li>
                Some tables are missing a summary or caption. This means
                assistive technology users will find it difficult to determine a
                table’s contents without traversing it. This doesn’t meet WCAG
                2.1 success criterion 1.3.1 (info and relationships).
              </li>
              <li>
                There are hidden links in content on the charts tab. This means
                a keyboard user can focus on an unexpected element on the page
                thus causing confusion. This doesn’t meet WCAG 2.1 success
                criterion 2.4.3 (Understanding focus order)
              </li>
              <li>
                Some pages have multiple buttons with the same title. This means
                a screen reader user is unable to differentiate between these
                buttons out of context to make an informed selection. This
                doesn’t meet WCAG 2.1 success criterion 2.4.6 (understanding
                headings and labels).
              </li>
              <li>
                Some pages have multiple headings with the same title. This
                means it is difficult for screen reader users to distinguish out
                of context what the headings are relating to. This doesn’t meet
                WCAG 2.1 success criterion 2.4.6 (understanding headings and
                labels).
              </li>
            </ol>
            <h3 className="govuk-heading-m">Disproportionate burden</h3>
            <p className="govuk-body">Not applicable</p>
          </section>
          <section className="govuk-section-break--xl">
            <h2 className="govuk-heading-l">How we tested this website</h2>
            <p className="govuk-body">
              This website was last tested on 15 November 2021 against{' '}
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
              DAC tested a sample of pages to cover the core functionality of
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
                <Link to="/data-catalogue">download data page</Link>
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
          </section>
          <section className="govuk-section-break govuk-section-break--xl">
            <h2 className="govuk-heading-l">
              Preparation of this accessibility statement
            </h2>
            <p className="govuk-body">
              This statement was prepared on 4 December 2019. It was last
              reviewed on 6 January 2022.
            </p>
          </section>
        </div>
      </div>
    </Page>
  );
};

export default AcccessibilityStatementPage;
