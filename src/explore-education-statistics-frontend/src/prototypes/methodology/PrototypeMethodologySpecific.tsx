import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import Link from '@frontend/components/Link';
import PrototypePage from '@frontend/prototypes/components/PrototypePage';
import React from 'react';

const BrowseReleasesPage = () => {
  return (
    <PrototypePage
      breadcrumbs={[
        {
          link: '/prototypes/methodology-home',
          text: 'Methodology',
        },
      ]}
    >
      <h1 className="govuk-heading-xl">Education statistics: methodology</h1>
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <p className="govuk-body-l">
            Browse to find out about the methodology behind specific education
            statistics and data and how and why they're collected and published.
          </p>
        </div>
        <div className="govuk-grid-column-one-third">
          <aside className="app-related-items">
            <h2 className="govuk-heading-m" id="releated-content">
              Related content
            </h2>
            <nav role="navigation" aria-labelledby="subsection-title">
              <ul className="govuk-list">
                <li>
                  <Link to="/prototypes/browse-releases">
                    Find statistics and data
                  </Link>
                </li>
              </ul>
            </nav>
          </aside>
        </div>
      </div>

      <Accordion id="methodology-specific">
        <AccordionSection heading="Early years and schools">
          <h3>Absence and exclusions</h3>
          <ul className="govuk-list-bullet">
            <li>
              <Link to="#">Pupil exclusion statistics: methodology</Link>
            </li>
            <li>
              <Link to="/prototypes/methodology-absence">
                Pupil absence statistics: methodology
              </Link>
            </li>
          </ul>
          <hr />
          <h3>Capacity and admissions</h3>
          <ul className="govuk-list-bullet">
            <li>
              <Link to="#">Admissions and appeals statistics: methodology</Link>
            </li>
            <li>
              <Link to="#">School capacity statistics: methodology</Link>
            </li>
          </ul>
          <hr />
          <h3>Results</h3>
          <ul className="govuk-list-bullet">
            <li>
              <Link to="#">Key stage 1 (KS1) statistics: methodology</Link>
            </li>
            <li>
              <Link to="#">Key stage 4 (KS4) statistics: methodology</Link>
            </li>
            <li>
              <Link to="#">
                Phonics screening check and KS1 assessments statistics:
                methodology
              </Link>
            </li>
            <li>
              <Link to="#">
                Early years foundation stage (EYFS) profile statistics:
                methodology
              </Link>
            </li>
          </ul>
          <hr />
          <h3>School and pupil numbers</h3>
          <ul className="govuk-list-bullet">
            <li>
              {' '}
              <Link to="#">
                School pupil characteristics statistics: methodology
              </Link>
            </li>
            <li>
              <Link to="#">School worksforce statistics: methodology</Link>
            </li>
          </ul>
          <hr />
          <h3>Teacher numbers</h3>
          <ul className="govuk-list-bullet">
            <li>
              {' '}
              <Link to="#">
                Initial teacher training (ITT) performance statistics:
                methodology
              </Link>
            </li>
          </ul>
        </AccordionSection>
        <AccordionSection heading="Higher education">
          <h3>Further education</h3>
          <ul className="govuk-list-bullet">
            <li>
              <Link to="#">Destination of leavers statistics: methodology</Link>
            </li>
            <li>
              <Link to="#">
                Apprenticeships and traineeships statistics: methodology
              </Link>
            </li>
            <li>
              <Link to="#">
                Further education and skills statistics: methodology
              </Link>
            </li>
            <li>
              <Link to="#">
                16 to 18 school performance statistics: methodology
              </Link>
            </li>
          </ul>
        </AccordionSection>
        <AccordionSection heading="Social care">
          <h3>Number of children</h3>
          <ul className="govuk-list-bullet">
            <li>
              <Link to="#">Children in need statistics: methodology</Link>
            </li>
            <li>
              <Link to="#">Looked after children statistics: methodology</Link>
            </li>
          </ul>
        </AccordionSection>
      </Accordion>
    </PrototypePage>
  );
};

export default BrowseReleasesPage;
