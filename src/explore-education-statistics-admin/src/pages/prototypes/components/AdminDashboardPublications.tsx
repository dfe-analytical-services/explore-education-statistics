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
      <Accordion id="pupil-absence">
        <AccordionSection
          heading="Pupil absense statistics and data for schools in England"
          caption=""
        >
          <ul className="govuk-list dfe-admin">
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
                  published={new Date('2019-09-20 09:30')}
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
                lastEdited={new Date('2019-03-20 17:37')}
                lastEditor={{ id: 'me', name: 'me', permissions: [] }}
                published={new Date('2019-03-20 09:30')}
              />
            </li>
            <li>
              <DashboardRelease
                title="Academic year,"
                years="2016 to 2017"
                editing={window.location.search === '?status=editLiveRelease'}
                lastEdited={new Date('2018-03-20 14:23')}
                lastEditor={{ id: 'me', name: 'me', permissions: [] }}
                published={new Date('2019-03-20 09:30')}
              />
            </li>
            <li>
              <DashboardRelease
                title="Academic year,"
                years="2015 to 2016"
                editing={window.location.search === '?status=editLiveRelease'}
                lastEdited={new Date('2017-03-20 16:15')}
                lastEditor={{ id: 'me', name: 'me', permissions: [] }}
                published={new Date('2019-03-20 09:30')}
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
          <ul className="govuk-list">
            <li>
              <DashboardRelease
                title="Autumn term, academic year, "
                years="2017 to 2018"
                isLatest
                editing={window.location.search === '?status=editLiveRelease'}
                lastEdited={new Date('2019-03-20 17:37')}
                lastEditor={{ id: 'me', name: 'me', permissions: [] }}
                published={new Date('2019-03-20 09:30')}
              />
            </li>

            <li>
              <DashboardRelease
                title="Autumn term, academic year, "
                years="2016 to 2017"
                editing
                isNew={false}
                lastEdited={new Date('2019-04-24 16:55')}
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
        <AccordionSection
          heading="Pupil absense statistics and data for schools in England: autumn and
        spring terms"
          caption=""
        >
          <ul className="govuk-list">
            <li>
              <DashboardRelease
                title="Autumn and spring terms, academic year, "
                years="2017 to 2018"
                isLatest
                editing={window.location.search === '?status=editLiveRelease'}
                lastEdited={new Date('2019-03-20 17:37')}
                lastEditor={{ id: 'me', name: 'me', permissions: [] }}
                published={new Date('2019-03-20 09:30')}
              />
            </li>

            <li>
              <DashboardRelease
                title="Autumn and spring terms, academic year, "
                years="2016 to 2017"
                editing
                isNew={false}
                lastEdited={new Date('2019-04-24 16:55')}
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
      </Accordion>

      <Link
        to="/prototypes/publication-create-new"
        className="govuk-button govuk-button--secondary"
      >
        Create new publication
      </Link>
    </>
  );
};

export default AdminDashboardPublications;
