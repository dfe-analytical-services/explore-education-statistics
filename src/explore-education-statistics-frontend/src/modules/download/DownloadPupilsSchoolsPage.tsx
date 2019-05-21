import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import Details from '@common/components/Details';
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
          <Details summary="Permanent and fixed-period exclusions in England">
            <h3 className="govuk-heading-s">Download data files</h3>
            <p>
              <a href="#">Exclusions by characteristic</a> (csv, 100MB)
            </p>
            <p>
              <a href="#">Exclusions by geographic level</a> (csv, 20MB)
            </p>
            <p>
              <a href="#">Exclusions by reason</a> (csv, 30MB)
            </p>
            <p>
              <a href="#">Duration of fixed exclusions</a> (csv, 5MB)
            </p>
            <p>
              <a href="#">Number of fixed exclusions</a> (csv, 40MB)
            </p>
            <p>
              <a href="#">Total days missed due to fixed period exclusions</a>{' '}
              (csv, 10MB)
            </p>
            <p>
              <a href="#">
                All data for permanent and fixed-period exclusions in England
              </a>{' '}
              (csv, 205MB)
            </p>
          </Details>
        </AccordionSection>
        <AccordionSection heading="Pupil absence" caption="">
          <>
            <Details summary="Pupil absence for schools in England">
              <h3 className="govuk-heading-s">Download data</h3>
              <p>
                <a href="#">Absence by characteristic</a> (csv, 100MB)
              </p>
              <p>
                <a href="#">Absence by geographic level</a> (csv, 10MB)
              </p>
              <p>
                <a href="#">Absence by term</a> (csv, 25MB)
              </p>
              <p>
                <a href="#">Absence for four year olds</a> (csv, 5MB)
              </p>
              <p>
                <a href="#">Absence in prus</a> (csv, 10MB)
              </p>
              <p>
                <a href="#">
                  Absence number missing at least one session by reason
                </a>{' '}
                (csv, 20MB)
              </p>
              <p>
                <a href="#">Absence rate percent bands</a> (csv, 5MB)
              </p>
              <p>
                <a href="#">
                  All data for pupil absence for schools in England
                </a>{' '}
                (csv, 175MB)
              </p>
            </Details>
          </>

          <div className="govuk-!-margin-top-6 govuk-!-padding-top-0 govuk-!-margin-bottom-9">
            <ul className="govuk-bullet-list govuk-!-margin-top-0">
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
