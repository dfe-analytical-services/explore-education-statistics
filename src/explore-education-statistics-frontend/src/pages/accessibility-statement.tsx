import Page from '@frontend/components/Page';
import React from 'react';
import Link from '@frontend/components/Link';

function AcccessibilityStatementPage() {
  return (
    <Page
      title="Accessibility statement for Explore education statistics"
      breadcrumbLabel="Accessibility statement"
    >
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-three-quarters">
          <p>
            This accessibility statement applies to the
            explore-education-statistics.service.gov.uk website. This website is
            run by the{' '}
            <a
              rel="noopener noreferrer nofollow"
              target="_blank"
              href="https://www.gov.uk/government/organisations/department-for-education"
            >
              Department for Education (DfE) (opens in new tab)
            </a>
            . This statement does not cover any other services run by the
            Department for Education (DfE) or GOV.UK.
          </p>
          <h2>How you should be able to use this website</h2>
          <p>
            We want as many people as possible to be able to use this website.
            You should be able to:
          </p>
          <ul>
            <li>
              change colours, contrast levels and fonts using browser or device
              settings
            </li>
            <li>zoom in up to 400% without the text spilling off the screen</li>
            <li>
              navigate most of the website using a keyboard or speech
              recognition software
            </li>
            <li>
              listen to most of the website using a screen reader (including the
              most recent versions of JAWS, NVDA and VoiceOver)
            </li>
          </ul>
          <p>
            We've also made the website text as simple as possible to
            understand.
          </p>
          <p>
            <a
              rel="noopener noreferrer nofollow"
              target="_blank"
              href="https://mcmw.abilitynet.org.uk/"
            >
              AbilityNet (opens in new tab)
            </a>{' '}
            has advice on making your device easier to use if you have a
            disability.
          </p>
          <h2>How accessible this website is</h2>
          <p>We know some parts of this website are not fully accessible:</p>
          <ul>
            <li>
              some graphics, including geographic charts, used to convey
              information may have poor colour contrast
            </li>
            <li>
              users of voice activation software may have difficulties
              interacting with geographic charts.
            </li>
          </ul>
          <h2>Feedback and contact information</h2>
          <p>
            If you find any problems not listed on this page, need information
            in a different format, or think we're not meeting accessibility
            requirements, contact us:
          </p>
          <ul>
            <li>
              email{' '}
              <a href="mailto:explore.statistics@education.gov.uk">
                explore.statistics@education.gov.uk
              </a>
            </li>
          </ul>
          <p>In your message, include:</p>
          <ul>
            <li>the web address (URL) of the content</li>
          </ul>
          <h2>Enforcement procedure</h2>
          <p>
            The Equality and Human Rights Commission (EHRC) is responsible for
            enforcing the Public Sector Bodies (Websites and Mobile
            Applications) (No. 2) Accessibility Regulations 2018 (the
            'accessibility regulations').{' '}
          </p>
          <p>
            If you are not happy with how we respond to your complaint, contact
            the{' '}
            <a
              href="https://www.equalityadvisoryservice.com/"
              rel="noopener noreferrer nofollow"
              target="_blank"
            >
              Equality Advisory and Support Service (EASS) (opens in new tab)
            </a>
            .
          </p>
          <h2>Technical information about this website's accessibility</h2>
          <p>
            The Department for Education (DfE) is committed to making its
            website accessible, in accordance with the Public Sector Bodies
            (Websites and Mobile Applications) (No. 2) Accessibility Regulations
            2018.
          </p>
          <h2>Compliance status</h2>
          <p>
            This website is partially compliant with the{' '}
            <a
              href="https://www.w3.org/TR/WCAG22/"
              rel="noopener noreferrer nofollow"
              target="_blank"
            >
              Web Content Accessibility Guidelines version 2.2 (opens in a new
              tab)
            </a>{' '}
            AA standard, due to the non-compliances listed below.
          </p>
          <h2>Non accessible content</h2>
          <p>
            The content listed below is non-accessible for the following
            reasons. We will address these issues during our public beta phase
            to ensure our content is accessible.
          </p>
          <ol>
            <li>
              On publication pages with embedded charts, some colour elements
              used to distinguish different ranges of results have low contrast.
              This means it may be difficult for some low vision users to
              decipher. This doesn't meet WCAG 2.2 (Level AA) success criterion
              1,4,11 (understanding non-text contrast).
            </li>
          </ol>
          <h3>Disproportionate burden</h3>
          <p>Not applicable.</p>
          <h2>How we tested this website</h2>
          <p>
            This website was last tested on 19th August 2025 against{' '}
            <a
              href="https://www.w3.org/TR/WCAG22/"
              rel="noopener noreferrer nofollow"
              target="_blank"
            >
              Accessibility Guidelines WCAG2.2 (opens in new tab)
            </a>
            .
          </p>
          <p>
            The test was carried out by the{' '}
            <a
              href="https://digitalaccessibilitycentre.org/"
              rel="noopener noreferrer nofollow"
              target="_blank"
            >
              Digital accessibility centre (DAC) (opens in new tab)
            </a>
            . The service was tested by a team of experienced auditors and
            analysts, many of who are disabled individuals and users of adaptive
            technology.
          </p>
          <p>
            DAC tested a sample of pages to cover the core functionality of the
            service including:
          </p>
          <ul>
            <li>
              <Link to="/">the homepage</Link>
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
              <Link to="/data-tables">table tool page</Link>
            </li>
            <li>
              <Link to="/data-catalogue">data catalogue page</Link>
            </li>
            <li>
              <Link to="/methodology">methodology homepage</Link>
            </li>
            <li>
              <Link to="/methodology/pupil-absence-statistics-methodology">
                specific release methodology
              </Link>
            </li>
            <li>
              <Link to="/glossary">glossary page</Link>
            </li>
            <li>
              <Link to="/subscriptions/new-subscription/pupil-absence-in-schools-in-england">
                notify me page
              </Link>
            </li>
          </ul>
          <h2>What we're doing to improve accessibility</h2>
          <p>
            We plan to continually test the service for accessibility issues,
            and are working through a prioritised list of issues to resolve.
          </p>
          <h3>Preparation of this accessibility statement</h3>
          <p>
            This statement was prepared on 4 December 2019. It was last reviewed
            on 25 November 2025.
          </p>
          <p>
            This website was last tested in August 2025 against the WCAG 2.2 AA
            standard. This test of a representative sample of pages was carried
            out by the Digital Accessibility Centre (DAC).
          </p>
          <p>
            We also used findings from our own testing when preparing this
            accessibility statement.
          </p>
        </div>
      </div>
    </Page>
  );
}

export default AcccessibilityStatementPage;
