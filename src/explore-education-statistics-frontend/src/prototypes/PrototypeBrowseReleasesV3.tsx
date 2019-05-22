import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import Details from '@common/components/Details';
import React from 'react';
import Link from '../components/Link';
import PrototypeDownloadDropdown from './components/PrototypeDownloadDropdown';
import PrototypePage from './components/PrototypePage';

const BrowseReleasesPage = () => {
  return (
    <PrototypePage
      breadcrumbs={[{ text: 'Find statistics and download data' }]}
    >
      <h1 className="govuk-heading-xl">Find statistics and data</h1>
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <p className="govuk-body">
            Browse to find the statistics and data you’re looking for and open
            the section to get links to:
          </p>
          <ul className="govuk-bulllet-list govuk-!-margin-bottom-6">
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
      <hr />
      <div className="dfe-contents-details">
        <h2 className="govuk-heading-m">Contents</h2>
        <Details summary="Children and early years - including social care">
          <h2 className="govuk-heading-l govuk-visually-hidden" id="section-1">
            Children and early years - including social care
          </h2>
          <Accordion id="children-and-early-years">
            <AccordionSection heading="Childcare and early years" caption="">
              <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
                <ul className="govuk-bullet-list govuk-!-margin-top-0">
                  <li>
                    <strong>30 hours free childcare</strong> - currently
                    available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-childcare-and-early-years#30-hours-free-childcare">
                      Statistics at DfE
                    </a>
                  </li>
                  <li>
                    <strong>Childcare and early years providers survey</strong>{' '}
                    - currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-childcare-and-early-years#childcare-and-early-years-providers-survey">
                      Statistics at DfE
                    </a>
                  </li>
                  <li>
                    <strong>Childcare and early years survey of parents</strong>{' '}
                    - currently available via{' '}
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
            </AccordionSection>
            <AccordionSection
              heading="Children in need and child protection"
              caption=""
            >
              <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
                <ul className="govuk-bullet-list govuk-!-margin-top-0">
                  <li>
                    <strong>Characteristics of children in need</strong> -
                    currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-childcare-and-early-years#30-hours-free-childcare">
                      Statistics at DfE
                    </a>
                  </li>
                </ul>
              </div>
            </AccordionSection>
            <AccordionSection
              heading="Children's social work workforce"
              caption=""
            >
              <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
                <ul className="govuk-bulllet-list govuk-!-margin-bottom-9">
                  <li>
                    <strong>Children's social work workforce</strong> -
                    currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-childrens-social-care-workforce#statutory-collection">
                      Statistics at DfE
                    </a>
                  </li>
                </ul>
              </div>
            </AccordionSection>
            <AccordionSection
              heading="Early years foundation stage profile"
              caption=""
            >
              <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
                <ul className="govuk-bulllet-list govuk-!-margin-bottom-9">
                  <li>
                    <strong>
                      Early years foundation stage profile results
                    </strong>{' '}
                    - currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-early-years-foundation-stage-profile#results-at-national-and-local-authority-level">
                      Statistics at DfE
                    </a>
                  </li>
                </ul>
              </div>
            </AccordionSection>
            <AccordionSection heading="Looked-after children" caption="">
              <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
                <ul className="govuk-bulllet-list govuk-!-margin-bottom-9">
                  <li>
                    <strong>
                      Children looked after in England including adoptions
                    </strong>{' '}
                    - currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-looked-after-children#looked-after-children">
                      Statistics at DfE
                    </a>
                  </li>
                  <li>
                    <strong>Outcomes for children looked after by LAs</strong> -
                    currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-looked-after-children#outcomes-for-looked-after-children">
                      Statistics at DfE
                    </a>
                  </li>
                </ul>
              </div>
            </AccordionSection>
            <AccordionSection heading="Secure children's homes" caption="">
              <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
                <ul className="govuk-bulllet-list govuk-!-margin-bottom-9">
                  <li>
                    <strong>
                      Children accommodated in secure children's homes
                    </strong>{' '}
                    - currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-secure-children-s-homes">
                      Statistics at DfE
                    </a>
                  </li>
                </ul>
              </div>
            </AccordionSection>
          </Accordion>
        </Details>
        <Details summary="Destinations of pupils and students - including NEET">
          <h2 className="govuk-heading-l govuk-visually-hidden" id="section-2">
            Destinations of pupils and students - including NEET
          </h2>
          <Accordion id="destinations-pupils-and-students">
            <AccordionSection
              heading="Destinations of key stage 4 and key stage 5 pupils	"
              caption=""
            >
              <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
                <ul className="govuk-bulllet-list govuk-!-margin-bottom-9">
                  <li>
                    <strong>Destinations of KS4 and KS5 pupils</strong> -
                    currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-destinations#destinations-after-key-stage-4-and-5">
                      Statistics at DfE
                    </a>
                  </li>
                </ul>
              </div>
            </AccordionSection>
            <AccordionSection heading="Graduate labour market" caption="">
              <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
                <ul className="govuk-bulllet-list govuk-!-margin-bottom-9">
                  <li>
                    <strong>Graduate labour market statistics</strong> -
                    currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/graduate-labour-market-quarterly-statistics#documents">
                      Statistics at DfE
                    </a>
                  </li>
                </ul>
              </div>
            </AccordionSection>
            <AccordionSection heading="NEET and participation" caption="">
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
            </AccordionSection>
          </Accordion>
        </Details>
        <Details summary="Finance and funding">
          <h2 className="govuk-heading-l govuk-visually-hidden" id="section-3">
            Finance and funding
          </h2>
          <Accordion id="finance-and-funding">
            <AccordionSection
              heading="Local authority and school finance"
              caption=""
            >
              <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
                <ul className="govuk-bulllet-list govuk-!-margin-bottom-9">
                  <li>
                    <strong>
                      Income and expenditure in academies in England
                    </strong>{' '}
                    - currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-local-authority-school-finance-data#academy-spending">
                      Statistics at DfE
                    </a>
                  </li>
                  <li>
                    <strong>LA and school expenditure</strong> - currently
                    available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-local-authority-school-finance-data#local-authority-and-school-finance">
                      Statistics at DfE
                    </a>
                  </li>
                  <li>
                    <strong>Planned LA and school expenditure</strong> -
                    currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-local-authority-school-finance-data#planned-local-authority-and-school-spending-">
                      Statistics at DfE
                    </a>
                  </li>
                </ul>
              </div>
            </AccordionSection>
            <AccordionSection heading="Student loan forecasts	" caption="">
              <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
                <ul className="govuk-bulllet-list govuk-!-margin-bottom-9">
                  <li>
                    <strong>Student loan forecasts for England</strong> -
                    currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-student-loan-forecasts#documents">
                      Statistics at DfE
                    </a>
                  </li>
                </ul>
              </div>
            </AccordionSection>
          </Accordion>
        </Details>
        <Details summary="Further education">
          <h2 className="govuk-heading-l govuk-visually-hidden" id="section-4">
            Further education
          </h2>
          <Accordion id="further-education">
            <AccordionSection heading="Advanced learner loans" caption="">
              <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
                <ul className="govuk-bulllet-list govuk-!-margin-bottom-9">
                  <li>
                    <strong>Advanced learner loans applications</strong> -
                    currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/further-education#advanced-learner-loans-applications-2017-to-2018">
                      Statistics at DfE
                    </a>
                  </li>
                </ul>
              </div>
            </AccordionSection>
            <AccordionSection heading="FE Choices" caption="">
              <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
                <ul className="govuk-bulllet-list govuk-!-margin-bottom-9">
                  <li>
                    <strong>FE choices employer satisfaction survey</strong> -
                    currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/fe-choices#employer-satisfaction-survey-data">
                      Statistics at DfE
                    </a>
                  </li>
                  <li>
                    <strong>FE choices learner satisfaction survey</strong> -
                    currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/fe-choices#learner-satisfaction-survey-data">
                      Statistics at DfE
                    </a>
                  </li>
                </ul>
              </div>
            </AccordionSection>
            <AccordionSection heading="Further education and skills" caption="">
              <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
                <ul className="govuk-bulllet-list govuk-!-margin-bottom-9">
                  <li>
                    <strong>Apprenticeship and levy statistics</strong> -
                    currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/further-education-and-skills-statistical-first-release-sfr#apprenticeships-and-levy---older-data">
                      Statistics at DfE
                    </a>
                  </li>
                  <li>
                    <strong>Apprenticeships and traineeships</strong> -
                    currently available via{' '}
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
            </AccordionSection>
            <AccordionSection
              heading="Further education for benefits claimants"
              caption=""
            >
              <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
                <ul className="govuk-bulllet-list govuk-!-margin-bottom-9">
                  <li>
                    <strong>Further education for benefits claimants</strong> -
                    currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/further-education-for-benefit-claimants#documents">
                      Statistics at DfE
                    </a>
                  </li>
                </ul>
              </div>
            </AccordionSection>

            <AccordionSection
              heading="National achievement rates tables"
              caption=""
            >
              <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
                <ul className="govuk-bulllet-list govuk-!-margin-bottom-9">
                  <li>
                    <strong>National achievement rates tables</strong> -
                    currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/sfa-national-success-rates-tables#national-achievement-rates-tables">
                      Statistics at DfE
                    </a>
                  </li>
                </ul>
              </div>
            </AccordionSection>
          </Accordion>
        </Details>
        <Details summary="Higher education">
          <h2 className="govuk-heading-l govuk-visually-hidden" id="section-5">
            Higher education
          </h2>
          <Accordion id="higher-education">
            <AccordionSection
              heading="Higher education graduate employment and earnings"
              caption=""
            >
              <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
                <ul className="govuk-bullet-list govuk-!-margin-top-0">
                  <li>
                    <strong>Graduate outcomes (LEO)</strong> - currently
                    available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-higher-education-graduate-employment-and-earnings#documents">
                      Statistics at DfE
                    </a>
                  </li>
                </ul>
              </div>
            </AccordionSection>
            <AccordionSection heading="Higher education statistics" caption="">
              <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
                <ul className="govuk-bullet-list govuk-!-margin-top-0">
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
                    <strong>Performance indicators in higher education</strong>{' '}
                    - currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/official-statistics-releases#performance-indicators">
                      Statistics at DfE
                    </a>
                  </li>
                  <li>
                    <strong>
                      Staff at higher education providers in the UK
                    </strong>{' '}
                    - currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/official-statistics-releases#staff-at-higher-education">
                      Statistics at DfE
                    </a>
                  </li>
                </ul>
              </div>
            </AccordionSection>
            <AccordionSection
              heading="Participation rates in higher education"
              caption=""
            >
              <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
                <ul className="govuk-bullet-list govuk-!-margin-top-0">
                  <li>
                    <strong>Participation rates in higher education</strong> -
                    currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-on-higher-education-initial-participation-rates#participation-rates-in-higher-education-for-england">
                      Statistics at DfE
                    </a>
                  </li>
                </ul>
              </div>
            </AccordionSection>
            <AccordionSection
              heading="Widening participation in higher education	"
              caption=""
            >
              <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
                <ul className="govuk-bullet-list govuk-!-margin-top-0">
                  <li>
                    <strong>Widening participation in higher education</strong>{' '}
                    - currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/widening-participation-in-higher-education#documents">
                      Statistics at DfE
                    </a>
                  </li>
                </ul>
              </div>
            </AccordionSection>
          </Accordion>
        </Details>
        <Details summary="Pupils and schools">
          <h2 className="govuk-heading-l govuk-visually-hidden" id="section-6">
            Pupils and schools
          </h2>
          <Accordion id="pupils-and-schools">
            <AccordionSection heading="Admission appeals" caption="">
              <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
                <ul className="govuk-bullet-list govuk-!-margin-top-0">
                  <li>
                    <strong>Admissions appeals in England</strong> - currently
                    available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-admission-appeals#documents">
                      Statistics at DfE
                    </a>
                  </li>
                </ul>
              </div>
            </AccordionSection>
            <AccordionSection heading="Exclusions" caption="">
              <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
                <ul className="govuk-bulllet-list govuk-!-margin-top-0">
                  <li>
                    <strong>
                      Permanent and fixed-period exclusions in England
                    </strong>{' '}
                    <PrototypeDownloadDropdown link="/prototypes/publication-gcse" />
                  </li>
                </ul>
              </div>
            </AccordionSection>
            <AccordionSection heading="Pupil absence" caption="">
              <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
                <ul className="govuk-bullet-list govuk-!-margin-top-0">
                  <li>
                    <strong>Pupil absence in schools in England</strong>{' '}
                    <PrototypeDownloadDropdown link="/prototypes/publication-gcse" />
                  </li>
                  <li>
                    <strong>
                      Pupil absence in schools in England: autumn term
                    </strong>{' '}
                    - currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-pupil-absence#autumn-term-release">
                      Statistics at DfE
                    </a>
                  </li>
                  <li>
                    <strong>
                      Pupil absence in schools in England: autumn and spring
                    </strong>{' '}
                    - currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-pupil-absence#combined-autumn--and-spring-term-release">
                      Statistics at DfE
                    </a>
                  </li>
                </ul>
              </div>
            </AccordionSection>
            <AccordionSection
              heading="Parental responsibility measures	"
              caption=""
            >
              <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
                <ul className="govuk-bullet-list govuk-!-margin-top-0">
                  <li>
                    <strong>Parental responsibility measures</strong> -
                    currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/parental-responsibility-measures#official-statistics">
                      Statistics at DfE
                    </a>
                  </li>
                </ul>
              </div>
            </AccordionSection>
            <AccordionSection heading="Pupil projections" caption="">
              <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
                <ul className="govuk-bullet-list govuk-!-margin-top-0">
                  <li>
                    <strong>National pupil projections</strong> - currently
                    available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-pupil-projections#documents">
                      Statistics at DfE
                    </a>
                  </li>
                </ul>
              </div>
            </AccordionSection>
            <AccordionSection heading="School and pupil numbers" caption="">
              <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
                <ul className="govuk-bullet-list govuk-!-margin-top-0">
                  <li>
                    <strong>Schools, pupils and their characteristics</strong> -
                    currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-school-and-pupil-numbers#documents">
                      Statistics at DfE
                    </a>
                  </li>
                </ul>
              </div>
            </AccordionSection>
            <AccordionSection heading="School applications" caption="">
              <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
                <ul className="govuk-bullet-list govuk-!-margin-top-0">
                  <li>
                    <strong>
                      Secondary and primary schools applications and offers
                    </strong>{' '}
                    - currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-school-applications#documents">
                      Statistics at DfE
                    </a>
                  </li>
                </ul>
              </div>
            </AccordionSection>
            <AccordionSection heading="School capacity" caption="">
              <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
                <ul className="govuk-bullet-list govuk-!-margin-top-0">
                  <li>
                    <strong>School capacity</strong> - currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-school-capacity#school-capacity-data:-by-academic-year">
                      Statistics at DfE
                    </a>
                  </li>
                </ul>
              </div>
            </AccordionSection>
            <AccordionSection
              heading="Special educational needs (SEN)"
              caption=""
            >
              <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
                <ul className="govuk-bullet-list govuk-!-margin-top-0">
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
            </AccordionSection>
          </Accordion>
        </Details>
        <Details summary="School and college performance - including GCSE and key stage results">
          <h2 className="govuk-heading-l govuk-visually-hidden" id="section-7">
            School and college performance - including GCSE and key stage
            results
          </h2>
          <Accordion id="performance">
            <AccordionSection heading="16 to 19 attainment" caption="">
              <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
                <ul className="govuk-bullet-list govuk-!-margin-top-0">
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
            </AccordionSection>
            <AccordionSection heading="GCSEs (key stage 4)	" caption="">
              <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
                <ul className="govuk-bullet-list govuk-!-margin-top-0">
                  <li>
                    <h3 className="govuk-heading-s govuk-!-margin-bottom-0">
                      GCSE and equivalent results
                    </h3>{' '}
                    <PrototypeDownloadDropdown link="/prototypes/publication-gcse" />
                  </li>
                  <li>
                    <strong>Multi-academy trust performance measures</strong> -
                    currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-gcses-key-stage-4#multi-academy-trust-performance-measures">
                      Statistics at DfE
                    </a>
                  </li>
                  <li>
                    <strong>
                      Revised GCSE and equivalent results in England
                    </strong>{' '}
                    - currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-gcses-key-stage-4#gcse-and-equivalent-results,-including-pupil-characteristics">
                      Statistics at DfE
                    </a>
                  </li>
                  <li>
                    <strong>Secondary school performance tables</strong> -
                    currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-gcses-key-stage-4#secondary-school-performance-tables">
                      Statistics at DfE
                    </a>
                  </li>
                </ul>
              </div>
            </AccordionSection>
            <AccordionSection heading="Key stage 1" caption="">
              <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
                <ul className="govuk-bullet-list govuk-!-margin-top-0">
                  <li>
                    <strong>
                      Phonics screening check and key stage 1 assessments
                    </strong>{' '}
                    - currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-key-stage-1#phonics-screening-check-and-key-stage-1-assessment">
                      Statistics at DfE
                    </a>
                  </li>
                </ul>
              </div>
            </AccordionSection>
            <AccordionSection heading="Key stage 2" caption="">
              <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
                <ul className="govuk-bullet-list govuk-!-margin-top-0">
                  <li>
                    <strong>
                      Key stage 2 national curriculum test: review outcomes
                    </strong>{' '}
                    - currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-key-stage-2#key-stage-2-national-curriculum-tests:-review-outcomes">
                      Statistics at DfE
                    </a>
                  </li>
                  <li>
                    <strong>Multi-academy trust performance measures</strong> -
                    currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-key-stage-2#pupil-attainment-at-key-stage-2">
                      Statistics at DfE
                    </a>
                  </li>
                  <li>
                    <strong>
                      National curriculum assessments at key stage 2
                    </strong>{' '}
                    - currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-key-stage-2#national-curriculum-assessments-at-key-stage-2">
                      Statistics at DfE
                    </a>
                  </li>
                  <li>
                    <strong>Primary school performance tables</strong> -
                    currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-key-stage-2#primary-school-performance-tables">
                      Statistics at DfE
                    </a>
                  </li>
                </ul>
              </div>
            </AccordionSection>
            <AccordionSection
              heading="Outcome based success measures	"
              caption=""
            >
              <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
                <ul className="govuk-bullet-list govuk-!-margin-top-0">
                  <li>
                    <strong>
                      Further education outcome-based success measures
                    </strong>{' '}
                    - currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-outcome-based-success-measures#statistics">
                      Statistics at DfE
                    </a>
                  </li>
                </ul>
              </div>
            </AccordionSection>
            <AccordionSection heading="Performance tables	" caption="">
              <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
                <ul className="govuk-bullet-list govuk-!-margin-top-0">
                  <li>
                    <strong>Primary school performance tables</strong> -
                    currently available via{' '}
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
            </AccordionSection>
          </Accordion>
        </Details>
        <Details summary="Teachers and school workforce">
          <h2 className="govuk-heading-l govuk-visually-hidden" id="section-8">
            Teachers and school workforce
          </h2>
          <Accordion id="teachers-and-workforce">
            <AccordionSection
              heading="Initial teacher training (ITT)	"
              caption=""
            >
              <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
                <ul className="govuk-bullet-list govuk-!-margin-top-0">
                  <li>
                    <strong>
                      Initial teacher training performance profiles
                    </strong>{' '}
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
                    <strong>
                      TSM and initial teacher training allocations
                    </strong>{' '}
                    - currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-teacher-training#teacher-supply-model-and-itt-allocations">
                      Statistics at DfE
                    </a>
                  </li>
                </ul>
              </div>
            </AccordionSection>
            <AccordionSection heading="School workforce	" caption="">
              <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
                <ul className="govuk-bullet-list govuk-!-margin-top-0">
                  <li>
                    <strong>School workforce in England</strong> - currently
                    available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-school-workforce#documents">
                      Statistics at DfE
                    </a>
                  </li>
                </ul>
              </div>
            </AccordionSection>
            <AccordionSection
              heading="Teacher workforce statistics and analysis	"
              caption=""
            >
              <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
                <ul className="govuk-bullet-list govuk-!-margin-top-0">
                  <li>
                    <strong>Teachers analysis compendium</strong> - currently
                    available via{' '}
                    <a href="https://www.gov.uk/government/collections/teacher-workforce-statistics-and-analysis#documents">
                      Statistics at DfE
                    </a>
                  </li>
                </ul>
              </div>
            </AccordionSection>
          </Accordion>
        </Details>
        <Details summary="UK education and training statistics">
          <h2 className="govuk-heading-l govuk-visually-hidden" id="section-9">
            UK education and training statistics
          </h2>
          <Accordion id="uk-statistics">
            <AccordionSection
              heading="UK education and training statistics"
              caption=""
            >
              <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
                <ul className="govuk-bullet-list govuk-!-margin-top-0">
                  <li>
                    <strong>
                      Education and training statistics for the UK
                    </strong>{' '}
                    - currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-education-and-training#documents">
                      Statistics at DfE
                    </a>
                  </li>
                </ul>
              </div>
            </AccordionSection>
          </Accordion>
        </Details>
      </div>
    </PrototypePage>
  );
};

export default BrowseReleasesPage;
