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
      <h2 className="govuk-heading-l">Early years</h2>
      <Accordion id="early-years">
        <AccordionSection heading="Attainment and outcomes" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <ul className="govuk-list-bullet">
              <li>
                {' '}
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Childcare and early years
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
              <li className="govuk-!-margin-top-6">
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Early years foundation stage profile
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
              <li className="govuk-!-margin-top-6">
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Education and training
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
              <li className="govuk-!-margin-top-6">
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Parental responsibility measures
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
              <li className="govuk-!-margin-top-6">
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Special educational needs (SEN)
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
            </ul>
          </div>
        </AccordionSection>
        <AccordionSection heading="Finance" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <ul className="govuk-list-bullet">
              <li>
                {' '}
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Education and training
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
            </ul>
          </div>
        </AccordionSection>
        <AccordionSection heading="Institutions" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <ul className="govuk-list-bullet">
              <li>
                {' '}
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Childcare and early years
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
              <li>
                {' '}
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Education and training
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
            </ul>
          </div>
        </AccordionSection>
        <AccordionSection
          heading="Participation and characteristics"
          caption=""
        >
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <ul className="govuk-list-bullet">
              <li>
                {' '}
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Childcare and early years
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
              <li className="govuk-!-margin-top-6">
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Education and training
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
              <li className="govuk-!-margin-top-6">
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Parental responsibility measures
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
              <li className="govuk-!-margin-top-6">
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Pupil projections
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
              <li className="govuk-!-margin-top-6">
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Special educational needs (SEN)
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
            </ul>
          </div>
        </AccordionSection>
      </Accordion>
      <h2 className="govuk-heading-l govuk-!-margin-top-9">
        Further education
      </h2>
      <Accordion id="further-education">
        <AccordionSection heading="Attainment and outcomes" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <ul className="govuk-list-bullet">
              <li>
                {' '}
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  16 to 19 attainment
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
              <li className="govuk-!-margin-top-6">
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Education and training
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
              <li className="govuk-!-margin-top-6">
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Further education and skills
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
              <li className="govuk-!-margin-top-6">
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Further education for benefits claimants
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
              <li className="govuk-!-margin-top-6">
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  National achievement rates tables
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
              <li className="govuk-!-margin-top-6">
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Outcome based success measures
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
              <li className="govuk-!-margin-top-6">
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Performance tables
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
            </ul>
          </div>
        </AccordionSection>
        <AccordionSection heading="Finance" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <ul className="govuk-list-bullet">
              <li>
                {' '}
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Advance learner loans
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
              <li className="govuk-!-margin-top-6">
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Education and training
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
              <li className="govuk-!-margin-top-6">
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Further education and skills
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
            </ul>
          </div>
        </AccordionSection>
        <AccordionSection heading="Institutions" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <ul className="govuk-list-bullet">
              <li className="govuk-!-margin-top-6">
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Education and training
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
            </ul>
          </div>
        </AccordionSection>
        <AccordionSection
          heading="Participation and characteristics"
          caption=""
        >
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <ul className="govuk-list-bullet">
              <li>
                {' '}
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Education and training
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
              <li className="govuk-!-margin-top-6">
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  FE Choices
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
              <li className="govuk-!-margin-top-6">
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Further education and skills
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
              <li className="govuk-!-margin-top-6">
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Further education for benefits claimants
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
              <li className="govuk-!-margin-top-6">
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Not in education, employment or training (NEET) and
                  participation
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
            </ul>
          </div>
        </AccordionSection>
      </Accordion>
      <h2 className="govuk-heading-l govuk-!-margin-top-9">Higher education</h2>
      <Accordion id="higher-education">
        <AccordionSection heading="Attainment and outcomes" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <ul className="govuk-list-bullet">
              <li>
                {' '}
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Education and training
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
              <li className="govuk-!-margin-top-6">
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Graduate labour market
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
              <li className="govuk-!-margin-top-6">
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Higher education graduate employment and earnings
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
              <li className="govuk-!-margin-top-6">
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Higher education statistics
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
              <li className="govuk-!-margin-top-6">
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Initial teacher training (ITT)
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
              <li className="govuk-!-margin-top-6">
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Performance tables
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
              <li className="govuk-!-margin-top-6">
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Widening participation in higher education
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
            </ul>
          </div>
        </AccordionSection>

        <AccordionSection heading="Finance" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <ul className="govuk-list-bullet">
              <li>
                {' '}
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Education and training
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
              <li className="govuk-!-margin-top-6">
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Higher education graduate employment and earnings
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
              <li className="govuk-!-margin-top-6">
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Student loan forecasts
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
            </ul>
          </div>
        </AccordionSection>

        <AccordionSection heading="Institutions" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <ul className="govuk-list-bullet">
              <li>
                {' '}
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Education and training
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
              <li className="govuk-!-margin-top-6">
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Higher education statistics
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
            </ul>
          </div>
        </AccordionSection>

        <AccordionSection
          heading="Participation and characteristics"
          caption=""
        >
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <ul className="govuk-list-bullet">
              <li>
                {' '}
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Education and training
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
              <li className="govuk-!-margin-top-6">
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Higher education graduate employment and earnings
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
              <li className="govuk-!-margin-top-6">
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Higher education
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
              <li className="govuk-!-margin-top-6">
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Initial teacher training (ITT)
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
              <li className="govuk-!-margin-top-6">
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Not in education, employment or training (NEET) and
                  participation
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
              <li className="govuk-!-margin-top-6">
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Participation rates in higher education
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
              <li className="govuk-!-margin-top-6">
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Widening participation in higher education
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
            </ul>
          </div>
        </AccordionSection>

        <AccordionSection heading="Workforce" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <ul className="govuk-list-bullet">
              <li>
                {' '}
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Higher education
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
            </ul>
          </div>
        </AccordionSection>
      </Accordion>

      <h2 className="govuk-heading-l govuk-!-margin-top-9">Schools</h2>
      <Accordion id="schools">
        <AccordionSection heading="Attainment and outcomes" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <ul className="govuk-list-bullet">
              <li>
                {' '}
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  16 to 19 attainment
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
              <li>
                {' '}
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Destinations of key stage 4 and key stage 5 pupils
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
              <li>
                {' '}
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Education and training
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
              <li>
                {' '}
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  GCSEs (key stage 4) and equivalent results
                </h4>
                <p className="govuk-body">
                  View statistics, create charts and tables and download data
                  files for GCSE and equivalent results in England
                </p>
                <div className="govuk-!-margin-top-0">
                  <PrototypeDownloadDropdown link="/prototypes/publication-gcse" />
                </div>
              </li>
              <li className="govuk-!-margin-top-6">
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Key stage 1
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
              <li className="govuk-!-margin-top-6">
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Key stage 2
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
              <li className="govuk-!-margin-top-6">
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Parental responsibility measures
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
              <li className="govuk-!-margin-top-6">
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Performance tables
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
              <li className="govuk-!-margin-top-6">
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Special educational needs (SEN)
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
            </ul>
          </div>
        </AccordionSection>

        <AccordionSection heading="Finance" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <ul className="govuk-list-bullet">
              <li>
                {' '}
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Education and training
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
              <li className="govuk-!-margin-top-6">
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Local authority and school finance
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
            </ul>
          </div>
        </AccordionSection>

        <AccordionSection heading="Institutions" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <ul className="govuk-list-bullet">
              <li>
                {' '}
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Admission appeals
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
              <li className="govuk-!-margin-top-6">
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Education and training
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
              <li>
                {' '}
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  GCSEs (key stage 4) and equivalent results
                </h4>
                <p className="govuk-body">
                  View statistics, create charts and tables and download data
                  files for GCSE and equivalent results in England
                </p>
                <div className="govuk-!-margin-top-0">
                  <PrototypeDownloadDropdown link="/prototypes/publication-gcse" />
                </div>
              </li>
              <li className="govuk-!-margin-top-6">
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  School applications
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
              <li className="govuk-!-margin-top-6">
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  School capacity
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
              <li className="govuk-!-margin-top-6">
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  School and pupil numbers
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
              <li className="govuk-!-margin-top-6">
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Workforce statistics and analysis
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
            </ul>
          </div>
        </AccordionSection>

        <AccordionSection
          heading="Participation and characteristics"
          caption=""
        >
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <ul className="govuk-list-bullet">
              <li>
                {' '}
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  16 to 19 attainment
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
              <li>
                {' '}
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Admission appeals
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
              <li>
                {' '}
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Destinations of key stage 4 and key stage 5 pupils
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
              <li className="govuk-!-margin-top-6">
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Exclusions
                </h4>
                <p className="govuk-body">
                  View statistics, create charts and tables and download data
                  files for fixed-period and permanent exclusion statistics
                </p>
                <div className="govuk-!-margin-top-0">
                  <PrototypeDownloadDropdown link="/prototypes/publication-exclusions" />
                </div>
              </li>
              <li>
                {' '}
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Education and training
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
              <li className="govuk-!-margin-top-6">
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Not in education, employment or training (NEET) and
                  participation
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
              <li className="govuk-!-margin-top-6">
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Parental responsibility measures
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
              <li>
                {' '}
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Pupil absence
                </h4>
                <p className="govuk-body">
                  View statistics, create charts and tables and download data
                  files for authorised, overall, persistent and unauthorised
                  absence
                </p>
                <div className="govuk-!-margin-top-0">
                  <PrototypeDownloadDropdown />
                </div>
              </li>
              <li className="govuk-!-margin-top-6">
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Pupil projections
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
              <li className="govuk-!-margin-top-6">
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  School and pupil numbers
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
              <li className="govuk-!-margin-top-6">
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  School applications
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
              <li className="govuk-!-margin-top-6">
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Special educational needs (SEN)
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
            </ul>
          </div>
        </AccordionSection>

        <AccordionSection heading="Workforce" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <ul className="govuk-list-bullet">
              <li>
                {' '}
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Initial teacher training (ITT)
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
              <li className="govuk-!-margin-top-6">
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  School workforce
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
              <li className="govuk-!-margin-top-6">
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Workforce statistics and analysis
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
            </ul>
          </div>
        </AccordionSection>
      </Accordion>
      <h2 className="govuk-heading-l govuk-!-margin-top-9">Social care</h2>
      <Accordion id="social">
        <AccordionSection heading="Children’s social care" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <ul className="govuk-list-bullet">
              <li>
                {' '}
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Children in need and child protection
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
              <li className="govuk-!-margin-top-6">
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Looked-after children
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
              <li className="govuk-!-margin-top-6">
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Secure children’s homes
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
            </ul>
          </div>
        </AccordionSection>
        <AccordionSection heading="Institutions" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <ul className="govuk-list-bullet">
              <li>
                {' '}
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Secure children’s homes
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
            </ul>
          </div>
        </AccordionSection>
        <AccordionSection heading="Workforce" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <ul className="govuk-list-bullet">
              <li>
                {' '}
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Children's social work workforce
                </h4>
                <div className="govuk-inset-text">
                  These statistics and data are not yet available on the explore
                  education statistics service. To find and download these
                  statistics and data browse{' '}
                  <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                    Statistics at DfE
                  </a>
                </div>
              </li>
            </ul>
          </div>
        </AccordionSection>
      </Accordion>
    </PrototypePage>
  );
};

export default BrowseReleasesPage;
