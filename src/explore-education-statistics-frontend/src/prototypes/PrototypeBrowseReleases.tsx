import React from 'react';
import { Accordion } from '../components/Accordion';
import { AccordionSection } from '../components/AccordionSection';
// import Details from '../components/Details';
import Link from '../components/Link';
import PrototypeDataTile from './components/PrototypeDataTile';
import PrototypePage from './components/PrototypePage';
import PrototypeSearchForm from './components/PrototypeSearchForm';
import PrototypeTileWithChart from './components/PrototypeTileWithChart';

const BrowseReleasesPage = () => {
  return (
    <PrototypePage breadcrumbs={[{ text: 'Browse statistical releases' }]}>
      <h1 className="govuk-heading-xl">Browse statistical releases</h1>
      <p className="govuk-body-l">
        Here you can browse DfE statistical releases for{' '}
        <Link to="#schools">schools</Link>,{' '}
        <Link to="#higher-education">higher education</Link> and{' '}
        <Link to="#social-care">social care</Link> in England.
      </p>
      <h2 className="govuk-heading-l">Schools and early years</h2>
      <Accordion id="schools">
        <AccordionSection
          heading="Absence and exclusions"
          caption="Pupil absence, permanent and fixed period exclusions"
        >
          <h3 className="govuk-heading-s">
            Latest schools and early years releases
          </h3>
          <ul className="govuk-list-bullet govuk-!-margin-bottom-9">
            <li>
              {' '}
              <Link
                to="/prototypes/publication"
                className="govuk-heading-s govuk-!-margin-bottom-0"
              >
                Pupil absence release
              </Link>
              <p className="govuk-caption-m govuk-!-margin-top-0 govuk-!-margin-bottom-0">
                Overall absence, authorised absence, unauthorised absence,
                persisitent absence
              </p>
              <Link
                to="/prototypes/publication"
                className="govuk-!-margin-left-3 govuk-!-margin-top-0"
              >
                Download this data
              </Link>
              <Link
                to="/prototypes/publication"
                className="govuk-!-margin-left-3 govuk-!-margin-top-0"
              >
                Download this data
              </Link>
              <Link
                to="/prototypes/publication"
                className="govuk-!-margin-left-3 govuk-!-margin-top-0"
              >
                Explore this data
              </Link>
            </li>
            <li className="govuk-!-margin-top-6">
              <a
                href="publication"
                className="govuk-heading-s govuk-!-margin-bottom-0"
              >
                Permananent and fixed period exclusions release
              </a>
              <p className="govuk-caption-m govuk-!-margin-top-0 govuk-!-margin-bottom-0">
                Permanent exclusions, and fixed period exclusions
              </p>
              <Link
                to="/prototypes/publication"
                className="govuk-!-margin-top-0"
              >
                Download data
              </Link>
            </li>
          </ul>
        </AccordionSection>
        <AccordionSection
          heading="Capacity and exclusions"
          caption="School capacity, admission appeals"
        >
          School finance releases
        </AccordionSection>
        <AccordionSection
          heading="Results"
          caption="Local authority and school finance"
        >
          School finance releases
        </AccordionSection>
        <AccordionSection
          heading="School and pupil numbers"
          caption="Schools, pupils and their characteristics, SEN and EHC plans, SEN in England"
        >
          School finance
        </AccordionSection>
        <AccordionSection
          heading="School finance"
          caption="Local authority and school finance"
        >
          School finance
        </AccordionSection>
        <AccordionSection
          heading="Teacher numbers"
          caption="The number and characteristics of teachers"
        >
          School finance
        </AccordionSection>
      </Accordion>
      <h2 className="govuk-heading-l govuk-!-margin-top-9">
        Higher education releases
      </h2>
      <Accordion id="higher-education">
        <AccordionSection
          heading="Further education"
          caption="Pupil absence, permanent and fixed period exclusions"
        >
          <ul className="govuk-list-bullet govuk-!-margin-bottom-9">
            <li>
              {' '}
              <Link
                to="/prototypes/publication"
                className="govuk-heading-s govuk-!-margin-bottom-0"
              >
                Pupil absence
              </Link>
              <p className="govuk-caption-m govuk-!-margin-top-0 govuk-!-margin-bottom-0">
                Overall absence, authorised absence, unauthorised absence,
                persisitent absence
              </p>
              <Link
                to="/prototypes/publication"
                className="govuk-!-margin-top-0"
              >
                Download data
              </Link>
            </li>
            <li className="govuk-!-margin-top-6">
              <a
                href="publication"
                className="govuk-heading-s govuk-!-margin-bottom-0"
              >
                Permananent and fixed period exclusions
              </a>
              <p className="govuk-caption-m govuk-!-margin-top-0 govuk-!-margin-bottom-0">
                Permanent exclusions, and fixed period exclusions
              </p>
              <Link
                to="/prototypes/publication"
                className="govuk-!-margin-top-0"
              >
                Download data
              </Link>
            </li>
          </ul>
        </AccordionSection>
        <AccordionSection
          heading="Higher education"
          caption="School capacity, admission appeals"
        >
          School finance releases
        </AccordionSection>
      </Accordion>
      <h2 className="govuk-heading-l govuk-!-margin-top-9">
        Social care releases
      </h2>
      <Accordion id="social">
        <AccordionSection
          heading="Number of children"
          caption="Pupil absence, permanent and fixed period exclusions"
        >
          <ul className="govuk-list-bullet govuk-!-margin-bottom-9">
            <li>
              {' '}
              <Link
                to="/prototypes/publication"
                className="govuk-heading-s govuk-!-margin-bottom-0"
              >
                Pupil absence
              </Link>
              <p className="govuk-caption-m govuk-!-margin-top-0 govuk-!-margin-bottom-0">
                Overall absence, authorised absence, unauthorised absence,
                persisitent absence
              </p>
              <Link
                to="/prototypes/publication"
                className="govuk-!-margin-top-0"
              >
                Download data
              </Link>
            </li>
            <li className="govuk-!-margin-top-6">
              <a
                href="publication"
                className="govuk-heading-s govuk-!-margin-bottom-0"
              >
                Permananent and fixed period exclusions
              </a>
              <p className="govuk-caption-m govuk-!-margin-top-0 govuk-!-margin-bottom-0">
                Permanent exclusions, and fixed period exclusions
              </p>
              <Link
                to="/prototypes/publication"
                className="govuk-!-margin-top-0"
              >
                Download data
              </Link>
            </li>
          </ul>
        </AccordionSection>
        <AccordionSection
          heading="Vulnerable children"
          caption="School capacity, admission appeals"
        >
          School finance releases
        </AccordionSection>
      </Accordion>
    </PrototypePage>
  );
};

export default BrowseReleasesPage;
