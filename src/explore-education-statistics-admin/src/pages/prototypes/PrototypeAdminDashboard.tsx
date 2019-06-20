import AdminDashboardApprovedForPublication from '@admin/pages/prototypes/components/AdminDashboardApprovedForPublication';
import AdminDashboardNeedsWork from '@admin/pages/prototypes/components/AdminDashboardNeedsWork';
import PrototypeAdminDashboardPublications from '@admin/pages/prototypes/components/PrototypeAdminDashboardPublications';
import AdminDashboardReadyForApproval from '@admin/pages/prototypes/components/AdminDashboardReadyForApproval';
import RelatedInformation from '@common/components/RelatedInformation';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import React from 'react';
import { RouteChildrenProps } from 'react-router';
import Link from '../../components/Link';
import PrototypePage from './components/PrototypePage';

const PrototypeBrowseReleasesPage = ({ location }: RouteChildrenProps) => {
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
          <RelatedInformation heading="Help and guidance">
            <ul className="govuk-list">
              <li>
                <Link to="/prototypes/methodology-home">
                  Administrators guide{' '}
                </Link>
              </li>
            </ul>
          </RelatedInformation>
        </div>
      </div>
      <Tabs>
        <TabsSection id="publications" title="Publications">
          <PrototypeAdminDashboardPublications />
        </TabsSection>
        {/* <TabsSection
          id="task-in-progress"
          title={`In progress ${
            location.search === '?status=editNewRelease' ? '(2)' : '(1)'
          }`}
        >
          <AdminDashboardInProgress />
        </TabsSection> */}
        <TabsSection
          id="task-ready-approval1"
          title={`Ready to review ${
            location.search === '?status=readyApproval' ? '(1)' : ''
          }`}
        >
          <AdminDashboardReadyForApproval />
        </TabsSection>
        <TabsSection id="task-ready-approval2" title="Needs work">
          <AdminDashboardNeedsWork />
        </TabsSection>
        <TabsSection id="task-ready-approval3" title="Approved for publication">
          <AdminDashboardApprovedForPublication />
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
              {location.search === '?status=editRelease' && (
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

export default PrototypeBrowseReleasesPage;
