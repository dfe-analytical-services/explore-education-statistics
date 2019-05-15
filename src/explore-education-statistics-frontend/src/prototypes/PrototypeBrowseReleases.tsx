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
          <h1 className="govuk-heading-xl">Find statistics and data</h1>
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
              underlying data so you can download files and carry out your own
              statistical analysis
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
            {' '}
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Childcare and early years
            </h4>
            <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
              <ul className="govuk-bulllet-list govuk-!-margin-bottom-">
                <li>
                  <strong>30 hours free childcare</strong> - currently available
                  via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-childcare-and-early-years#30-hours-free-childcare">
                    Statistics at DfE
                  </a>
                </li>
                <li>
                  <strong>Childcare and early years providers survey</strong> -
                  currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-childcare-and-early-years#childcare-and-early-years-providers-survey">
                    Statistics at DfE
                  </a>
                </li>
                <li>
                  <strong>Childcare and early years survey of parents</strong> -
                  currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-childcare-and-early-years#childcare-and-early-years-survey-of-parents">
                    Statistics at DfE
                  </a>
                </li>
                <li>
                  <strong>
                    Education provision: children under 5 years of age
                  </strong>{' '}
                  - currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-childcare-and-early-years#childcare-and-early-years-survey-of-parents">
                    Statistics at DfE
                  </a>
                </li>
              </ul>
            </div>
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Early years foundation stage profile
            </h4>
            <div className="govuk-inset-text">
              Currently available via{' '}
              <a href="https://www.gov.uk/government/collections/statistics-early-years-foundation-stage-profile#results-at-national-and-local-authority-level">
                Statistics at DfE
              </a>
            </div>
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Education and training
            </h4>
            <div className="govuk-inset-text">
              Currently available via{' '}
              <a href="https://www.gov.uk/government/collections/statistics-education-and-training#documents">
                Statistics at DfE
              </a>
            </div>
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Parental responsibility measures
            </h4>
            <div className="govuk-inset-text">
              Currently available via{' '}
              <a href="https://www.gov.uk/government/collections/parental-responsibility-measures#official-statistics">
                Statistics at DfE
              </a>
            </div>
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Special educational needs (SEN)
            </h4>
            <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
              <ul className="govuk-bulllet-list govuk-!-margin-bottom-">
                <li>
                  <strong>Special educational needs in England</strong> -
                  currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-special-educational-needs-sen#national-statistics-on-special-educational-needs-in-england">
                    Statistics at DfE
                  </a>
                </li>
                <li>
                  <strong>
                    Special educational needs: analysis and summary of data
                    sources
                  </strong>{' '}
                  - currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-special-educational-needs-sen#analysis-of-children-with-special-educational-needs">
                    Statistics at DfE
                  </a>
                </li>
                <li>
                  <strong>Statements on SEN and EHC plans</strong> - currently
                  available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-special-educational-needs-sen#statements-of-special-educational-needs-(sen)-and-education,-health-and-care-(ehc)-plans">
                    Statistics at DfE
                  </a>
                </li>
              </ul>
            </div>
          </div>
        </AccordionSection>
        <AccordionSection heading="Finance" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Education and training
            </h4>
            <div className="govuk-inset-text">
              Currently available via{' '}
              <a href="https://www.gov.uk/government/collections/statistics-education-and-training#documents">
                Statistics at DfE
              </a>
            </div>
          </div>
        </AccordionSection>
        <AccordionSection heading="Institutions" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Childcare and early years
            </h4>
            <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
              <ul className="govuk-bulllet-list govuk-!-margin-bottom-">
                <li>
                  <strong>30 hours free childcare</strong> - currently available
                  via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-childcare-and-early-years#30-hours-free-childcare">
                    Statistics at DfE
                  </a>
                </li>
                <li>
                  <strong>Childcare and early years providers survey</strong> -
                  currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-childcare-and-early-years#childcare-and-early-years-providers-survey">
                    Statistics at DfE
                  </a>
                </li>
                <li>
                  <strong>Childcare and early years survey of parents</strong> -
                  currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-childcare-and-early-years#childcare-and-early-years-survey-of-parents">
                    Statistics at DfE
                  </a>
                </li>
                <li>
                  <strong>
                    Education provision: children under 5 years of age
                  </strong>{' '}
                  - currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-childcare-and-early-years#childcare-and-early-years-survey-of-parents">
                    Statistics at DfE
                  </a>
                </li>
              </ul>
            </div>
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Education and training
            </h4>
            <div className="govuk-inset-text">
              Currently available via{' '}
              <a href="https://www.gov.uk/government/collections/statistics-education-and-training#documents">
                Statistics at DfE
              </a>
            </div>
          </div>
        </AccordionSection>
        <AccordionSection
          heading="Participation and characteristics"
          caption=""
        >
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Childcare and early years
            </h4>
            <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
              <ul className="govuk-bulllet-list govuk-!-margin-bottom-">
                <li>
                  <strong>30 hours free childcare</strong> - currently available
                  via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-childcare-and-early-years#30-hours-free-childcare">
                    Statistics at DfE
                  </a>
                </li>
                <li>
                  <strong>Childcare and early years providers survey</strong> -
                  currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-childcare-and-early-years#childcare-and-early-years-providers-survey">
                    Statistics at DfE
                  </a>
                </li>
                <li>
                  <strong>Childcare and early years survey of parents</strong> -
                  currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-childcare-and-early-years#childcare-and-early-years-survey-of-parents">
                    Statistics at DfE
                  </a>
                </li>
                <li>
                  <strong>
                    Education provision: children under 5 years of age
                  </strong>{' '}
                  - currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-childcare-and-early-years#childcare-and-early-years-survey-of-parents">
                    Statistics at DfE
                  </a>
                </li>
              </ul>
            </div>
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Education and training
            </h4>
            <div className="govuk-inset-text">
              Currently available via{' '}
              <a href="https://www.gov.uk/government/collections/statistics-education-and-training#documents">
                Statistics at DfE
              </a>
            </div>
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Parental responsibility measures
            </h4>
            <div className="govuk-inset-text">
              Currently available via{' '}
              <a href="https://www.gov.uk/government/collections/parental-responsibility-measures#official-statistics">
                Statistics at DfE
              </a>
            </div>
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Pupil projections
            </h4>
            <div className="govuk-inset-text">
              Currently available via{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                Statistics at DfE
              </a>
            </div>
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Special educational needs (SEN)
            </h4>
            <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
              <ul className="govuk-bulllet-list govuk-!-margin-bottom-">
                <li>
                  <strong>Special educational needs in England</strong> -
                  currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-special-educational-needs-sen#national-statistics-on-special-educational-needs-in-england">
                    Statistics at DfE
                  </a>
                </li>
                <li>
                  <strong>
                    Special educational needs: analysis and summary of data
                    sources
                  </strong>{' '}
                  - currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-special-educational-needs-sen#analysis-of-children-with-special-educational-needs">
                    Statistics at DfE
                  </a>
                </li>
                <li>
                  <strong>Statements on SEN and EHC plans</strong> - currently
                  available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-special-educational-needs-sen#statements-of-special-educational-needs-(sen)-and-education,-health-and-care-(ehc)-plans">
                    Statistics at DfE
                  </a>
                </li>
              </ul>
            </div>
          </div>
        </AccordionSection>
      </Accordion>
      <h2 className="govuk-heading-l govuk-!-margin-top-9">
        Further education
      </h2>
      <Accordion id="further-education">
        <AccordionSection heading="Attainment and outcomes" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              16 to 19 attainment
            </h4>
            <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
              <ul className="govuk-bulllet-list govuk-!-margin-bottom-">
                <li>
                  <strong>
                    16 to 18 school and college performance tables
                  </strong>{' '}
                  - currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-attainment-at-19-years#16-to-18-school-and-college-performance-tables">
                    Statistics at DfE
                  </a>
                </li>
                <li>
                  <strong>A levels and other 16 to 18 results</strong> -
                  currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-attainment-at-19-years#a-levels-and-other-16-to-18-results">
                    Statistics at DfE
                  </a>
                </li>
                <li>
                  <strong>
                    Level 2 and 3 attainment by young people aged 19
                  </strong>{' '}
                  - currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-attainment-at-19-years#level-2-and-3-attainment">
                    Statistics at DfE
                  </a>
                </li>
              </ul>
            </div>
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Education and training
            </h4>
            <div className="govuk-inset-text">
              Currently available via{' '}
              <a href="https://www.gov.uk/government/collections/statistics-education-and-training#documents">
                Statistics at DfE
              </a>
            </div>
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Further education and skills
            </h4>
            <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
              <ul className="govuk-bulllet-list govuk-!-margin-bottom-">
                <li>
                  <strong>Apprenticeship and levy statistics</strong> -
                  currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/further-education-and-skills-statistical-first-release-sfr#apprenticeships-and-levy---older-data">
                    Statistics at DfE
                  </a>
                </li>
                <li>
                  <strong>Apprenticeships and traineeships</strong> - currently
                  available via{' '}
                  <a href="https://www.gov.uk/government/collections/further-education-and-skills-statistical-first-release-sfr#apprenticeships-and-traineeships---older-data">
                    Statistics at DfE
                  </a>
                </li>
                <li>
                  <strong>Further education and skills</strong> - currently
                  available via{' '}
                  <a href="https://www.gov.uk/government/collections/further-education-and-skills-statistical-first-release-sfr#fe-and-skills---older-data">
                    Statistics at DfE
                  </a>
                </li>
              </ul>
            </div>
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Further education for benefits claimants
            </h4>
            <div className="govuk-inset-text">
              Currently available via{' '}
              <a href="https://www.gov.uk/government/collections/further-education-for-benefit-claimants#documents">
                Statistics at DfE
              </a>
            </div>
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              National achievement rates tables
            </h4>
            <div className="govuk-inset-text">
              Currently available via{' '}
              <a href="https://www.gov.uk/government/collections/sfa-national-success-rates-tables#national-achievement-rates-tables">
                Statistics at DfE
              </a>
            </div>
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Outcome based success measures
            </h4>
            <div className="govuk-inset-text">
              Currently available via{' '}
              <a href="https://www.gov.uk/government/collections/statistics-outcome-based-success-measures#statistics">
                Statistics at DfE
              </a>
            </div>
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Performance tables
            </h4>
            <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
              <ul className="govuk-bulllet-list govuk-!-margin-bottom-">
                <li>
                  <strong>Primary school performance tables</strong> - currently
                  available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-performance-tables#primary-school-(key-stage-2)">
                    Statistics at DfE
                  </a>
                </li>
                <li>
                  <strong>School and college performance tables</strong> -
                  currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-performance-tables#school-and-college:-post-16-(key-stage-5)">
                    Statistics at DfE
                  </a>
                </li>
                <li>
                  <strong>Secondary school performance tables</strong> -
                  currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-performance-tables#secondary-school-(key-stage-4)">
                    Statistics at DfE
                  </a>
                </li>
              </ul>
            </div>
          </div>
        </AccordionSection>
        <AccordionSection heading="Finance" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Advance learner loans
            </h4>
            <div className="govuk-inset-text">
              Currently available via{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                Statistics at DfE
              </a>
            </div>
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Education and training
            </h4>
            <div className="govuk-inset-text">
              Currently available via{' '}
              <a href="https://www.gov.uk/government/collections/statistics-education-and-training#documents">
                Statistics at DfE
              </a>
            </div>
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Further education and skills
            </h4>
            <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
              <ul className="govuk-bulllet-list govuk-!-margin-bottom-">
                <li>
                  <strong>Apprenticeship and levy statistics</strong> -
                  currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/further-education-and-skills-statistical-first-release-sfr#apprenticeships-and-levy---older-data">
                    Statistics at DfE
                  </a>
                </li>
                <li>
                  <strong>Apprenticeships and traineeships</strong> - currently
                  available via{' '}
                  <a href="https://www.gov.uk/government/collections/further-education-and-skills-statistical-first-release-sfr#apprenticeships-and-traineeships---older-data">
                    Statistics at DfE
                  </a>
                </li>
                <li>
                  <strong>Further education and skills</strong> - currently
                  available via{' '}
                  <a href="https://www.gov.uk/government/collections/further-education-and-skills-statistical-first-release-sfr#fe-and-skills---older-data">
                    Statistics at DfE
                  </a>
                </li>
              </ul>
            </div>
          </div>
        </AccordionSection>
        <AccordionSection heading="Institutions" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Education and training
            </h4>
            <div className="govuk-inset-text">
              Currently available via{' '}
              <a href="https://www.gov.uk/government/collections/statistics-education-and-training#documents">
                Statistics at DfE
              </a>
            </div>
          </div>
        </AccordionSection>
        <AccordionSection
          heading="Participation and characteristics"
          caption=""
        >
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Education and training
            </h4>
            <div className="govuk-inset-text">
              Currently available via{' '}
              <a href="https://www.gov.uk/government/collections/statistics-education-and-training#documents">
                Statistics at DfE
              </a>
            </div>
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              FE Choices
            </h4>
            <div className="govuk-inset-text">
              Currently available via{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                Statistics at DfE
              </a>
            </div>
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Further education and skills
            </h4>
            <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
              <ul className="govuk-bulllet-list govuk-!-margin-bottom-">
                <li>
                  <strong>Apprenticeship and levy statistics</strong> -
                  currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/further-education-and-skills-statistical-first-release-sfr#apprenticeships-and-levy---older-data">
                    Statistics at DfE
                  </a>
                </li>
                <li>
                  <strong>Apprenticeships and traineeships</strong> - currently
                  available via{' '}
                  <a href="https://www.gov.uk/government/collections/further-education-and-skills-statistical-first-release-sfr#apprenticeships-and-traineeships---older-data">
                    Statistics at DfE
                  </a>
                </li>
                <li>
                  <strong>Further education and skills</strong> - currently
                  available via{' '}
                  <a href="https://www.gov.uk/government/collections/further-education-and-skills-statistical-first-release-sfr#fe-and-skills---older-data">
                    Statistics at DfE
                  </a>
                </li>
              </ul>
            </div>
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Further education for benefits claimants
            </h4>
            <div className="govuk-inset-text">
              Currently available via{' '}
              <a href="https://www.gov.uk/government/collections/further-education-for-benefit-claimants#documents">
                Statistics at DfE
              </a>
            </div>
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Not in education, employment or training (NEET) and participation
            </h4>
            <div className="govuk-inset-text">
              Currently available via{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                Statistics at DfE
              </a>
            </div>
          </div>
        </AccordionSection>
      </Accordion>
      <h2 className="govuk-heading-l govuk-!-margin-top-9">Higher education</h2>
      <Accordion id="higher-education">
        <AccordionSection heading="Attainment and outcomes" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Education and training
            </h4>
            <div className="govuk-inset-text">
              Currently available via{' '}
              <a href="https://www.gov.uk/government/collections/statistics-education-and-training#documents">
                Statistics at DfE
              </a>
            </div>
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Graduate labour market
            </h4>
            <div className="govuk-inset-text">
              Currently available via{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                Statistics at DfE
              </a>
            </div>
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Higher education graduate employment and earnings
            </h4>
            <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
              <div className="govuk-inset-text">
                <strong>Graduate outcomes (LEO)</strong> - currently available
                via{' '}
                <a href="https://www.gov.uk/government/collections/statistics-higher-education-graduate-employment-and-earnings#documents">
                  Statistics at DfE
                </a>
              </div>
            </div>
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Higher education statistics
            </h4>
            <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
              <ul className="govuk-bulllet-list govuk-!-margin-bottom-">
                <li>
                  <strong>Higher education: destinations of leavers</strong> -
                  currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/official-statistics-releases#destinations-of-higher-education-leavers">
                    Statistics at DfE
                  </a>
                </li>
                <li>
                  <strong>
                    Higher education enrolments and qualifications
                  </strong>{' '}
                  - currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/official-statistics-releases#higher-education-enrolments-and-qualifications">
                    Statistics at DfE
                  </a>
                </li>
                <li>
                  <strong>Performance indicators in higher education</strong> -
                  currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/official-statistics-releases#performance-indicators">
                    Statistics at DfE
                  </a>
                </li>
                <li>
                  <strong>Staff at higher education providers in the UK</strong>{' '}
                  - currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/official-statistics-releases#staff-at-higher-education">
                    Statistics at DfE
                  </a>
                </li>
              </ul>
            </div>
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Initial teacher training (ITT)
            </h4>
            <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
              <ul className="govuk-bulllet-list govuk-!-margin-bottom-">
                <li>
                  <strong>Initial teacher training performance profiles</strong>{' '}
                  - currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-teacher-training#performance-data">
                    Statistics at DfE
                  </a>
                </li>
                <li>
                  <strong>
                    Initial teacher training: trainee number census
                  </strong>{' '}
                  - currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-teacher-training#census-data">
                    Statistics at DfE
                  </a>
                </li>
                <li>
                  <strong>TSM and initial teacher training allocations</strong>{' '}
                  - currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-teacher-training#teacher-supply-model-and-itt-allocations">
                    Statistics at DfE
                  </a>
                </li>
              </ul>
            </div>
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Performance tables
            </h4>
            <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
              <ul className="govuk-bulllet-list govuk-!-margin-bottom-">
                <li>
                  <strong>Primary school performance tables</strong> - currently
                  available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-performance-tables#primary-school-(key-stage-2)">
                    Statistics at DfE
                  </a>
                </li>
                <li>
                  <strong>School and college performance tables</strong> -
                  currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-performance-tables#school-and-college:-post-16-(key-stage-5)">
                    Statistics at DfE
                  </a>
                </li>
                <li>
                  <strong>Secondary school performance tables</strong> -
                  currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-performance-tables#secondary-school-(key-stage-4)">
                    Statistics at DfE
                  </a>
                </li>
              </ul>
            </div>
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Widening participation in higher education
            </h4>
            <div className="govuk-inset-text">
              Currently available via{' '}
              <a href="https://www.gov.uk/government/collections/widening-participation-in-higher-education#documents">
                Statistics at DfE
              </a>
            </div>
          </div>
        </AccordionSection>
        <AccordionSection heading="Finance" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Education and training
            </h4>
            <div className="govuk-inset-text">
              Currently available via{' '}
              <a href="https://www.gov.uk/government/collections/statistics-education-and-training#documents">
                Statistics at DfE
              </a>
            </div>
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Higher education graduate employment and earnings
            </h4>
            <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
              <div className="govuk-inset-text">
                <strong>Graduate outcomes (LEO)</strong> - currently available
                via{' '}
                <a href="https://www.gov.uk/government/collections/statistics-higher-education-graduate-employment-and-earnings#documents">
                  Statistics at DfE
                </a>
              </div>
            </div>
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Student loan forecasts
            </h4>
            <div className="govuk-inset-text">
              Currently available via{' '}
              <a href="https://www.gov.uk/government/collections/statistics-student-loan-forecasts#documents">
                Statistics at DfE
              </a>
            </div>
          </div>
        </AccordionSection>
        <AccordionSection heading="Institutions" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Education and training
            </h4>
            <div className="govuk-inset-text">
              Currently available via{' '}
              <a href="https://www.gov.uk/government/collections/statistics-education-and-training#documents">
                Statistics at DfE
              </a>
            </div>
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Higher education statistics
            </h4>
            <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
              <ul className="govuk-bulllet-list govuk-!-margin-bottom-">
                <li>
                  <strong>Higher education: destinations of leavers</strong> -
                  currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/official-statistics-releases#destinations-of-higher-education-leavers">
                    Statistics at DfE
                  </a>
                </li>
                <li>
                  <strong>
                    Higher education enrolments and qualifications
                  </strong>{' '}
                  - currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/official-statistics-releases#higher-education-enrolments-and-qualifications">
                    Statistics at DfE
                  </a>
                </li>
                <li>
                  <strong>Performance indicators in higher education</strong> -
                  currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/official-statistics-releases#performance-indicators">
                    Statistics at DfE
                  </a>
                </li>
                <li>
                  <strong>Staff at higher education providers in the UK</strong>{' '}
                  - currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/official-statistics-releases#staff-at-higher-education">
                    Statistics at DfE
                  </a>
                </li>
              </ul>
            </div>
          </div>
        </AccordionSection>
        <AccordionSection
          heading="Participation and characteristics"
          caption=""
        >
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Education and training
            </h4>
            <div className="govuk-inset-text">
              Currently available via{' '}
              <a href="https://www.gov.uk/government/collections/statistics-education-and-training#documents">
                Statistics at DfE
              </a>
            </div>
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Higher education graduate employment and earnings
            </h4>
            <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
              <div className="govuk-inset-text">
                <strong>Graduate outcomes (LEO)</strong> - currently available
                via{' '}
                <a href="https://www.gov.uk/government/collections/statistics-higher-education-graduate-employment-and-earnings#documents">
                  Statistics at DfE
                </a>
              </div>
            </div>
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Higher education
            </h4>
            <div className="govuk-inset-text">
              Currently available via{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                Statistics at DfE
              </a>
            </div>
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Initial teacher training (ITT)
            </h4>
            <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
              <ul className="govuk-bulllet-list govuk-!-margin-bottom-">
                <li>
                  <strong>Initial teacher training performance profiles</strong>{' '}
                  - currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-teacher-training#performance-data">
                    Statistics at DfE
                  </a>
                </li>
                <li>
                  <strong>
                    Initial teacher training: trainee number census
                  </strong>{' '}
                  - currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-teacher-training#census-data">
                    Statistics at DfE
                  </a>
                </li>
                <li>
                  <strong>TSM and initial teacher training allocations</strong>{' '}
                  - currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-teacher-training#teacher-supply-model-and-itt-allocations">
                    Statistics at DfE
                  </a>
                </li>
              </ul>
            </div>
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Not in education, employment or training (NEET) and participation
            </h4>
            <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
              <ul className="govuk-bulllet-list govuk-!-margin-bottom-">
                <li>
                  <strong>
                    Participation in education, training and employment
                  </strong>{' '}
                  - currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-neet#participation-in-education,-employment-or-training">
                    Statistics at DfE
                  </a>
                </li>
                <li>
                  <strong>NEET statistics quarterly brief</strong> - currently
                  available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-neet#neet:-2016-to-2017-data-">
                    Statistics at DfE
                  </a>
                </li>
              </ul>
            </div>
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Participation rates in higher education
            </h4>
            <div className="govuk-inset-text">
              Currently available via{' '}
              <a href="https://www.gov.uk/government/collections/statistics-on-higher-education-initial-participation-rates#participation-rates-in-higher-education-for-england">
                Statistics at DfE
              </a>
            </div>
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Widening participation in higher education
            </h4>
            <div className="govuk-inset-text">
              Currently available via{' '}
              <a href="https://www.gov.uk/government/collections/widening-participation-in-higher-education#documents">
                Statistics at DfE
              </a>
            </div>
          </div>
        </AccordionSection>
        <AccordionSection heading="Workforce" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Higher education statistics
            </h4>
            <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
              <ul className="govuk-bulllet-list govuk-!-margin-bottom-">
                <li>
                  <strong>Higher education: destinations of leavers</strong> -
                  currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/official-statistics-releases#destinations-of-higher-education-leavers">
                    Statistics at DfE
                  </a>
                </li>
                <li>
                  <strong>
                    Higher education enrolments and qualifications
                  </strong>{' '}
                  - currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/official-statistics-releases#higher-education-enrolments-and-qualifications">
                    Statistics at DfE
                  </a>
                </li>
                <li>
                  <strong>Performance indicators in higher education</strong> -
                  currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/official-statistics-releases#performance-indicators">
                    Statistics at DfE
                  </a>
                </li>
                <li>
                  <strong>Staff at higher education providers in the UK</strong>{' '}
                  - currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/official-statistics-releases#staff-at-higher-education">
                    Statistics at DfE
                  </a>
                </li>
              </ul>
            </div>
          </div>
        </AccordionSection>
      </Accordion>
      <h2 className="govuk-heading-l govuk-!-margin-top-9">Schools</h2>
      <Accordion id="schools">
        <AccordionSection heading="Attainment and outcomes" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              16 to 19 attainment
            </h4>
            <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
              <ul className="govuk-bulllet-list govuk-!-margin-bottom-">
                <li>
                  <strong>
                    16 to 18 school and college performance tables
                  </strong>{' '}
                  - currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-attainment-at-19-years#16-to-18-school-and-college-performance-tables">
                    Statistics at DfE
                  </a>
                </li>
                <li>
                  <strong>A levels and other 16 to 18 results</strong> -
                  currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-attainment-at-19-years#a-levels-and-other-16-to-18-results">
                    Statistics at DfE
                  </a>
                </li>
                <li>
                  <strong>
                    Level 2 and 3 attainment by young people aged 19
                  </strong>{' '}
                  - currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-attainment-at-19-years#level-2-and-3-attainment">
                    Statistics at DfE
                  </a>
                </li>
              </ul>
            </div>
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Destinations of key stage 4 and key stage 5 pupils
            </h4>
            <div className="govuk-inset-text">
              Currently available via{' '}
              <a href="https://www.gov.uk/government/collections/statistics-destinations#destinations-after-key-stage-4-and-5">
                Statistics at DfE
              </a>
            </div>
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Education and training
            </h4>
            <div className="govuk-inset-text">
              Currently available via{' '}
              <a href="https://www.gov.uk/government/collections/statistics-education-and-training#documents">
                Statistics at DfE
              </a>
            </div>
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              GCSEs (key stage 4) and equivalent results
            </h4>
            <div className="govuk-inset-text">
              <PrototypeDownloadDropdown link="/prototypes/publication-gcse" />
            </div>
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Key stage 1
            </h4>
            <div className="govuk-inset-text">
              <strong>
                Phonics screening check and key stage 1 assessments
              </strong>{' '}
              - currently available via{' '}
              <a href="https://www.gov.uk/government/collections/statistics-key-stage-1#phonics-screening-check-and-key-stage-1-assessment">
                Statistics at DfE
              </a>
            </div>
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Key stage 2
            </h4>
            <div className="govuk-inset-text">
              Currently available via{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                Statistics at DfE
              </a>
            </div>
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Parental responsibility measures
            </h4>
            <div className="govuk-inset-text">
              Currently available via{' '}
              <a href="https://www.gov.uk/government/collections/parental-responsibility-measures#official-statistics">
                Statistics at DfE
              </a>
            </div>
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Performance tables
            </h4>
            <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
              <ul className="govuk-bulllet-list govuk-!-margin-bottom-">
                <li>
                  <strong>Primary school performance tables</strong> - currently
                  available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-performance-tables#primary-school-(key-stage-2)">
                    Statistics at DfE
                  </a>
                </li>
                <li>
                  <strong>School and college performance tables</strong> -
                  currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-performance-tables#school-and-college:-post-16-(key-stage-5)">
                    Statistics at DfE
                  </a>
                </li>
                <li>
                  <strong>Secondary school performance tables</strong> -
                  currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-performance-tables#secondary-school-(key-stage-4)">
                    Statistics at DfE
                  </a>
                </li>
              </ul>
            </div>
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Special educational needs (SEN)
            </h4>
            <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
              <ul className="govuk-bulllet-list govuk-!-margin-bottom-">
                <li>
                  <strong>Special educational needs in England</strong> -
                  currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-special-educational-needs-sen#national-statistics-on-special-educational-needs-in-england">
                    Statistics at DfE
                  </a>
                </li>
                <li>
                  <strong>
                    Special educational needs: analysis and summary of data
                    sources
                  </strong>{' '}
                  - currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-special-educational-needs-sen#analysis-of-children-with-special-educational-needs">
                    Statistics at DfE
                  </a>
                </li>
                <li>
                  <strong>Statements on SEN and EHC plans</strong> - currently
                  available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-special-educational-needs-sen#statements-of-special-educational-needs-(sen)-and-education,-health-and-care-(ehc)-plans">
                    Statistics at DfE
                  </a>
                </li>
              </ul>
            </div>
          </div>
        </AccordionSection>
        <AccordionSection heading="Finance" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Education and training
            </h4>
            <div className="govuk-inset-text">
              Currently available via{' '}
              <a href="https://www.gov.uk/government/collections/statistics-education-and-training#documents">
                Statistics at DfE
              </a>
            </div>
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Local authority and school finance
            </h4>
            <div className="govuk-inset-text">
              Currently available via{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                Statistics at DfE
              </a>
            </div>
          </div>
        </AccordionSection>

        <AccordionSection heading="Institutions" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Admission appeals
            </h4>
            <div className="govuk-inset-text">
              Currently available via{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                Statistics at DfE
              </a>
            </div>
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Education and training
            </h4>
            <div className="govuk-inset-text">
              Currently available via{' '}
              <a href="https://www.gov.uk/government/collections/statistics-education-and-training#documents">
                Statistics at DfE
              </a>
            </div>
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              GCSEs (key stage 4) and equivalent results
            </h4>
            <p className="govuk-body">
              View statistics, create charts and tables and download data files
              for GCSE and equivalent results in England
            </p>
            <div className="govuk-!-margin-top-0">
              <PrototypeDownloadDropdown link="/prototypes/publication-gcse" />
            </div>
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              School applications
            </h4>
            <div className="govuk-inset-text">
              Currently available via{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                Statistics at DfE
              </a>
            </div>
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              School capacity
            </h4>
            <div className="govuk-inset-text">
              Currently available via{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                Statistics at DfE
              </a>
            </div>
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              School and pupil numbers
            </h4>
            <div className="govuk-inset-text">
              Currently available via{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                Statistics at DfE
              </a>
            </div>
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Workforce statistics and analysis
            </h4>
            <div className="govuk-inset-text">
              Currently available via{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                Statistics at DfE
              </a>
            </div>
          </div>
        </AccordionSection>

        <AccordionSection
          heading="Participation and characteristics"
          caption=""
        >
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              16 to 19 attainment
            </h4>
            <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
              <ul className="govuk-bulllet-list govuk-!-margin-bottom-">
                <li>
                  <strong>
                    16 to 18 school and college performance tables
                  </strong>{' '}
                  - currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-attainment-at-19-years#16-to-18-school-and-college-performance-tables">
                    Statistics at DfE
                  </a>
                </li>
                <li>
                  <strong>A levels and other 16 to 18 results</strong> -
                  currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-attainment-at-19-years#a-levels-and-other-16-to-18-results">
                    Statistics at DfE
                  </a>
                </li>
                <li>
                  <strong>
                    Level 2 and 3 attainment by young people aged 19
                  </strong>{' '}
                  - currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-attainment-at-19-years#level-2-and-3-attainment">
                    Statistics at DfE
                  </a>
                </li>
              </ul>
            </div>
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Admission appeals
            </h4>
            <div className="govuk-inset-text">
              Currently available via{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                Statistics at DfE
              </a>
            </div>
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Destinations of key stage 4 and key stage 5 pupils
            </h4>
            <div className="govuk-inset-text">
              Currently available via{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                Statistics at DfE
              </a>
            </div>
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Exclusions
            </h4>
            <div className="govuk-inset-text">
              <PrototypeDownloadDropdown link="/prototypes/publication-exclusions" />
            </div>
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Education and training
            </h4>
            <div className="govuk-inset-text">
              Currently available via{' '}
              <a href="https://www.gov.uk/government/collections/statistics-education-and-training#documents">
                Statistics at DfE
              </a>
            </div>
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Not in education, employment or training (NEET) and participation
            </h4>
            <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
              <ul className="govuk-bulllet-list govuk-!-margin-bottom-9">
                <li>
                  <strong>
                    Participation in education, training and employment
                  </strong>{' '}
                  - currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-neet#participation-in-education,-employment-or-training">
                    Statistics at DfE
                  </a>
                </li>
                <li>
                  <strong>NEET statistics quarterly brief</strong> - currently
                  available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-neet#neet:-2016-to-2017-data-">
                    Statistics at DfE
                  </a>
                </li>
              </ul>
            </div>
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Parental responsibility measures
            </h4>
            <div className="govuk-inset-text">
              Currently available via{' '}
              <a href="https://www.gov.uk/government/collections/parental-responsibility-measures#official-statistics">
                Statistics at DfE
              </a>
            </div>
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Pupil absence
            </h4>
            <div className="govuk-inset-text">
              <PrototypeDownloadDropdown />
            </div>
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Pupil projections
            </h4>
            <div className="govuk-inset-text">
              Currently available via{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                Statistics at DfE
              </a>
            </div>
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              School and pupil numbers
            </h4>
            <div className="govuk-inset-text">
              Currently available via{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                Statistics at DfE
              </a>
            </div>
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              School applications
            </h4>
            <div className="govuk-inset-text">
              Currently available via{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                Statistics at DfE
              </a>
            </div>
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Special educational needs (SEN)
            </h4>
            <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
              <ul className="govuk-bulllet-list govuk-!-margin-bottom-">
                <li>
                  <strong>Special educational needs in England</strong> -
                  currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-special-educational-needs-sen#national-statistics-on-special-educational-needs-in-england">
                    Statistics at DfE
                  </a>
                </li>
                <li>
                  <strong>
                    Special educational needs: analysis and summary of data
                    sources
                  </strong>{' '}
                  - currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-special-educational-needs-sen#analysis-of-children-with-special-educational-needs">
                    Statistics at DfE
                  </a>
                </li>
                <li>
                  <strong>Statements on SEN and EHC plans</strong> - currently
                  available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-special-educational-needs-sen#statements-of-special-educational-needs-(sen)-and-education,-health-and-care-(ehc)-plans">
                    Statistics at DfE
                  </a>
                </li>
              </ul>
            </div>
          </div>
        </AccordionSection>

        <AccordionSection heading="Workforce" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Initial teacher training (ITT)
            </h4>
            <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
              <ul className="govuk-bulllet-list govuk-!-margin-bottom-">
                <li>
                  <strong>Initial teacher training performance profiles</strong>{' '}
                  - currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-teacher-training#performance-data">
                    Statistics at DfE
                  </a>
                </li>
                <li>
                  <strong>
                    Initial teacher training: trainee number census
                  </strong>{' '}
                  - currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-teacher-training#census-data">
                    Statistics at DfE
                  </a>
                </li>
                <li>
                  <strong>TSM and initial teacher training allocations</strong>{' '}
                  - currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-teacher-training#teacher-supply-model-and-itt-allocations">
                    Statistics at DfE
                  </a>
                </li>
              </ul>
            </div>
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              School workforce
            </h4>
            <div className="govuk-inset-text">
              Currently available via{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                Statistics at DfE
              </a>
            </div>
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Workforce statistics and analysis
            </h4>
            <div className="govuk-inset-text">
              Currently available via{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                Statistics at DfE
              </a>
            </div>
          </div>
        </AccordionSection>
      </Accordion>
      <h2 className="govuk-heading-l govuk-!-margin-top-9">Social care</h2>
      <Accordion id="social">
        <AccordionSection heading="Children’s social care" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Children in need and child protection
            </h4>
            <div className="govuk-inset-text">
              Currently available via{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                Statistics at DfE
              </a>
            </div>
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Looked-after children
            </h4>
            <div className="govuk-inset-text">
              Currently available via{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                Statistics at DfE
              </a>
            </div>
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Secure children’s homes
            </h4>
            <div className="govuk-inset-text">
              Currently available via{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                Statistics at DfE
              </a>
            </div>
          </div>
        </AccordionSection>
        <AccordionSection heading="Institutions" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Secure children’s homes
            </h4>
            <div className="govuk-inset-text">
              Currently available via{' '}
              <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
                Statistics at DfE
              </a>
            </div>
          </div>
        </AccordionSection>
        <AccordionSection heading="Workforce" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
              Children's social work workforce
            </h4>
            <div className="govuk-inset-text">
              Currently available via{' '}
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
