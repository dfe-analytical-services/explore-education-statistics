import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import Details from '@common/components/Details';
import SearchForm from '@common/components/SearchForm';
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
            the section to view:
          </p>
          <ul className="govuk-bulllet-list govuk-!-margin-bottom-6">
            <li>
              up-to-date national statistical headlines, breakdowns and
              explanations
            </li>
            <li>
              charts and tables to help you compare and contrast national and
              regional statistical data and trends
            </li>
          </ul>
          <SearchForm />
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

      <Accordion id="themes">
        <AccordionSection
          heading="Children, early years and social care"
          caption="Including children in need, EYFS, looked after children and social workforce statistics"
        >
          <Details summary="Childcare and early years - including parent and provider surveys">
            <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
              <ul className="govuk-bullet-list govuk-!-margin-top-0">
                <li>
                  <p>
                    <strong>30 hours free childcare</strong> - currently
                    available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-childcare-and-early-years#30-hours-free-childcare">
                      Statistics at DfE
                    </a>
                  </p>
                </li>
                <li>
                  <p>
                    <strong>Childcare and early years providers survey</strong>{' '}
                    - currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-childcare-and-early-years#childcare-and-early-years-providers-survey">
                      Statistics at DfE
                    </a>
                  </p>
                </li>
                <li>
                  <p>
                    <strong>Childcare and early years survey of parents</strong>{' '}
                    - currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-childcare-and-early-years#childcare-and-early-years-survey-of-parents">
                      Statistics at DfE
                    </a>
                  </p>
                </li>
                <li>
                  <p>
                    <strong>
                      Education provision: children under 5 years of age
                    </strong>{' '}
                    - currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-childcare-and-early-years#childcare-and-early-years-survey-of-parents">
                      Statistics at DfE
                    </a>
                  </p>
                </li>
              </ul>
            </div>
          </Details>
          <Details summary="Children in need and child protection">
            <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
              <ul className="govuk-bullet-list govuk-!-margin-top-0">
                <li>
                  <p>
                    <strong>Characteristics of children in need</strong> -
                    currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-childcare-and-early-years#30-hours-free-childcare">
                      Statistics at DfE
                    </a>
                  </p>
                </li>
              </ul>
            </div>
          </Details>
          <Details summary="Children's social work workforce">
            <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
              <ul className="govuk-bulllet-list govuk-!-margin-bottom-9">
                <li>
                  <p>
                    <strong>Children's social work workforce</strong> -
                    currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-childrens-social-care-workforce#statutory-collection">
                      Statistics at DfE
                    </a>
                  </p>
                </li>
              </ul>
            </div>
          </Details>
          <Details summary="Early years foundation stage (EYFS) profile">
            <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
              <ul className="govuk-bulllet-list govuk-!-margin-bottom-9">
                <li>
                  <p>
                    <strong>
                      Early years foundation stage profile results
                    </strong>{' '}
                    - currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-early-years-foundation-stage-profile#results-at-national-and-local-authority-level">
                      Statistics at DfE
                    </a>
                  </p>
                </li>
              </ul>
            </div>
          </Details>
          <Details summary="Looked-after children - inlcuding adoptions">
            <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
              <ul className="govuk-bulllet-list govuk-!-margin-bottom-9">
                <li>
                  <p>
                    <strong>
                      Children looked after in England including adoptions
                    </strong>{' '}
                    - currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-looked-after-children#looked-after-children">
                      Statistics at DfE
                    </a>
                  </p>
                </li>
                <li>
                  <p>
                    <strong>Outcomes for children looked after by LAs</strong> -
                    currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-looked-after-children#outcomes-for-looked-after-children">
                      Statistics at DfE
                    </a>
                  </p>
                </li>
              </ul>
            </div>
          </Details>
          <Details summary="Secure children's homes">
            <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
              <ul className="govuk-bulllet-list govuk-!-margin-bottom-9">
                <li>
                  <p>
                    <strong>
                      Children accommodated in secure children's homes
                    </strong>{' '}
                    - currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-secure-children-s-homes">
                      Statistics at DfE
                    </a>
                  </p>
                </li>
              </ul>
            </div>
          </Details>
        </AccordionSection>

        <AccordionSection
          heading="Destinations of pupils and students"
          caption="Including graduate labour market and not in education, employment or training (NEET) statistics"
        >
          <Details summary="Destinations of key stage 4 and key stage 5 pupils	">
            <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
              <ul className="govuk-bulllet-list govuk-!-margin-bottom-9">
                <li>
                  <p>
                    <strong>Destinations of KS4 and KS5 pupils</strong> -
                    currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-destinations#destinations-after-key-stage-4-and-5">
                      Statistics at DfE
                    </a>
                  </p>
                </li>
              </ul>
            </div>
          </Details>
          <Details summary="Graduate labour market">
            <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
              <ul className="govuk-bulllet-list govuk-!-margin-bottom-9">
                <li>
                  <p>
                    <strong>Graduate labour market statistics</strong> -
                    currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/graduate-labour-market-quarterly-statistics#documents">
                      Statistics at DfE
                    </a>
                  </p>
                </li>
              </ul>
            </div>
          </Details>
          <Details summary="Not in education, employment or training (NEET) and participation">
            <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
              <ul className="govuk-bulllet-list govuk-!-margin-bottom-9">
                <li>
                  <p>
                    <strong>
                      Participation in education, training and employment
                    </strong>{' '}
                    - currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-neet#participation-in-education,-employment-or-training">
                      Statistics at DfE
                    </a>
                  </p>
                </li>
                <li>
                  <p>
                    <strong>NEET statistics quarterly brief</strong> - currently
                    available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-neet#neet:-2016-to-2017-data-">
                      Statistics at DfE
                    </a>
                  </p>
                </li>
              </ul>
            </div>
          </Details>
        </AccordionSection>
        <AccordionSection
          heading="Finance and funding"
          caption="Including local authority (LA) and student loan statistics"
        >
          <Details summary="Local authority and school finance - including academy and planned LA and school expenditure">
            <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
              <ul className="govuk-bulllet-list govuk-!-margin-bottom-9">
                <li>
                  <p>
                    <strong>
                      Income and expenditure in academies in England
                    </strong>{' '}
                    - currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-local-authority-school-finance-data#academy-spending">
                      Statistics at DfE
                    </a>
                  </p>
                </li>
                <li>
                  <p>
                    <strong>LA and school expenditure</strong> - currently
                    available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-local-authority-school-finance-data#local-authority-and-school-finance">
                      Statistics at DfE
                    </a>
                  </p>
                </li>
                <li>
                  <p>
                    <strong>Planned LA and school expenditure</strong> -
                    currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-local-authority-school-finance-data#planned-local-authority-and-school-spending-">
                      Statistics at DfE
                    </a>
                  </p>
                </li>
              </ul>
            </div>
          </Details>
          <Details summary="Student loan forecasts">
            <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
              <ul className="govuk-bulllet-list govuk-!-margin-bottom-9">
                <li>
                  <p>
                    <strong>Student loan forecasts for England</strong> -
                    currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-student-loan-forecasts#documents">
                      Statistics at DfE
                    </a>
                  </p>
                </li>
              </ul>
            </div>
          </Details>
        </AccordionSection>
        <AccordionSection
          heading="Further education"
          caption="Including advanced learner loan, benefit claimant and apprenticeship and traineeship statistics"
        >
          <Details summary="Advanced learner loans">
            <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
              <ul className="govuk-bulllet-list govuk-!-margin-bottom-9">
                <li>
                  <p>
                    <strong>Advanced learner loans applications</strong> -
                    currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/further-education#advanced-learner-loans-applications-2017-to-2018">
                      Statistics at DfE
                    </a>
                  </p>
                </li>
              </ul>
            </div>
          </Details>
          <Details summary="FE Choices - including employer and learner surveys">
            <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
              <ul className="govuk-bulllet-list govuk-!-margin-bottom-9">
                <li>
                  <p>
                    <strong>FE choices employer satisfaction survey</strong> -
                    currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/fe-choices#employer-satisfaction-survey-data">
                      Statistics at DfE
                    </a>
                  </p>
                </li>
                <li>
                  <p>
                    <strong>FE choices learner satisfaction survey</strong> -
                    currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/fe-choices#learner-satisfaction-survey-data">
                      Statistics at DfE
                    </a>
                  </p>
                </li>
              </ul>
            </div>
          </Details>
          <Details summary="Further education and skills -including apprenticeships and traineeships">
            <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
              <ul className="govuk-bulllet-list govuk-!-margin-bottom-9">
                <li>
                  <p>
                    <strong>Apprenticeship and levy statistics</strong> -
                    currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/further-education-and-skills-statistical-first-release-sfr#apprenticeships-and-levy---older-data">
                      Statistics at DfE
                    </a>
                  </p>
                </li>
                <li>
                  <p>
                    <strong>Apprenticeships and traineeships</strong> -
                    currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/further-education-and-skills-statistical-first-release-sfr#apprenticeships-and-traineeships---older-data">
                      Statistics at DfE
                    </a>
                  </p>
                </li>
                <li>
                  <p>
                    <strong>Further education and skills</strong> - currently
                    available via{' '}
                    <a href="https://www.gov.uk/government/collections/further-education-and-skills-statistical-first-release-sfr#fe-and-skills---older-data">
                      Statistics at DfE
                    </a>
                  </p>
                </li>
              </ul>
            </div>
          </Details>
          <Details summary="Further education for benefits claimants">
            <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
              <ul className="govuk-bulllet-list govuk-!-margin-bottom-9">
                <li>
                  <p>
                    <strong>Further education for benefits claimants</strong> -
                    currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/further-education-for-benefit-claimants#documents">
                      Statistics at DfE
                    </a>
                  </p>
                </li>
              </ul>
            </div>
          </Details>

          <Details summary="National achievement rates tables">
            <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
              <ul className="govuk-bulllet-list govuk-!-margin-bottom-9">
                <li>
                  <p>
                    <strong>National achievement rates tables</strong> -
                    currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/sfa-national-success-rates-tables#national-achievement-rates-tables">
                      Statistics at DfE
                    </a>
                  </p>
                </li>
              </ul>
            </div>
          </Details>
        </AccordionSection>

        <AccordionSection
          heading="Higher education"
          caption="Including university graduate employment and participation statistics"
        >
          <Details summary="Higher education graduate employment and earnings">
            <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
              <ul className="govuk-bullet-list govuk-!-margin-top-0">
                <li>
                  <p>
                    <strong>Graduate outcomes (LEO)</strong> - currently
                    available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-higher-education-graduate-employment-and-earnings#documents">
                      Statistics at DfE
                    </a>
                  </p>
                </li>
              </ul>
            </div>
          </Details>
          <Details summary="Higher education statistics - including outcome, performance and staff">
            <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
              <ul className="govuk-bullet-list govuk-!-margin-top-0">
                <li>
                  <p>
                    <strong>Higher education: destinations of leavers</strong> -
                    currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/official-statistics-releases#destinations-of-higher-education-leavers">
                      Statistics at DfE
                    </a>
                  </p>
                </li>
                <li>
                  <p>
                    <strong>
                      Higher education enrolments and qualifications
                    </strong>{' '}
                    - currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/official-statistics-releases#higher-education-enrolments-and-qualifications">
                      Statistics at DfE
                    </a>
                  </p>
                </li>
                <li>
                  <p>
                    <strong>Performance indicators in higher education</strong>{' '}
                    - currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/official-statistics-releases#performance-indicators">
                      Statistics at DfE
                    </a>
                  </p>
                </li>
                <li>
                  <p>
                    <strong>
                      Staff at higher education providers in the UK
                    </strong>{' '}
                    - currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/official-statistics-releases#staff-at-higher-education">
                      Statistics at DfE
                    </a>
                  </p>
                </li>
              </ul>
            </div>
          </Details>
          <Details summary="Participation rates in higher education">
            <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
              <ul className="govuk-bullet-list govuk-!-margin-top-0">
                <li>
                  <p>
                    <strong>Participation rates in higher education</strong> -
                    currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-on-higher-education-initial-participation-rates#participation-rates-in-higher-education-for-england">
                      Statistics at DfE
                    </a>
                  </p>
                </li>
              </ul>
            </div>
          </Details>
          <Details summary="Widening participation in higher education	">
            <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
              <ul className="govuk-bullet-list govuk-!-margin-top-0">
                <li>
                  <p>
                    <strong>Widening participation in higher education</strong>{' '}
                    - currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/widening-participation-in-higher-education#documents">
                      Statistics at DfE
                    </a>
                  </p>
                </li>
              </ul>
            </div>
          </Details>
        </AccordionSection>

        <AccordionSection
          heading="Pupils and schools"
          caption="Including absence, application and offers, capacity, exclusion and special educational needs (SEN) statistics"
        >
          <Details summary="Admission appeals">
            <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
              <ul className="govuk-bullet-list govuk-!-margin-top-0">
                <li>
                  <p>
                    <strong>Admissions appeals in England</strong> - currently
                    available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-admission-appeals#documents">
                      Statistics at DfE
                    </a>
                  </p>
                </li>
              </ul>
            </div>
          </Details>
          <Details summary="Exclusions">
            <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
              <ul className="govuk-bulllet-list govuk-!-margin-top-0">
                <li>
                  <p>
                    <strong>
                      Permanent and fixed-period exclusions in England
                    </strong>{' '}
                    <PrototypeDownloadDropdown link="/prototypes/publication-exclusions" />
                  </p>
                </li>
              </ul>
            </div>
          </Details>
          <Details summary="Pupil absence">
            <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
              <ul className="govuk-bullet-list govuk-!-margin-top-0">
                <li>
                  <p>
                    <strong>Pupil absence in schools in England</strong>{' '}
                    <PrototypeDownloadDropdown link="/prototypes/publication" />
                  </p>
                </li>
                <li>
                  <p>
                    <strong>
                      Pupil absence in schools in England: autumn term
                    </strong>{' '}
                    - currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-pupil-absence#autumn-term-release">
                      Statistics at DfE
                    </a>
                  </p>
                </li>
                <li>
                  <p>
                    <strong>
                      Pupil absence in schools in England: autumn and spring
                    </strong>{' '}
                    - currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-pupil-absence#combined-autumn--and-spring-term-release">
                      Statistics at DfE
                    </a>
                  </p>
                </li>
              </ul>
            </div>
          </Details>
          <Details summary="Parental responsibility measures	">
            <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
              <ul className="govuk-bullet-list govuk-!-margin-top-0">
                <li>
                  <p>
                    <strong>Parental responsibility measures</strong> -
                    currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/parental-responsibility-measures#official-statistics">
                      Statistics at DfE
                    </a>
                  </p>
                </li>
              </ul>
            </div>
          </Details>
          <Details summary="Pupil projections">
            <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
              <ul className="govuk-bullet-list govuk-!-margin-top-0">
                <li>
                  <p>
                    <strong>National pupil projections</strong> - currently
                    available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-pupil-projections#documents">
                      Statistics at DfE
                    </a>
                  </p>
                </li>
              </ul>
            </div>
          </Details>
          <Details summary="School and pupil numbers">
            <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
              <ul className="govuk-bullet-list govuk-!-margin-top-0">
                <li>
                  <p>
                    <strong>Schools, pupils and their characteristics</strong> -
                    currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-school-and-pupil-numbers#documents">
                      Statistics at DfE
                    </a>
                  </p>
                </li>
              </ul>
            </div>
          </Details>
          <Details summary="School applications and offers">
            <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
              <ul className="govuk-bullet-list govuk-!-margin-top-0">
                <li>
                  <p>
                    <strong>
                      Secondary and primary schools applications and offers
                    </strong>{' '}
                    - currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-school-applications#documents">
                      Statistics at DfE
                    </a>
                  </p>
                </li>
              </ul>
            </div>
          </Details>
          <Details summary="School capacity">
            <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
              <ul className="govuk-bullet-list govuk-!-margin-top-0">
                <li>
                  <p>
                    <strong>School capacity</strong> - currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-school-capacity#school-capacity-data:-by-academic-year">
                      Statistics at DfE
                    </a>
                  </p>
                </li>
              </ul>
            </div>
          </Details>
          <Details summary="Special educational needs (SEN)">
            <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
              <ul className="govuk-bullet-list govuk-!-margin-top-0">
                <li>
                  <p>
                    <strong>Special educational needs in England</strong> -
                    currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-special-educational-needs-sen#national-statistics-on-special-educational-needs-in-england">
                      Statistics at DfE
                    </a>
                  </p>
                </li>
                <li>
                  <p>
                    <strong>
                      Special educational needs: analysis and summary of data
                      sources
                    </strong>{' '}
                    - currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-special-educational-needs-sen#analysis-of-children-with-special-educational-needs">
                      Statistics at DfE
                    </a>
                  </p>
                </li>
                <li>
                  <p>
                    <strong>Statements on SEN and EHC plans</strong> - currently
                    available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-special-educational-needs-sen#statements-of-special-educational-needs-(sen)-and-education,-health-and-care-(ehc)-plans">
                      Statistics at DfE
                    </a>
                  </p>
                </li>
              </ul>
            </div>
          </Details>
        </AccordionSection>

        <AccordionSection
          heading="School and college outcomes and performance"
          caption="Including GCSE and key stage statistics"
        >
          <Details summary="16 to 19 attainment - including A levels, level 2 and 3 and other results">
            <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
              <ul className="govuk-bullet-list govuk-!-margin-top-0">
                <li>
                  <p>
                    <strong>
                      16 to 18 school and college performance tables
                    </strong>{' '}
                    - currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-attainment-at-19-years#16-to-18-school-and-college-performance-tables">
                      Statistics at DfE
                    </a>
                  </p>
                </li>
                <li>
                  <p>
                    <strong>A levels and other 16 to 18 results</strong> -
                    currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-attainment-at-19-years#a-levels-and-other-16-to-18-results">
                      Statistics at DfE
                    </a>
                  </p>
                </li>
                <li>
                  <p>
                    <strong>
                      Level 2 and 3 attainment by young people aged 19
                    </strong>{' '}
                    - currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-attainment-at-19-years#level-2-and-3-attainment">
                      Statistics at DfE
                    </a>
                  </p>
                </li>
              </ul>
            </div>
          </Details>
          <Details summary="GCSEs (key stage 4) - including multi-academy trust (MAT) measures and secondary school tables">
            <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
              <ul className="govuk-bullet-list govuk-!-margin-top-0">
                <li>
                  <p>
                    <h3 className="govuk-heading-s govuk-!-margin-bottom-0">
                      GCSE and equivalent results
                    </h3>{' '}
                    <PrototypeDownloadDropdown link="/prototypes/publication-gcse" />
                  </p>
                </li>
                <li>
                  <p>
                    <strong>Multi-academy trust performance measures</strong> -
                    currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-gcses-key-stage-4#multi-academy-trust-performance-measures">
                      Statistics at DfE
                    </a>
                  </p>
                </li>
                <li>
                  <p>
                    <strong>
                      Revised GCSE and equivalent results in England
                    </strong>{' '}
                    - currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-gcses-key-stage-4#gcse-and-equivalent-results,-including-pupil-characteristics">
                      Statistics at DfE
                    </a>
                  </p>
                </li>
                <li>
                  <p>
                    <strong>Secondary school performance tables</strong> -
                    currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-gcses-key-stage-4#secondary-school-performance-tables">
                      Statistics at DfE
                    </a>
                  </p>
                </li>
              </ul>
            </div>
          </Details>
          <Details summary="Key stage 1 - including phonics">
            <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
              <ul className="govuk-bullet-list govuk-!-margin-top-0">
                <li>
                  <p>
                    <strong>
                      Phonics screening check and key stage 1 assessments
                    </strong>{' '}
                    - currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-key-stage-1#phonics-screening-check-and-key-stage-1-assessment">
                      Statistics at DfE
                    </a>
                  </p>
                </li>
              </ul>
            </div>
          </Details>
          <Details summary="Key stage 2 - including multi-academy trust (MAT) measures, national curriculum assessments and primary school tables">
            <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
              <ul className="govuk-bullet-list govuk-!-margin-top-0">
                <li>
                  <p>
                    <strong>
                      Key stage 2 national curriculum test: review outcomes
                    </strong>{' '}
                    - currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-key-stage-2#key-stage-2-national-curriculum-tests:-review-outcomes">
                      Statistics at DfE
                    </a>
                  </p>
                </li>
                <li>
                  <p>
                    <strong>Multi-academy trust performance measures</strong> -
                    currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-key-stage-2#pupil-attainment-at-key-stage-2">
                      Statistics at DfE
                    </a>
                  </p>
                </li>
                <li>
                  <p>
                    <strong>
                      National curriculum assessments at key stage 2
                    </strong>{' '}
                    - currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-key-stage-2#national-curriculum-assessments-at-key-stage-2">
                      Statistics at DfE
                    </a>
                  </p>
                </li>
                <li>
                  <p>
                    <strong>Primary school performance tables</strong> -
                    currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-key-stage-2#primary-school-performance-tables">
                      Statistics at DfE
                    </a>
                  </p>
                </li>
              </ul>
            </div>
          </Details>
          <Details summary="Outcome based success measures	">
            <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
              <ul className="govuk-bullet-list govuk-!-margin-top-0">
                <li>
                  <p>
                    <strong>
                      Further education outcome-based success measures
                    </strong>{' '}
                    - currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-outcome-based-success-measures#statistics">
                      Statistics at DfE
                    </a>
                  </p>
                </li>
              </ul>
            </div>
          </Details>
          <Details summary="Performance tables - including college, primary and secondary school tables">
            <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
              <ul className="govuk-bullet-list govuk-!-margin-top-0">
                <li>
                  <p>
                    <strong>Primary school performance tables</strong> -
                    currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-performance-tables#primary-school-(key-stage-2)">
                      Statistics at DfE
                    </a>
                  </p>
                </li>
                <li>
                  <p>
                    <strong>School and college performance tables</strong> -
                    currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-performance-tables#school-and-college:-post-16-(key-stage-5)">
                      Statistics at DfE
                    </a>
                  </p>
                </li>
                <li>
                  <p>
                    <strong>Secondary school performance tables</strong> -
                    currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-performance-tables#secondary-school-(key-stage-4)">
                      Statistics at DfE
                    </a>
                  </p>
                </li>
              </ul>
            </div>
          </Details>
        </AccordionSection>
        <AccordionSection
          heading="Teachers and school workforce"
          caption="Including initial teacher training (ITT) statistics"
        >
          <Details summary="Initial teacher training (ITT) - including performance profiles, trainee census and training allocations">
            <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
              <ul className="govuk-bullet-list govuk-!-margin-top-0">
                <li>
                  <p>
                    <strong>
                      Initial teacher training performance profiles
                    </strong>{' '}
                    - currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-teacher-training#performance-data">
                      Statistics at DfE
                    </a>
                  </p>
                </li>
                <li>
                  <p>
                    <strong>
                      Initial teacher training: trainee number census
                    </strong>{' '}
                    - currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-teacher-training#census-data">
                      Statistics at DfE
                    </a>
                  </p>
                </li>
                <li>
                  <p>
                    <strong>
                      TSM and initial teacher training allocations
                    </strong>{' '}
                    - currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-teacher-training#teacher-supply-model-and-itt-allocations">
                      Statistics at DfE
                    </a>
                  </p>
                </li>
              </ul>
            </div>
          </Details>
          <Details summary="School workforce">
            <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
              <ul className="govuk-bullet-list govuk-!-margin-top-0">
                <li>
                  <p>
                    <strong>School workforce in England</strong> - currently
                    available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-school-workforce#documents">
                      Statistics at DfE
                    </a>
                  </p>
                </li>
              </ul>
            </div>
          </Details>
          <Details summary="Teacher workforce statistics and analysis	">
            <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
              <ul className="govuk-bullet-list govuk-!-margin-top-0">
                <li>
                  <p>
                    <strong>Teachers analysis compendium</strong> - currently
                    available via{' '}
                    <a href="https://www.gov.uk/government/collections/teacher-workforce-statistics-and-analysis#documents">
                      Statistics at DfE
                    </a>
                  </p>
                </li>
              </ul>
            </div>
          </Details>
        </AccordionSection>
        <AccordionSection
          heading="UK education and training statistics"
          caption="Including summarised expenditure, post-compulsory education, qualification and school statistics"
        >
          <Details summary="UK education and training statistics">
            <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
              <ul className="govuk-bullet-list govuk-!-margin-top-0">
                <li>
                  <p>
                    <strong>
                      Education and training statistics for the UK
                    </strong>{' '}
                    - currently available via{' '}
                    <a href="https://www.gov.uk/government/collections/statistics-education-and-training#documents">
                      Statistics at DfE
                    </a>
                  </p>
                </li>
              </ul>
            </div>
          </Details>
        </AccordionSection>

        <AccordionSection
          heading="Ad hoc and transparency statistics"
          caption="Including statistics which are not part of DfE's regular statistical releases"
        >
          <Details summary="UK education and training statistics">
            <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
              <ul className="govuk-bullet-list govuk-!-margin-top-0">
                <li>
                  <p>
                    <strong>DfE external data shares</strong> - currently
                    available via{' '}
                    <a href="https://www.gov.uk/government/publications/dfe-external-data-shares">
                      Statistics at DfE
                    </a>
                  </p>
                </li>
                <li>
                  <p>
                    <strong>Further education skills index</strong> - currently
                    available via{' '}
                    <a href="https://www.gov.uk/government/publications/further-education-skills-index">
                      Statistics at DfE
                    </a>
                  </p>
                </li>
                <li>
                  <p>
                    <strong>
                      School Improvement support for 2018 to 2019 academic year
                    </strong>{' '}
                    - currently available via{' '}
                    <a href="https://www.gov.uk/government/publications/school-improvement-support-for-2018-to-2019-summary-statistics">
                      Statistics at DfE
                    </a>
                  </p>
                </li>
              </ul>
            </div>
          </Details>
        </AccordionSection>
      </Accordion>
    </PrototypePage>
  );
};

export default BrowseReleasesPage;
