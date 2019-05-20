import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import Link from '@frontend/components/Link';
import Page from '@frontend/components/Page';
import PageTitle from '@frontend/components/PageTitle';
import React from 'react';
import { RouteChildrenProps } from '../../../../explore-education-statistics-admin/node_modules/@types/react-router';

const DownloadIndexPage = ({ location }: RouteChildrenProps) => {
  return (
    <Page
      breadcrumbs={[
        {
          link: '/download',
          name: 'Download',
        },
      ]}
    >
      <PageTitle title="Download data files" />

      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <p className="govuk-body-l">
            Lorem ipsum dolor sit amet, consectetur adipisicing elit. Quidem
            optio tenetur a molestias totam vero consectetur reiciendis dicta
            ipsum alias. Ut saepe, minima esse veniam dolorum fuga reprehenderit
            fugiat ducimus.
          </p>
        </div>
        <div className="govuk-grid-column-one-third">
          <aside className="app-related-items">
            <h2 className="govuk-heading-m" id="releated-content">
              Related content
            </h2>
            <nav role="navigation" aria-labelledby="subsection-title">
              <ul className="govuk-list">
                <li>
                  <Link to="/statistics">Find statistics and data</Link>
                </li>
                <li>
                  <Link to="/glossary">Education statistics: glossary</Link>
                </li>
              </ul>
            </nav>
          </aside>
        </div>
      </div>

      <h2 className="govuk-heading-l">Choose a theme</h2>

      <ul className="govuk-list">
        <li>
          <a href="#">Children and early years - including social care</a>
        </li>
        <li>
          <a href="#">Destinations of pupils and students - including NEET</a>
        </li>
        <li>
          <a href="#">Finance and funding</a>
        </li>
        <li>
          <a href="#">Further education</a>
        </li>
        <li>
          <a href="#">Higher education</a>
        </li>
        <li>
          <a href="#">Pupils and schools</a>
        </li>
        <li>
          <a href="#">
            Schools and college performance - including GCSE and key stage
            performance
          </a>
        </li>
        <li>
          <a href="#">Teachers and school workforce</a>
        </li>
        <li>
          <a href="#">UK education and training statistics</a>
        </li>
      </ul>

      <Accordion id="methodology-specific">
        <AccordionSection heading="Early years and schools">
          <h3>Absence and exclusions</h3>
          <ul className="govuk-list-bullet">
            <li>
              <Link to="#">Pupil exclusion statistics: methodology</Link>
            </li>
            <li>
              <Link to="#">Pupil absence statistics: methodology</Link>
            </li>
          </ul>
          <hr />
          <h3>Capacity and admissions</h3>
          <ul className="govuk-list-bullet">
            <li>
              <Link to="#">Admissions and appeals statistics: methodology</Link>
            </li>
            <li>
              <Link to="#">School capacity statistics: methodology</Link>
            </li>
          </ul>
          <hr />
          <h3>Results</h3>
          <ul className="govuk-list-bullet">
            <li>
              <Link to="#">Key stage 1 (KS1) statistics: methodology</Link>
            </li>
            <li>
              <Link to="#">Key stage 4 (KS4) statistics: methodology</Link>
            </li>
            <li>
              <Link to="#">
                Phonics screening check and KS1 assessments statistics:
                methodology
              </Link>
            </li>
            <li>
              <Link to="#">
                Early years foundation stage (EYFS) profile statistics:
                methodology
              </Link>
            </li>
          </ul>
          <hr />
          <h3>School and pupil numbers</h3>
          <ul className="govuk-list-bullet">
            <li>
              {' '}
              <Link to="#">
                School pupil characteristics statistics: methodology
              </Link>
            </li>
            <li>
              <Link to="#">School worksforce statistics: methodology</Link>
            </li>
          </ul>
          <hr />
          <h3>Teacher numbers</h3>
          <ul className="govuk-list-bullet">
            <li>
              {' '}
              <Link to="#">
                Initial teacher training (ITT) performance statistics:
                methodology
              </Link>
            </li>
          </ul>
        </AccordionSection>
        <AccordionSection heading="Higher education">
          <h3>Further education</h3>
          <ul className="govuk-list-bullet">
            <li>
              <Link to="#">Destination of leavers statistics: methodology</Link>
            </li>
            <li>
              <Link to="#">
                Apprenticeships and traineeships statistics: methodology
              </Link>
            </li>
            <li>
              <Link to="#">
                Further education and skills statistics: methodology
              </Link>
            </li>
            <li>
              <Link to="#">
                16 to 18 school performance statistics: methodology
              </Link>
            </li>
          </ul>
        </AccordionSection>
        <AccordionSection heading="Social care">
          <h3>Number of children</h3>
          <ul className="govuk-list-bullet">
            <li>
              <Link to="#">Children in need statistics: methodology</Link>
            </li>
            <li>
              <Link to="#">Looked after children statistics: methodology</Link>
            </li>
          </ul>
        </AccordionSection>
      </Accordion>
    </Page>
  );
};

export default DownloadIndexPage;
