import DashboardRelease from '@admin/pages/prototypes/components/DashboardRelease';
import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import React from 'react';
import Link from '@admin/components/Link';

const AdminDashboardPublications = () => {
  return (
    <>
      <h2 className="govuk-heading-l govuk-!-margin-bottom-0">
        Pupils and schools, pupil absence
      </h2>
      <p className="govuk-body">
        Edit an existing release or create a new release for current
        publications.
      </p>
      <Link to="/prototypes/publication-create-new" className="govuk-button">
        Create a new publication
      </Link>
      <Accordion id="pupil-absence">
        <AccordionSection
          heading="Pupil absense statistics and data for schools in England"
          caption=""
        >
          <p>
            <Link to="#">View methodology</Link>
          </p>

          <ul className="govuk-list dfe-admin">
            {window.location.search === '?status=readyApproval' && (
              <li>
                <DashboardRelease
                  title="Academic year,"
                  years="2018 to 2019"
                  tag="Ready to review"
                  review
                  lastEdited={new Date('2019-03-20 17:37')}
                  lastEditor={{ id: 'me', name: 'me', permissions: [] }}
                  published={new Date('2019-09-20 09:30')}
                  nextRelease={new Date('2020-09-20 09:30')}
                  dataType="Revised"
                />
              </li>
            )}
            {window.location.search === '?status=editNewRelease' && (
              <li>
                <DashboardRelease
                  title="Academic year,"
                  years="2018 to 2019"
                  tag="New release in progress"
                  editing={window.location.search === '?status=editNewRelease'}
                  isNew
                  lastEdited={new Date('2019-03-20 17:37')}
                  lastEditor={{ id: 'me', name: 'me', permissions: [] }}
                  published={new Date('2019-09-24 09:30')}
                  nextRelease={new Date('2020-09-25 09:30')}
                  dataType="Provisional"
                />
              </li>
            )}
            <li>
              <DashboardRelease
                title="Academic year,"
                years="2017 to 2018"
                tag={
                  window.location.search === '?status=editLiveRelease'
                    ? 'Editing in progress'
                    : ''
                }
                isLatest
                editing={window.location.search === '?status=editLiveRelease'}
                lastEdited={new Date('2018-03-20 17:37')}
                lastEditor={{ id: 'me', name: 'me', permissions: [] }}
                published={new Date('2018-09-24 09:30')}
                nextRelease={new Date('2019-09-23 09:30')}
                dataType="Final"
              />
            </li>
            <li>
              <DashboardRelease
                title="Academic year,"
                years="2016 to 2017"
                isLive
                editing={window.location.search === '?status=editLiveRelease'}
                lastEdited={new Date('2018-03-20 14:23')}
                lastEditor={{ id: 'me', name: 'me', permissions: [] }}
                published={new Date('2017-09-25 09:30')}
                dataType="Final"
              />
            </li>
            <li>
              <DashboardRelease
                title="Academic year,"
                years="2015 to 2016"
                isLive
                editing={window.location.search === '?status=editLiveRelease'}
                lastEdited={new Date('2017-03-20 16:15')}
                lastEditor={{ id: 'me', name: 'me', permissions: [] }}
                published={new Date('2016-03-26 09:30')}
                dataType="Final"
              />
            </li>
          </ul>
          <Link to="/prototypes/release-create-new" className="govuk-button">
            Create new release
          </Link>
        </AccordionSection>
        <AccordionSection
          heading=" Pupil absense statistics and data for schools in England: autumn term"
          caption=""
        >
          <p>
            <Link to="#">View methodology</Link>
          </p>
          <ul className="govuk-list">
            <li>
              <DashboardRelease
                title="Autumn term, academic year, "
                years="2017 to 2018"
                isLatest
                editing={window.location.search === '?status=editLiveRelease'}
                lastEdited={new Date('2019-03-20 17:37')}
                lastEditor={{ id: 'me', name: 'me', permissions: [] }}
                published={new Date('2018-01-08 09:30')}
                nextRelease={new Date('2019-01-07 17:37')}
              />
            </li>

            <li>
              <DashboardRelease
                title="Autumn term, academic year, "
                years="2016 to 2017"
                isLive
                lastEdited={new Date('2019-04-24 16:55')}
                lastEditor={{ id: 'me', name: 'Ann Evans', permissions: [] }}
                published={new Date('2017-01-06 09:30')}
                lead="Ann Evans"
              />
            </li>
          </ul>
          <Link to="/prototypes/release-create-new" className="govuk-button">
            Create new release
          </Link>
        </AccordionSection>
        <AccordionSection
          heading="Pupil absense statistics and data for schools in England: autumn and
        spring terms"
          caption=""
        >
          <p>
            <Link to="#">View methodology</Link>
          </p>
          <ul className="govuk-list">
            <li>
              <DashboardRelease
                title="Autumn and spring terms, academic year, "
                years="2017 to 2018"
                isLatest
                editing={window.location.search === '?status=editLiveRelease'}
                lastEdited={new Date('2019-03-20 17:37')}
                lastEditor={{ id: 'me', name: 'me', permissions: [] }}
                published={new Date('2019-09-24 09:30')}
                nextRelease={new Date('2020-09-25 09:30')}
              />
            </li>

            <li>
              <DashboardRelease
                title="Autumn and spring terms, academic year, "
                years="2016 to 2017"
                editing
                isLive
                lastEdited={new Date('2017-09-23 16:55')}
                lastEditor={{ id: 'me', name: 'Ann Evans', permissions: [] }}
                published={new Date('2019-03-20 09:30')}
                lead="Ann Evans"
              />
            </li>
          </ul>
          <Link to="/prototypes/release-create-new" className="govuk-button">
            Create new release
          </Link>
        </AccordionSection>
        {window.location.search === '?status=newPublication' && (
          <AccordionSection
            heading="Pupil absense statistics and data for schools in England: summer term"
            caption="New publication, requires release adding"
          >
            <p>
              <Link to="#">Add methodology to this publication</Link>
            </p>
            <Link to="/prototypes/release-create-new" className="govuk-button">
              Create new release
            </Link>
          </AccordionSection>
        )}
      </Accordion>
    </>
  );
};

export default AdminDashboardPublications;
