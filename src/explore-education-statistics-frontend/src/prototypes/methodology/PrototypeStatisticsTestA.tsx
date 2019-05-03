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
      <h1 className="govuk-heading-xl">Find statistics and download data</h1>
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <p className="govuk-body-l">
            Browse to find the statistics and data you’re looking for and open
            the section to get links to:
          </p>
          <ul className="govuk-bulllet-list govuk-!-margin-bottom-9">
            <li>
              up-to-date national statistical headlines, breakdowns and
              explanations
            </li>
            <li>
              charts and tables to help you compare, contrast and view national
              and regional statistical data and trends
            </li>
            <li>
              our table tool to build your own tables online and explore our
              range of national and regional data
            </li>
            <li>
              links to underlying data so you can download files and carry out
              your own statistical analysis
            </li>
          </ul>
        </div>
        <div className="govuk-grid-column-one-third">
          <aside className="app-related-items">
            <h2 className="govuk-heading-m" id="releated-content">
              Related content
            </h2>
            <nav role="navigation" aria-labelledby="subsection-title">
              <ul className="govuk-list">
                <li>
                  <Link to="/prototypes/methodology-home">
                    Education statistics: methodology
                  </Link>
                </li>
                <li>
                  <Link to="https://eesadminprototype.z33.web.core.windows.net/prototypes/documentation/glossary">
                    Education statistics: glossary
                  </Link>
                </li>
              </ul>
            </nav>
          </aside>
        </div>
      </div>

      <Accordion id="early-years">
        <AccordionSection heading="Early years">
          <h3>Attainment and outcomes</h3>
          <ul className="govuk-list-bullet">
            <li>
              <strong>Childcare and early years</strong> - These statistics are
              not yet available on the explore education statistics service.
              Find them on GOV.UK under{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                Statistics DfE
              </a>
            </li>
          </ul>
          <ul>
            <li>
              <strong>Early years foundation stage profile</strong> - These
              statistics are not yet available on the explore education
              statistics service. Find them on GOV.UK under{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                Statistics DfE
              </a>
            </li>
          </ul>
          <ul>
            <li>
              <strong>Education and training</strong> - These statistics are not
              yet available on the explore education statistics service. Find
              them on GOV.UK under{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                Statistics DfE
              </a>
            </li>
          </ul>
          <ul>
            <li>
              <strong>Parental responsibility measures</strong> - These
              statistics are not yet available on the explore education
              statistics service. Find them on GOV.UK under{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                Statistics DfE
              </a>
            </li>
          </ul>
          <ul>
            <li>
              <strong>Special educational needs (SEN)</strong> - These
              statistics are not yet available on the explore education
              statistics service. Find them on GOV.UK under{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                Statistics DfE
              </a>
            </li>
          </ul>
          <h3>Finance</h3>
          <ul className="govuk-list-bullet">
            <li>
              <Link to="#">Admissions and appeals statistics: methodology</Link>
            </li>
            <li>
              <Link to="#">School capacity statistics: methodology</Link>
            </li>
          </ul>
          <h3>Institutions</h3>
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
          <h3>Participants and characteristics</h3>
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
        </AccordionSection>
        <AccordionSection heading="Further education">
          <h3>Attainment and outcomes</h3>
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
        <AccordionSection heading="Higher education">
          <h3>Attainment and outcomes</h3>
          <ul className="govuk-list-bullet">
            <li>
              <Link to="#">Children in need statistics: methodology</Link>
            </li>
            <li>
              <Link to="#">Looked after children statistics: methodology</Link>
            </li>
          </ul>
        </AccordionSection>
        <AccordionSection heading="Schools">
          <h3>Attainment and outcomes</h3>
          <ul className="govuk-list-bullet">
            <li>
              <Link to="#">Children in need statistics: methodology</Link>
            </li>
            <li>
              <Link to="#">Looked after children statistics: methodology</Link>
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
