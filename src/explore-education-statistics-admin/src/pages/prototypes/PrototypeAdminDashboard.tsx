// import Accordion from '@common/components/Accordion';
// import AccordionSection from '@common/components/AccordionSection';
// import Details from '@common/components/Details';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import React from 'react';
import { RouteChildrenProps } from 'react-router';
import Link from '../../components/Link';
import PrototypePage from './components/PrototypePage';

const BrowseReleasesPage = ({ location }: RouteChildrenProps) => {
  return (
    <PrototypePage wide breadcrumbs={[{ text: 'Administrator dashboard' }]}>
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <span className="govuk-caption-xl">Welcome</span>
          <h1 className="govuk-heading-xl">
            John Smith{' '}
            <span className="govuk-body-s">
              Not you? <Link to="#">Sign out</Link>
            </span>
          </h1>
        </div>
        <div className="govuk-grid-column-one-third">
          <aside className="app-related-items">
            <h2 className="govuk-heading-m" id="releated-content">
              Help and guidance
            </h2>
            <nav role="navigation" aria-labelledby="subsection-title">
              <ul className="govuk-list">
                <li>
                  <Link to="/prototypes/methodology-home">
                    Administrators guide{' '}
                  </Link>
                </li>
              </ul>
            </nav>
          </aside>
        </div>
      </div>
      <Tabs>
        <TabsSection id="publications" title="Publications">
          <h2 className="govuk-heading-l govuk-!-margin-bottom-0">
            Absence and exclusions
          </h2>
          <p className="govuk-body">
            Edit an existing release or create a new release for current
            publications.
          </p>

          <ul className="govuk-list govuk-list--bullet">
            <li>
              <h3 className="govuk-heading-m govuk-!-margin-bottom-0">
                Pupil absence statistics
              </h3>
              <dl className="govuk-summary-list govuk-!-margin-bottom-9">
                <div className="govuk-summary-list__row">
                  <dt className="govuk-summary-list__key">Current status</dt>
                  <dd className="govuk-summary-list__value">
                    {location.search == '?status=editLiveRelease' && (
                      <span className="govuk-tag">Editing in progress</span>
                    )}{' '}
                    Live (latest release)
                  </dd>
                  <dd className="govuk-summary-list__actions" />
                </div>
                <div className="govuk-summary-list__row">
                  <dt className="govuk-summary-list__key">
                    Release for academic year
                  </dt>
                  <dd className="govuk-summary-list__value">
                    2017 to 2018
                    <Link to="#" className="govuk-!-margin-left-3">
                      Edit a previous release
                    </Link>
                  </dd>
                  <dd className="govuk-summary-list__actions">
                    <Link to="/prototypes/publication-edit">
                      Edit this release
                    </Link>
                  </dd>
                </div>
                <div className="govuk-summary-list__row">
                  <dt className="govuk-summary-list__key">Last edited</dt>
                  <dd className="govuk-summary-list__value  dfe-details-no-margin">
                    {' '}
                    20 March 2019 at 17:37 by <a href="#">me</a>
                  </dd>
                  <dd className="govuk-summary-list__actions">
                    {' '}
                    <Link to="/prototypes/publication-create-new">
                      Create new release
                    </Link>
                  </dd>
                </div>
              </dl>
            </li>
            {location.search == '?status=editNewRelease' && (
              <li>
                <h3 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Pupil absence statistics
                </h3>
                <dl className="govuk-summary-list  govuk-!-margin-bottom-9">
                  <div className="govuk-summary-list__row">
                    <dt className="govuk-summary-list__key">Current status</dt>
                    <dd className="govuk-summary-list__value">
                      <span className="govuk-tag">New release in progress</span>
                    </dd>
                    <dd className="govuk-summary-list__actions">
                      <Link to="/prototypes/publication-create-new-absence-status">
                        Set status
                      </Link>
                    </dd>
                  </div>
                  <div className="govuk-summary-list__row">
                    <div className="govuk-summary-list__key">
                      Release for academic year
                    </div>
                    <div className="govuk-summary-list__value">
                      2018 to 2019
                    </div>
                    <div className="govuk-summary-list__actions">
                      <Link to="/prototypes/publication-create-new-absence-config">
                        View / edit this draft
                      </Link>
                    </div>
                  </div>
                  <div className="govuk-summary-list__row">
                    <dt className="govuk-summary-list__key">Last edited</dt>
                    <dd className="govuk-summary-list__value">
                      {' '}
                      20 March 2019 at 17:37 by <a href="#">me</a>
                    </dd>
                    <dd className="govuk-summary-list__actions" />
                  </div>
                </dl>
              </li>
            )}
            <li>
              <h3 className="govuk-heading-m govuk-!-margin-bottom-0">
                Permanent and fixed-period exclusions statistics
              </h3>
              <dl className="govuk-summary-list  govuk-!-margin-bottom-9">
                <div className="govuk-summary-list__row">
                  <dt className="govuk-summary-list__key">Current status</dt>
                  <dd className="govuk-summary-list__value">
                    <span className="govuk-tag">Editing in progress</span> Live
                    (Latest release)
                  </dd>
                  <dd className="govuk-summary-list__actions" />
                </div>
                <div className="govuk-summary-list__row">
                  <dt className="govuk-summary-list__key">
                    Release for academic year
                  </dt>
                  <dd className="govuk-summary-list__value">
                    2017 to 2018
                    <Link to="#" className="govuk-!-margin-left-3">
                      Edit a previous release
                    </Link>
                  </dd>
                  <dd className="govuk-summary-list__actions">
                    <Link to="/prototypes/publication-edit">
                      View / edit this draft
                    </Link>
                  </dd>
                </div>
                <div className="govuk-summary-list__row">
                  <dt className="govuk-summary-list__key">Last edited</dt>
                  <dd className="govuk-summary-list__value  dfe-details-no-margin">
                    {' '}
                    24 April 2019 at 16:55 by <a href="#">Ann Evans</a>
                  </dd>
                  <dd className="govuk-summary-list__actions">
                    {' '}
                    <Link to="/prototypes/publication-create-new">
                      Create new release
                    </Link>
                  </dd>
                </div>
              </dl>
            </li>
          </ul>
        </TabsSection>
        <TabsSection
          id="task-in-progress"
          title={`In progress ${
            location.search == '?status=editNewRelease' ? '(2)' : '(1)'
          }`}
        >
          {location.search == '?status=editNewRelease' && (
            <>
              <h2 className="govuk-heading-m">New releases in progress</h2>
              <ul className="govuk-list-bullet  govuk-!-margin-bottom-9">
                <li>
                  {' '}
                  <h4 className="govuk-heading-s govuk-!-margin-bottom-0">
                    Pupil absence statistics
                  </h4>
                  <dl className="dfe-meta-content govuk-!-margin-0">
                    <dt className="govuk-caption-m">Last edited: </dt>
                    <dd>
                      20 March 2019 at 17:37 by <a href="#">me</a>
                    </dd>
                  </dl>
                  <div className="govuk-!-margin-top-0">
                    <Link to="/prototypes/publication-create-new-absence-config">
                      Edit release
                    </Link>
                  </div>
                </li>
              </ul>
            </>
          )}
          <h2 className="govuk-heading-m">Editing current releases</h2>
          <ul className="govuk-list-bullet">
            <li className="govuk-!-margin-top-6">
              <h4 className="govuk-heading-s govuk-!-margin-bottom-0">
                Permanent and fixed-period exclusions statistics
              </h4>
              <dl className="dfe-meta-content govuk-!-margin-0">
                <dt className="govuk-caption-m">Last edited: </dt>
                <dd>
                  24 April 2019 at 10:37 <a href="#">me</a>
                </dd>
              </dl>
              <div className="govuk-!-margin-top-0">
                <Link to="#">Edit</Link>
              </div>
            </li>
          </ul>
        </TabsSection>
        <TabsSection
          id="task-ready-approval"
          title={`Ready for approval ${
            location.search == '?status=readyApproval' ? '(1)' : ''
          }`}
        >
          {location.search != '?status=readyApproval' && (
            <div className="govuk-inset-text">
              There are currenly no releases ready for approval
            </div>
          )}
          {location.search == '?status=readyApproval' && (
            <>
              <h2 className="govuk-heading-m">Ready for approval</h2>
              <ul className="govuk-list-bullet">
                <li>
                  {' '}
                  <h4 className="govuk-heading-s govuk-!-margin-bottom-0">
                    Pupil absence statistics
                  </h4>
                  <dl className="dfe-meta-content govuk-!-margin-0">
                    <dt className="govuk-caption-m">Last edited: </dt>
                    <dd>
                      20 March 2019 at 17:37 by <a href="#">me</a>
                    </dd>
                  </dl>
                  <div className="govuk-!-margin-top-0">
                    <Link to="/prototypes/publication-create-new-absence-config">
                      Edit
                    </Link>
                  </div>
                </li>
              </ul>
            </>
          )}
        </TabsSection>
        <TabsSection id="task-ready-approval" title="Needs work">
          <div className="govuk-inset-text">
            There are currenly no releases requiring work
          </div>
        </TabsSection>
        <TabsSection id="task-ready-approval" title="Approved for publication">
          <div className="govuk-inset-text">
            There are currenly no releases approved for publication
          </div>
        </TabsSection>
      </Tabs>

      {/* <h2 className="govuk-heading-l">Early years and schools</h2>
      <Accordion id="schools">
        <AccordionSection
          heading="Absence and exclusions"
          caption="Pupil absence and permanent and fixed-period exclusions statistics and data"
        >
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <ul className="govuk-list-bullet">
              <li>
                {' '}
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Pupil absence statistics{' '}
                </h4>
                <dl className="dfe-meta-content govuk-!-margin-0">
                  <dt className="govuk-caption-m">Published: </dt>
                  <dd>
                    22 September 2018 by <a href="#">William Hendry</a>
                    <br />
                  </dd>
                  <dt className="govuk-caption-m">Last edited: </dt>
                  <dd>
                    20 March 2019 at 17:37 by <a href="#">me</a>
                    <br />
                  </dd>
                  <dt className="govuk-caption-m">Next release due: </dt>
                  <dd>
                    22 September 2019 in <strong>100</strong> days
                  </dd>
                </dl>
                <div className="govuk-!-margin-top-0">
                  <div className="govuk-grid-row">
                    <div className="govuk-grid-column-one-third">
                      <Link to="/prototypes/publication-edit">
                        Edit current release
                      </Link>
                    </div>
                    <div className="govuk-grid-column-one-third">
                      <Link to="/prototypes/publication-create-new">
                        Create new release
                      </Link>
                    </div>
                  </div>
                </div>
              </li>
              {location.search == '?status=editRelease' && (
                <li className="govuk-!-margin-top-6">
                  {' '}
                  <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                    Pupil absence statistics{' '}
                    <span className="govuk-tag">New release in progress</span>
                  </h4>
                  <dl className="dfe-meta-content govuk-!-margin-0">
                    <dt className="govuk-caption-m">Date to be published: </dt>
                    <dd>
                      22 September 2019 in <strong>100</strong> days <br />
                    </dd>
                    <dt className="govuk-caption-m">Last edited: </dt>
                    <dd>
                      20 March 2019 at 17:37 by <a href="#">me</a>
                      <br />
                    </dd>
                  </dl>
                  <div className="govuk-!-margin-top-0">
                    <div className="govuk-grid-row">
                      <div className="govuk-grid-column-one-third">
                        <Link to="/prototypes/publication-create-new-absence-config">
                          Edit this new release
                        </Link>
                      </div>
                    </div>
                  </div>
                </li>
              )}
              <li className="govuk-!-margin-top-6">
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  Permanent and fixed-period exclusions statistics
                </h4>
                <dl className="dfe-meta-content govuk-!-margin-0">
                  <dt className="govuk-caption-m">Published: </dt>
                  <dd>
                    22 September 2018 by <a href="#">Ann Evans</a>
                    <br />
                  </dd>
                  <dt className="govuk-caption-m">Last edited: </dt>
                  <dd>
                    20 March 2019 at 17:37 by <a href="#">me</a>
                    <br />
                  </dd>
                  <dt className="govuk-caption-m">Next release due: </dt>
                  <dd>
                    22 September 2019 in <strong>100</strong> days
                  </dd>
                </dl>
                <div className="govuk-!-margin-top-0">
                  <div className="govuk-grid-row">
                    <div className="govuk-grid-column-one-third">
                      <Link to="#">Edit current release</Link>
                    </div>
                    <div className="govuk-grid-column-one-third">
                      <Link to="#">Create new release</Link>
                    </div>
                  </div>
                </div>
              </li>
            </ul>
          </div>
        </AccordionSection>
        <AccordionSection
          heading="Capacity and admissions"
          caption="School capacity, admission appeals"
        >
          <h3 className="govuk-heading-s">
            Latest capacity and admissions releases
          </h3>
        </AccordionSection>
        <AccordionSection
          heading="Results"
          caption="Local authority and school finance"
        >
          <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
            <ul className="govuk-list-bullet">
              <li>
                {' '}
                <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
                  GCSE and equivalent results in England{' '}
                  <span className="govuk-tag">Editing in progress</span>
                </h4>
                <dl className="dfe-meta-content govuk-!-margin-0">
                  <dt className="govuk-caption-m">Published: </dt>
                  <dd>
                    22 September 2018 by <a href="#">William Hendry</a>
                    <br />
                  </dd>
                  <dt className="govuk-caption-m">Last edited: </dt>
                  <dd>
                    20 March 2019 at 17:37 by <a href="#">me</a>
                    <br />
                  </dd>
                  <dt className="govuk-caption-m">Next release due: </dt>
                  <dd>
                    22 September 2019 in <strong>100</strong> days
                  </dd>
                </dl>
                <div className="govuk-!-margin-top-0">
                  <div className="govuk-grid-row">
                    <div className="govuk-grid-column-one-third">
                      <Link to="/prototypes/publication-edit">
                        Edit current release
                      </Link>
                    </div>
                    <div className="govuk-grid-column-one-third">
                      <Link to="/prototypes/publication-create-new">
                        Create new release
                      </Link>
                    </div>
                  </div>
                </div>
              </li>
            </ul>
          </div>
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
              </Accordion> */}
    </PrototypePage>
  );
};

export default BrowseReleasesPage;
