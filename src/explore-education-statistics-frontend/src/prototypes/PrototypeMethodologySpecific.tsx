import React from 'react';
import Accordion from '../components/Accordion';
import AccordionSection from '../components/AccordionSection';
import Link from '../components/Link';
import PrototypeDataTile from './components/PrototypeDataTile';
import PrototypeDownloadDropdown from './components/PrototypeDownloadDropdown';
import PrototypePage from './components/PrototypePage';
import PrototypeSearchForm from './components/PrototypeSearchForm';
import PrototypeTileWithChart from './components/PrototypeTileWithChart';

const BrowseReleasesPage = () => {
  return (
    <PrototypePage
      breadcrumbs={[
        {
          link: '/prototypes/methodology-home',
          text: 'Methodology',
        },
      ]}
    >
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <h1 className="govuk-heading-xl">Specific methodology</h1>
          <p className="govuk-body-l">
            Find methodology and other useful information relating to each
            statistical theme.
          </p>
        </div>
        <div className="govuk-grid-column-one-third">
          <aside className="app-related-items" role="complementary">
            <h2 className="govuk-heading-m" id="releated-content">
              Related content
            </h2>
            <nav role="navigation" aria-labelledby="subsection-title">
              <ul className="govuk-list">
                <li>
                  <Link to="/prototypes/browse-releases">
                    Find statistics and data
                  </Link>
                </li>
              </ul>
            </nav>
          </aside>
        </div>
      </div>

      <Accordion id="methodology-specific">
        <AccordionSection heading="Early years and schools">
          <h3>Absence and exclusions</h3>
          <ul className="govuk-list-bullet">
            <li>
              <Link to="#">exclusions statistics guide</Link>
            </li>
            <li>
              <Link to="/prototypes/methodology-absence">
                pupil absence statistics guide
              </Link>
            </li>
          </ul>
          <hr />
          <h3>Capacity and admissions</h3>
          <ul className="govuk-list-bullet">
            <li>
              <Link to="#">admissions appeals statistics guide</Link>
            </li>
            <li>
              <Link to="#">school capacity statistics guide</Link>
            </li>
          </ul>
          <hr />
          <h3>Results</h3>
          <ul className="govuk-list-bullet">
            <li>
              <Link to="#">KS2 statistics guide</Link>
            </li>
            <li>
              <Link to="#">KS4 statistics guide</Link>
            </li>
            <li>
              <Link to="#">
                phonics screening check and KS1 assessments statistics guide
              </Link>
            </li>
            <li>
              <Link to="#">early years foundation stage profile results</Link>
            </li>
          </ul>
          <hr />
          <h3>School and pupil numbers</h3>
          <ul className="govuk-list-bullet">
            <li>
              {' '}
              <Link to="#">
                School pupils and their characteristics statistics guide
              </Link>
            </li>
            <li>
              <Link to="#">School worksforce statistics guide</Link>
            </li>
          </ul>
          <hr />
          <h3>Teacher numbers</h3>
          <ul className="govuk-list-bullet">
            <li>
              {' '}
              <Link to="#">
                initial teacher training performance statistics guide
              </Link>
            </li>
          </ul>
        </AccordionSection>
        <AccordionSection heading="Higher education">
          <h3>Further education</h3>
          <ul className="govuk-list-bullet">
            <li>
              <Link to="#">destination of leavers statistics guide</Link>
            </li>
            <li>
              <Link to="#">
                apprenticeships and traineeships statistics guide
              </Link>
            </li>
            <li>
              <Link to="#">further education and skills statistics guide</Link>
            </li>
            <li>
              <Link to="#">16 to 18 school performance statistics guide</Link>
            </li>
          </ul>
        </AccordionSection>
        <AccordionSection heading="Social care">
          <h3>Number of children</h3>
          <ul className="govuk-list-bullet">
            <li>
              <Link to="#">children in need statistics guide</Link>
            </li>
            <li>
              <Link to="#">looked after children statistics guide</Link>
            </li>
          </ul>
        </AccordionSection>
      </Accordion>
    </PrototypePage>
  );
};

export default BrowseReleasesPage;
