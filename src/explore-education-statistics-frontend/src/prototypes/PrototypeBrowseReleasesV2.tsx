import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import React from 'react';
import Link from '../components/Link';
import PrototypeDownloadDropdown from './components/PrototypeDownloadDropdown';
import PrototypePage from './components/PrototypePage';

const BrowseReleasesPage = () => {
  return (
    <PrototypePage
      breadcrumbs={[{ text: 'Find statistics and download data' }]}
    >
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <h1 className="govuk-heading-xl">
            Find statistics and download data
          </h1>
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
      <h2 className="govuk-heading-l">
        Children and early years - including social care
      </h2>
      <Accordion id="children-and-early-years">
        <AccordionSection heading="Childcare and early years" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <div className="govuk-inset-text">
              These statistics and data are not yet available on the explore
              education statistics service. To find and download these
              statistics and data browse{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                Statistics at DfE
              </a>
            </div>
          </div>
        </AccordionSection>
        <AccordionSection
          heading="Children in need and child protection"
          caption=""
        >
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <div className="govuk-inset-text">
              These statistics and data are not yet available on the explore
              education statistics service. To find and download these
              statistics and data browse{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                Statistics at DfE
              </a>
            </div>
          </div>
        </AccordionSection>
        <AccordionSection
          heading="Early years foundation stage profile"
          caption=""
        >
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <div className="govuk-inset-text">
              These statistics and data are not yet available on the explore
              education statistics service. To find and download these
              statistics and data browse{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                Statistics at DfE
              </a>
            </div>
          </div>
        </AccordionSection>
        <AccordionSection heading="Looked-after children" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <div className="govuk-inset-text">
              These statistics and data are not yet available on the explore
              education statistics service. To find and download these
              statistics and data browse{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                Statistics at DfE
              </a>
            </div>
          </div>
        </AccordionSection>
        <AccordionSection heading="Secure children's homes" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <div className="govuk-inset-text">
              These statistics and data are not yet available on the explore
              education statistics service. To find and download these
              statistics and data browse{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                Statistics at DfE
              </a>
            </div>
          </div>
        </AccordionSection>
        <AccordionSection heading="Special educational needs (SEN)" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <div className="govuk-inset-text">
              These statistics and data are not yet available on the explore
              education statistics service. To find and download these
              statistics and data browse{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                Statistics at DfE
              </a>
            </div>
          </div>
        </AccordionSection>
      </Accordion>
      <h2 className="govuk-heading-l govuk-!-margin-top-9">
        Finance and funding
      </h2>
      <Accordion id="finance-and-funding">
        <AccordionSection heading="Advanced learner loans" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <div className="govuk-inset-text">
              These statistics and data are not yet available on the explore
              education statistics service. To find and download these
              statistics and data browse{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                Statistics at DfE
              </a>
            </div>
          </div>
        </AccordionSection>
        <AccordionSection
          heading="Local authority and school finance"
          caption=""
        >
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <div className="govuk-inset-text">
              These statistics and data are not yet available on the explore
              education statistics service. To find and download these
              statistics and data browse{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                Statistics at DfE
              </a>
            </div>
          </div>
        </AccordionSection>
        <AccordionSection heading="Student loan forecasts" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <div className="govuk-inset-text">
              These statistics and data are not yet available on the explore
              education statistics service. To find and download these
              statistics and data browse{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                Statistics at DfE
              </a>
            </div>
          </div>
        </AccordionSection>
      </Accordion>
      <h2 className="govuk-heading-l govuk-!-margin-top-9">
        Further education
      </h2>
      <Accordion id="further-education">
        <AccordionSection heading="Education and training" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <div className="govuk-inset-text">
              These statistics and data are not yet available on the explore
              education statistics service. To find and download these
              statistics and data browse{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                Statistics at DfE
              </a>
            </div>
          </div>
        </AccordionSection>
        <AccordionSection heading="FE Choices" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <div className="govuk-inset-text">
              These statistics and data are not yet available on the explore
              education statistics service. To find and download these
              statistics and data browse{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                Statistics at DfE
              </a>
            </div>
          </div>
        </AccordionSection>
        <AccordionSection heading="Further education and skills" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <div className="govuk-inset-text">
              These statistics and data are not yet available on the explore
              education statistics service. To find and download these
              statistics and data browse{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                Statistics at DfE
              </a>
            </div>
          </div>
        </AccordionSection>
        <AccordionSection
          heading="Further education for benefits claimants"
          caption=""
        >
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <div className="govuk-inset-text">
              These statistics and data are not yet available on the explore
              education statistics service. To find and download these
              statistics and data browse{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                Statistics at DfE
              </a>
            </div>
          </div>
        </AccordionSection>

        <AccordionSection
          heading="National achievement rates tables"
          caption=""
        >
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <div className="govuk-inset-text">
              These statistics and data are not yet available on the explore
              education statistics service. To find and download these
              statistics and data browse{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                Statistics at DfE
              </a>
            </div>
          </div>
        </AccordionSection>

        <AccordionSection
          heading="Not in education, employment or training (NEET) and participation"
          caption=""
        >
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <div className="govuk-inset-text">
              These statistics and data are not yet available on the explore
              education statistics service. To find and download these
              statistics and data browse{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                Statistics at DfE
              </a>
            </div>
          </div>
        </AccordionSection>
      </Accordion>

      <h2 className="govuk-heading-l govuk-!-margin-top-9">Higher education</h2>
      <Accordion id="higher-education">
        <AccordionSection heading="Education and training" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <div className="govuk-inset-text">
              These statistics and data are not yet available on the explore
              education statistics service. To find and download these
              statistics and data browse{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                Statistics at DfE
              </a>
            </div>
          </div>
        </AccordionSection>
        <AccordionSection
          heading="Higher education graduate employment and earnings"
          caption=""
        >
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <div className="govuk-inset-text">
              These statistics and data are not yet available on the explore
              education statistics service. To find and download these
              statistics and data browse{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                Statistics at DfE
              </a>
            </div>
          </div>
        </AccordionSection>
        <AccordionSection heading="Higher education statistics" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <div className="govuk-inset-text">
              These statistics and data are not yet available on the explore
              education statistics service. To find and download these
              statistics and data browse{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                Statistics at DfE
              </a>
            </div>
          </div>
        </AccordionSection>
        <AccordionSection
          heading="Not in education, employment or training (NEET) and participation"
          caption=""
        >
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <div className="govuk-inset-text">
              These statistics and data are not yet available on the explore
              education statistics service. To find and download these
              statistics and data browse{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                Statistics at DfE
              </a>
            </div>
          </div>
        </AccordionSection>
        <AccordionSection
          heading="Participation rates in higher education"
          caption=""
        >
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <div className="govuk-inset-text">
              These statistics and data are not yet available on the explore
              education statistics service. To find and download these
              statistics and data browse{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                Statistics at DfE
              </a>
            </div>
          </div>
        </AccordionSection>
        <AccordionSection
          heading="Widening participation in higher education"
          caption=""
        >
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <div className="govuk-inset-text">
              These statistics and data are not yet available on the explore
              education statistics service. To find and download these
              statistics and data browse{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                Statistics at DfE
              </a>
            </div>
          </div>
        </AccordionSection>
      </Accordion>

      <h2 className="govuk-heading-l govuk-!-margin-top-9">
        Performance - including GCSE and key stage results
      </h2>
      <Accordion id="performance">
        <AccordionSection heading="16 to 19 attainment" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <div className="govuk-inset-text">
              These statistics and data are not yet available on the explore
              education statistics service. To find and download these
              statistics and data browse{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                Statistics at DfE
              </a>
            </div>
          </div>
        </AccordionSection>
        <AccordionSection
          heading="Destinations of key stage 4 and key stage 5 pupils"
          caption=""
        >
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <div className="govuk-inset-text">
              These statistics and data are not yet available on the explore
              education statistics service. To find and download these
              statistics and data browse{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                Statistics at DfE
              </a>
            </div>
          </div>
        </AccordionSection>
        <AccordionSection
          heading="GCSEs (key stage 4) and equivalent results"
          caption=""
        >
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <p className="govuk-body">
              View statistics, create charts and tables and download data files
              for GCSE and equivalent results in England
            </p>
            <div className="govuk-!-margin-top-0">
              <PrototypeDownloadDropdown link="/prototypes/publication-gcse" />
            </div>
          </div>
        </AccordionSection>
        <AccordionSection heading="Key stage 1" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <div className="govuk-inset-text">
              These statistics and data are not yet available on the explore
              education statistics service. To find and download these
              statistics and data browse{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                Statistics at DfE
              </a>
            </div>
          </div>
        </AccordionSection>
        <AccordionSection heading="Key stage 2" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <div className="govuk-inset-text">
              These statistics and data are not yet available on the explore
              education statistics service. To find and download these
              statistics and data browse{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                Statistics at DfE
              </a>
            </div>
          </div>
        </AccordionSection>
        <AccordionSection heading="Outcome based success measures" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <div className="govuk-inset-text">
              These statistics and data are not yet available on the explore
              education statistics service. To find and download these
              statistics and data browse{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                Statistics at DfE
              </a>
            </div>
          </div>
        </AccordionSection>
        <AccordionSection heading="Parental responsibility measures" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <div className="govuk-inset-text">
              These statistics and data are not yet available on the explore
              education statistics service. To find and download these
              statistics and data browse{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                Statistics at DfE
              </a>
            </div>
          </div>
        </AccordionSection>
        <AccordionSection heading="Performance tables" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <div className="govuk-inset-text">
              These statistics and data are not yet available on the explore
              education statistics service. To find and download these
              statistics and data browse{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                Statistics at DfE
              </a>
            </div>
          </div>
        </AccordionSection>
        <AccordionSection
          heading="Widening participation in higher education"
          caption=""
        >
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <div className="govuk-inset-text">
              These statistics and data are not yet available on the explore
              education statistics service. To find and download these
              statistics and data browse{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                Statistics at DfE
              </a>
            </div>
          </div>
        </AccordionSection>
      </Accordion>

      <h2 className="govuk-heading-l govuk-!-margin-top-9">
        Pupils and schools
      </h2>
      <Accordion id="pupils-and-schools">
        <AccordionSection heading="Admission appeals" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <div className="govuk-inset-text">
              These statistics and data are not yet available on the explore
              education statistics service. To find and download these
              statistics and data browse{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                Statistics at DfE
              </a>
            </div>
          </div>
        </AccordionSection>
        <AccordionSection heading="Exclusions" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <p className="govuk-body">
              View statistics, create charts and tables and download data files
              for fixed-period and permanent exclusion statistics
            </p>
            <div className="govuk-!-margin-top-0">
              <PrototypeDownloadDropdown link="/prototypes/publication-exclusions" />
            </div>
          </div>
        </AccordionSection>
        <AccordionSection
          heading="Not in education, employment or training (NEET) and participation"
          caption=""
        >
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <p className="govuk-body">
              View statistics, create charts and tables and download data files
              for fixed-period and permanent exclusion statistics
            </p>
            <div className="govuk-!-margin-top-0">
              <PrototypeDownloadDropdown link="/prototypes/publication-exclusions" />
            </div>
          </div>
        </AccordionSection>
        <AccordionSection heading="Pupil absence" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <p className="govuk-body">
              View statistics, create charts and tables and download data files
              for authorised, overall, persistent and unauthorised absence
            </p>
            <div className="govuk-!-margin-top-0">
              <PrototypeDownloadDropdown />
            </div>
          </div>
        </AccordionSection>
        <AccordionSection heading="Pupil projections" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <div className="govuk-inset-text">
              These statistics and data are not yet available on the explore
              education statistics service. To find and download these
              statistics and data browse{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                Statistics at DfE
              </a>
            </div>
          </div>
        </AccordionSection>
        <AccordionSection heading="School and pupil numbers" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <div className="govuk-inset-text">
              These statistics and data are not yet available on the explore
              education statistics service. To find and download these
              statistics and data browse{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                Statistics at DfE
              </a>
            </div>
          </div>
        </AccordionSection>
        <AccordionSection heading="School applications" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <div className="govuk-inset-text">
              These statistics and data are not yet available on the explore
              education statistics service. To find and download these
              statistics and data browse{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                Statistics at DfE
              </a>
            </div>
          </div>
        </AccordionSection>
        <AccordionSection heading="School capacity" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <div className="govuk-inset-text">
              These statistics and data are not yet available on the explore
              education statistics service. To find and download these
              statistics and data browse{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                Statistics at DfE
              </a>
            </div>
          </div>
        </AccordionSection>
        <AccordionSection heading="School workforce" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <div className="govuk-inset-text">
              These statistics and data are not yet available on the explore
              education statistics service. To find and download these
              statistics and data browse{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                Statistics at DfE
              </a>
            </div>
          </div>
        </AccordionSection>
        <AccordionSection heading="Special educational needs (SEN)" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <div className="govuk-inset-text">
              These statistics and data are not yet available on the explore
              education statistics service. To find and download these
              statistics and data browse{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                Statistics at DfE
              </a>
            </div>
          </div>
        </AccordionSection>
      </Accordion>

      <h2 className="govuk-heading-l govuk-!-margin-top-9">
        Teachers and workforce
      </h2>
      <Accordion id="teachers-and-workforce">
        <AccordionSection heading="Children's social work workforce" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <div className="govuk-inset-text">
              These statistics and data are not yet available on the explore
              education statistics service. To find and download these
              statistics and data browse{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                Statistics at DfE
              </a>
            </div>
          </div>
        </AccordionSection>
        <AccordionSection heading="Graduate labour market" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <div className="govuk-inset-text">
              These statistics and data are not yet available on the explore
              education statistics service. To find and download these
              statistics and data browse{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                Statistics at DfE
              </a>
            </div>
          </div>
        </AccordionSection>
        <AccordionSection
          heading="Higher education graduate employment and earnings"
          caption=""
        >
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <div className="govuk-inset-text">
              These statistics and data are not yet available on the explore
              education statistics service. To find and download these
              statistics and data browse{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                Statistics at DfE
              </a>
            </div>
          </div>
        </AccordionSection>
        <AccordionSection heading="Initial teacher training (ITT)" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <div className="govuk-inset-text">
              These statistics and data are not yet available on the explore
              education statistics service. To find and download these
              statistics and data browse{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                Statistics at DfE
              </a>
            </div>
          </div>
        </AccordionSection>
        <AccordionSection
          heading="Not in education, employment or training (NEET) and participation"
          caption=""
        >
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <div className="govuk-inset-text">
              These statistics and data are not yet available on the explore
              education statistics service. To find and download these
              statistics and data browse{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                Statistics at DfE
              </a>
            </div>
          </div>
        </AccordionSection>
        <AccordionSection heading="School workforce" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <div className="govuk-inset-text">
              These statistics and data are not yet available on the explore
              education statistics service. To find and download these
              statistics and data browse{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                Statistics at DfE
              </a>
            </div>
          </div>
        </AccordionSection>
        <AccordionSection
          heading="Workforce statistics and analysis"
          caption=""
        >
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <div className="govuk-inset-text">
              These statistics and data are not yet available on the explore
              education statistics service. To find and download these
              statistics and data browse{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                Statistics at DfE
              </a>
            </div>
          </div>
        </AccordionSection>
      </Accordion>
    </PrototypePage>
  );
};

export default BrowseReleasesPage;
