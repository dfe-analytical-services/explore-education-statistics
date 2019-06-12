import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import RelatedInformation from '@common/components/RelatedInformation';
import SearchForm from '@common/components/SearchForm';
import Link from '@frontend/components/Link';
import Page from '@frontend/components/Page';
import PageTitle from '@frontend/components/PageTitle';
import React from 'react';

function MethodologyIndexPage() {
  return (
    <Page
      breadcrumbs={[
        {
          link: '/methodology',
          name: 'Methodology',
        },
      ]}
    >
      <PageTitle title="Education statistics: methodology" />
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <p className="govuk-body-l">
            Browse to find out about the methodology behind specific education
            statistics and data and how and why they're collected and published.
          </p>

          <SearchForm />
        </div>
        <div className="govuk-grid-column-one-third">
          <RelatedInformation>
            <ul className="govuk-list">
              <li>
                <Link to="/statistics">Find statistics and data</Link>
              </li>
              <li>
                <Link to="/glossary">Education statistics: glossary</Link>
              </li>
            </ul>
          </RelatedInformation>
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
              <Link to="#">Pupil absence statistics: methodology</Link>
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
    </Page>
  );
}

export default MethodologyIndexPage;
