import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import Link from '@frontend/components/Link';
import Page from '@frontend/components/Page';
import PageTitle from '@frontend/components/PageTitle';
import PrototypeSearchForm from '@frontend/prototypes/components/PrototypeSearchForm';
import React from 'react';

function DownloadIndexPage() {
  return (
    <Page
      breadcrumbs={[
        {
          link: '/download',
          name: 'Download data',
        },
        {
          link: '/download/pupils-schools',
          name: 'Pupils and schools',
        },
      ]}
    >
      <PageTitle title="Download data files for pupils and schools" />
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <p className="govuk-body-l">
            Browse topics and download data files. Files are currently available
            to download in .csv format.
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
                  <Link to="/statistics">Find statistics and data</Link>
                </li>
                <li>
                  <Link to="/glossary">Education statistics: glossary</Link>
                </li>
              </ul>
            </nav>
          </aside>
          <div className="govuk-!-margin-top-9 govuk-!-margin-bottom-9">
            <PrototypeSearchForm />
          </div>
        </div>
      </div>

      <h2 className="govuk-heading-l">
        Browse topics{' '}
        <Link className="govuk-body govuk-!-margin-left-2" to="/download">
          {' '}
          Change theme
        </Link>
      </h2>

      <Accordion id="pupils-and-schools">
        <AccordionSection heading="Admission appeals" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <ul className="govuk-bullet-list govuk-!-margin-top-0 govuk-!-margin-bottom-9">
              <li>
                <p className="govuk-!-margin-top-0">
                  Admissions appeals in England - currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-admission-appeals#documents">
                    Statistics at DfE
                  </a>
                </p>
              </li>
            </ul>
          </div>
        </AccordionSection>
        <AccordionSection heading="Exclusions" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0 govuk-!-margin-bottom-9">
            <ul className="govuk-bulllet-list govuk-!-margin-top-0 ">
              <li>
                <p className="govuk-!-margin-top-0">
                  <Link to="#">
                    Download permanent and fixed-period exclusions in England
                  </Link>{' '}
                  <strong>(.csv, 55mb)</strong>
                </p>
              </li>
            </ul>
          </div>
        </AccordionSection>
        <AccordionSection heading="Pupil absence" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0 govuk-!-margin-bottom-9">
            <ul className="govuk-bullet-list govuk-!-margin-top-0">
              <li>
                <p className="govuk-!-margin-top-0">
                  <Link to="#">
                    Download pupil absence in schools in England
                  </Link>{' '}
                  <strong>(.csv, 55mb)</strong>
                </p>
              </li>
              <li className="govuk-!-margin-top-3">
                <p>
                  Pupil absence in schools in England: autumn term - currently
                  available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-pupil-absence#autumn-term-release">
                    Statistics at DfE
                  </a>
                </p>
              </li>
              <li>
                <p>
                  Pupil absence in schools in England: autumn and spring -
                  currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-pupil-absence#combined-autumn--and-spring-term-release">
                    Statistics at DfE
                  </a>
                </p>
              </li>
            </ul>
          </div>
        </AccordionSection>
        <AccordionSection heading="Parental responsibility measures	" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0 govuk-!-padding-bottom-9">
            <ul className="govuk-bullet-list govuk-!-margin-top-0">
              <li>
                <p className="govuk-!-margin-top-0">
                  Parental responsibility measures - currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/parental-responsibility-measures#official-statistics">
                    Statistics at DfE
                  </a>
                </p>
              </li>
            </ul>
          </div>
        </AccordionSection>
        <AccordionSection heading="Pupil projections" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <ul className="govuk-bullet-list govuk-!-margin-top-0 govuk-!-margin-bottom-9">
              <li>
                <p className="govuk-!-margin-top-0">
                  National pupil projections - currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-pupil-projections#documents">
                    Statistics at DfE
                  </a>
                </p>
              </li>
            </ul>
          </div>
        </AccordionSection>
        <AccordionSection heading="School and pupil numbers" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <ul className="govuk-bullet-list govuk-!-margin-top-0 govuk-!-margin-bottom-9">
              <li>
                <p className="govuk-!-margin-top-0">
                  Schools, pupils and their characteristics - currently
                  available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-school-and-pupil-numbers#documents">
                    Statistics at DfE
                  </a>
                </p>
              </li>
            </ul>
          </div>
        </AccordionSection>
        <AccordionSection heading="School applications" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <ul className="govuk-bullet-list govuk-!-margin-top-0 govuk-!-margin-bottom-9">
              <li>
                <p className="govuk-!-margin-top-0">
                  Secondary and primary schools applications and offers -
                  currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-school-applications#documents">
                    Statistics at DfE
                  </a>
                </p>
              </li>
            </ul>
          </div>
        </AccordionSection>
        <AccordionSection heading="School capacity" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <ul className="govuk-bullet-list govuk-!-margin-top-0 govuk-!-margin-bottom-9">
              <li>
                <p className="govuk-!-margin-top-0">
                  School capacity - currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-school-capacity#school-capacity-data:-by-academic-year">
                    Statistics at DfE
                  </a>
                </p>
              </li>
            </ul>
          </div>
        </AccordionSection>
        <AccordionSection heading="Special educational needs (SEN)" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <ul className="govuk-bullet-list govuk-!-margin-top-0 govuk-!-margin-bottom-9">
              <li>
                <p className="govuk-!-margin-top-0">
                  Special educational needs in England - currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-special-educational-needs-sen#national-statistics-on-special-educational-needs-in-england">
                    Statistics at DfE
                  </a>
                </p>
              </li>
              <li>
                <p>
                  Special educational needs: analysis and summary of data
                  sources - currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-special-educational-needs-sen#analysis-of-children-with-special-educational-needs">
                    Statistics at DfE
                  </a>
                </p>
              </li>
              <li>
                <p>
                  Statements on SEN and EHC plans - currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-special-educational-needs-sen#statements-of-special-educational-needs-(sen)-and-education,-health-and-care-(ehc)-plans">
                    Statistics at DfE
                  </a>
                </p>
              </li>
            </ul>
          </div>
        </AccordionSection>
      </Accordion>
    </Page>
  );
}

export default DownloadIndexPage;
