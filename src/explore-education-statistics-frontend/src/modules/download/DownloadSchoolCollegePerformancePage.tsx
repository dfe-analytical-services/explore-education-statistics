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
          link: '/download/theme',
          name: 'School and college performance',
        },
      ]}
    >
      <PageTitle title="Download data files for school and college performance - including GCSE and key stage results" />
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <Link className="govuk-body" to="/download">
            {' '}
            Change theme
          </Link>
        </div>
        <div className="govuk-grid-column-one-third">
          <div className="govuk-!-margin-bottom-6">
            <PrototypeSearchForm />
          </div>
        </div>
      </div>

      <Accordion id="performance">
        <AccordionSection heading="16 to 19 attainment" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <ul className="govuk-bullet-list govuk-!-margin-bottom-9 govuk-!-margin-top-0">
              <li>
                <p className="govuk-!-margin-top-0">
                  16 to 18 school and college performance tables - currently
                  available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-attainment-at-19-years#16-to-18-school-and-college-performance-tables">
                    Statistics at DfE
                  </a>
                </p>
              </li>
              <li>
                <p>
                  A levels and other 16 to 18 results - currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-attainment-at-19-years#a-levels-and-other-16-to-18-results">
                    Statistics at DfE
                  </a>
                </p>
              </li>
              <li>
                <p>
                  Level 2 and 3 attainment by young people aged 19 - currently
                  available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-attainment-at-19-years#level-2-and-3-attainment">
                    Statistics at DfE
                  </a>
                </p>
              </li>
            </ul>
          </div>
        </AccordionSection>
        <AccordionSection heading="GCSEs (key stage 4)	" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <ul className="govuk-bullet-list govuk-!-margin-bottom-9 govuk-!-margin-top-0">
              <li>
                <p className="govuk-!-margin-top-0">
                  <Link to="#">GCSE and equivalent results</Link>{' '}
                  <strong>(.csv, 55mb)</strong>
                </p>
              </li>
              <li>
                <p>
                  Multi-academy trust performance measures - currently available
                  via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-gcses-key-stage-4#multi-academy-trust-performance-measures">
                    Statistics at DfE
                  </a>
                </p>
              </li>
              <li>
                <p>
                  Revised GCSE and equivalent results in England - currently
                  available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-gcses-key-stage-4#gcse-and-equivalent-results,-including-pupil-characteristics">
                    Statistics at DfE
                  </a>
                </p>
              </li>
              <li>
                <p>
                  Secondary school performance tables - currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-gcses-key-stage-4#secondary-school-performance-tables">
                    Statistics at DfE
                  </a>
                </p>
              </li>
            </ul>
          </div>
        </AccordionSection>
        <AccordionSection heading="Key stage 1" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <ul className="govuk-bullet-list govuk-!-margin-bottom-9 govuk-!-margin-top-0">
              <li>
                <p className="govuk-!-margin-top-0">
                  Phonics screening check and key stage 1 assessments -
                  currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-key-stage-1#phonics-screening-check-and-key-stage-1-assessment">
                    Statistics at DfE
                  </a>
                </p>
              </li>
            </ul>
          </div>
        </AccordionSection>
        <AccordionSection heading="Key stage 2" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <ul className="govuk-bullet-list govuk-!-margin-bottom-9 govuk-!-margin-top-0">
              <li>
                <p className="govuk-!-margin-top-0">
                  Key stage 2 national curriculum test: review outcomes -
                  currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-key-stage-2#key-stage-2-national-curriculum-tests:-review-outcomes">
                    Statistics at DfE
                  </a>
                </p>
              </li>
              <li>
                <p>
                  Multi-academy trust performance measures - currently available
                  via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-key-stage-2#pupil-attainment-at-key-stage-2">
                    Statistics at DfE
                  </a>
                </p>
              </li>
              <li>
                <p>
                  National curriculum assessments at key stage 2 - currently
                  available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-key-stage-2#national-curriculum-assessments-at-key-stage-2">
                    Statistics at DfE
                  </a>
                </p>
              </li>
              <li>
                <p>
                  Primary school performance tables - currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-key-stage-2#primary-school-performance-tables">
                    Statistics at DfE
                  </a>
                </p>
              </li>
            </ul>
          </div>
        </AccordionSection>
        <AccordionSection heading="Outcome based success measures	" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <ul className="govuk-bullet-list govuk-!-margin-bottom-9 govuk-!-margin-top-0">
              <li>
                <p className="govuk-!-margin-top-0">
                  Further education outcome-based success measures - currently
                  available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-outcome-based-success-measures#statistics">
                    Statistics at DfE
                  </a>
                </p>
              </li>
            </ul>
          </div>
        </AccordionSection>
        <AccordionSection heading="Performance tables	" caption="">
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <ul className="govuk-bullet-list govuk-!-margin-bottom-9 govuk-!-margin-top-0">
              <li>
                <p className="govuk-!-margin-top-0">
                  Primary school performance tables - currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-performance-tables#primary-school-(key-stage-2)">
                    Statistics at DfE
                  </a>
                </p>
              </li>
              <li>
                <p>
                  School and college performance tables - currently available
                  via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-performance-tables#school-and-college:-post-16-(key-stage-5)">
                    Statistics at DfE
                  </a>
                </p>
              </li>
              <li>
                <p>
                  Secondary school performance tables - currently available via{' '}
                  <a href="https://www.gov.uk/government/collections/statistics-performance-tables#secondary-school-(key-stage-4)">
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
