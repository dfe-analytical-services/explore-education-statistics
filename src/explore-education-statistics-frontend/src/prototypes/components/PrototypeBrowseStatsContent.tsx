import React from 'react';
import Accordion from '../../components/Accordion';
import AccordionSection from '../../components/AccordionSection';
import Link from '../../components/Link';
import PrototypeDownloadDropdown from '../components/PrototypeDownloadDropdown';
import PrototypePage from '../components/PrototypePage';

interface Props {
  viewType: string;
}

const PrototypeBrowseStatsContent = ({ viewType }: Props) => {
  return (
    <PrototypePage
      breadcrumbs={[{ text: 'Find statistics and download data' }]}
    >
      {viewType === 'FIND' && (
        <>
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
          </ul>
        </>
      )}

      {viewType === 'DOWNLOAD' && (
        <>
          <h1 className="govuk-heading-xl">Download data files</h1>
          <p className="govuk-body-l">
            Browse to find the statistics and data you’re looking for and open
            the section to get links to:
          </p>
          <ul className="govuk-bulllet-list">
            <li>download data files in.csv or Excel format</li>
            <li>
              access our data via an Application Programming Interface (or API)
            </li>
          </ul>
          <div className="govuk-inset-text govuk-!-margin-bottom-9">
            <a href="#">Find out more about accessing and using our API</a>
          </div>
        </>
      )}

      <h2 className="govuk-heading-l">Early years and schools</h2>
      <Accordion id="schools">
        <AccordionSection
          heading="Absence and exclusions"
          caption="Pupil absence and permanent and fixed-period exclusions statistics and data"
        >
          <ul className="govuk-list-bullet">
            <li>
              {' '}
              <h3 className="govuk-heading-m govuk-!-margin-bottom-0">
                Pupil absence statistics
              </h3>
              <p className="govuk-caption-m govuk-!-margin-top-0 govuk-!-margin-bottom-1">
                View statistics, create charts and tables and download data
                files for authorised, overall, persistent and unauthorised
                absence
              </p>
              <div className="govuk-!-margin-top-0">
                <PrototypeDownloadDropdown viewType={viewType} />
              </div>
            </li>
            <li className="govuk-!-margin-top-6">
              <h3 className="govuk-heading-m govuk-!-margin-bottom-0">
                Permanent and fixed-period exclusions statistics
              </h3>
              <p className="govuk-caption-m govuk-!-margin-top-0 govuk-!-margin-bottom-1">
                View statistics, create charts and tables and download data
                files for fixed-period and permanent exclusion statistics
              </p>
              <div className="govuk-!-margin-top-0">
                <PrototypeDownloadDropdown
                  link="/prototypes/publication-exclusions"
                  viewType={viewType}
                />
              </div>
            </li>
          </ul>
        </AccordionSection>
        <AccordionSection
          heading="Capacity and exclusions"
          caption="School capacity, admission appeals"
        >
          <h3 className="govuk-heading-s">
            Latest capacity and exclusions releases
          </h3>
        </AccordionSection>
        <AccordionSection
          heading="Results"
          caption="Local authority and school finance"
        >
          <h3 className="govuk-heading-s">Latest results releases</h3>
        </AccordionSection>
        <AccordionSection
          heading="School and pupil numbers"
          caption="Schools, pupils and their characteristics, SEN and EHC plans, SEN in England"
        >
          <h3 className="govuk-heading-s">
            Latest school and pupil numbers releases
          </h3>
        </AccordionSection>
        <AccordionSection
          heading="School finance"
          caption="Local authority and school finance"
        >
          <h3 className="govuk-heading-s">Latest school finance releases</h3>
        </AccordionSection>
        <AccordionSection
          heading="Teacher numbers"
          caption="The number and characteristics of teachers"
        >
          <h3 className="govuk-heading-s">Latest teacher number releases</h3>
        </AccordionSection>
      </Accordion>
      <h2 className="govuk-heading-l govuk-!-margin-top-9">Higher education</h2>
      <Accordion id="higher-education">
        <AccordionSection
          heading="Further education"
          caption="Pupil absence, permanent and fixed period exclusions"
        >
          <h3 className="govuk-heading-s">Latest further education releases</h3>
        </AccordionSection>
        <AccordionSection
          heading="Higher education"
          caption="School capacity, admission appeals"
        >
          <h3 className="govuk-heading-s">Latest higher education releases</h3>
        </AccordionSection>
      </Accordion>
      <h2 className="govuk-heading-l govuk-!-margin-top-9">Social care</h2>
      <Accordion id="social">
        <AccordionSection
          heading="Number of children"
          caption="Pupil absence, permanent and fixed period exclusions"
        >
          <h3 className="govuk-heading-s">
            Latest number of children releases
          </h3>
        </AccordionSection>
        <AccordionSection
          heading="Vulnerable children"
          caption="School capacity, admission appeals"
        >
          <h3 className="govuk-heading-s">Latest school finance releases</h3>
        </AccordionSection>
      </Accordion>
    </PrototypePage>
  );
};

export default PrototypeBrowseStatsContent;
