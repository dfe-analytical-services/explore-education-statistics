import React from 'react';
import { Accordion } from '../components/Accordion';
import { AccordionSection } from '../components/AccordionSection';
import Link from '../components/Link';
import PrototypeDataTile from './components/PrototypeDataTile';
import PrototypeDownloadDropdown from './components/PrototypeDownloadDropdown';
import PrototypePage from './components/PrototypePage';
import PrototypeSearchForm from './components/PrototypeSearchForm';
import PrototypeTileWithChart from './components/PrototypeTileWithChart';

const BrowseReleasesPage = () => {
  return (
    <PrototypePage breadcrumbs={[{ text: 'Browse statistical releases' }]}>
      <h1 className="govuk-heading-xl">Browse statistical releases</h1>
      <p className="govuk-body-l">
        Here you can browse DfE statistical releases for{' '}
        <a href="#schools">schools</a>,{' '}
        <a href="#higher-education">higher education</a> and{' '}
        <a href="#social">social care</a> in England.
      </p>
      <h2 className="govuk-heading-l">Schools and early years</h2>
      <Accordion id="schools">
        <AccordionSection
          heading="Absence and exclusions"
          caption="Pupil absence, permanent and fixed period exclusions"
        >
          <div className="govuk-inset-text govuk-!-margin-top-0 govuk-!-padding-top-0">
            <h3 className="govuk-heading-m">
              Latest absence and exclusions releases
            </h3>
            <ul className="govuk-list">
              <li>
                {' '}
                <h4 className="govuk-heading-s govuk-!-margin-bottom-0">
                  <Link to="/prototypes/publication">
                    Pupil absence release
                  </Link>
                </h4>
                <p className="govuk-caption-m govuk-!-margin-top-0 govuk-!-margin-bottom-1">
                  Overall absence, authorised absence, unauthorised absence,
                  persisitent absence
                </p>
                <p className="govuk-!-margin-top-0">
                  <PrototypeDownloadDropdown />
                </p>
              </li>
              <li className="govuk-!-margin-top-6">
                <h4 className="govuk-heading-s govuk-!-margin-bottom-0">
                  <Link to="publication">
                    Permananent and fixed period exclusions release
                  </Link>
                </h4>
                <p className="govuk-caption-m govuk-!-margin-top-0 govuk-!-margin-bottom-1">
                  Permanent exclusions, and fixed period exclusions
                </p>
                <p className="govuk-!-margin-top-0">
                  <PrototypeDownloadDropdown />
                </p>
              </li>
            </ul>
          </div>
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

export default BrowseReleasesPage;
