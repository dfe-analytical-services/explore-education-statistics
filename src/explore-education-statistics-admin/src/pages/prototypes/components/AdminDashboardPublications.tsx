import DashboardRelease from '@admin/pages/prototypes/components/DashboardRelease';
import React from 'react';
import Details from '@common/components/Details';

const AdminDashboardPublications = () => {
  return (
    <>
      <h2 className="govuk-heading-l govuk-!-margin-bottom-0">
        Absence and exclusions
      </h2>
      <p className="govuk-body">
        Edit an existing release or create a new release for current
        publications.
      </p>

      <h2 className="govuk-heading-m">
        Pupil absense statistics and data for schools in England
      </h2>
      <ul className="govuk-list dfe-admin">
        <li>
          <DashboardRelease
            title="Academic year,"
            years="2017 to 2018"
            isLatest
            editing={window.location.search === '?status=editLiveRelease'}
            lastEdited={new Date('2019-03-20 17:37')}
            lastEditor={{ id: 'me', name: 'me', permissions: [] }}
          />
        </li>
        <li>
          <DashboardRelease
            title="Academic year,"
            years="2016 to 2017"
            editing={window.location.search === '?status=editLiveRelease'}
            lastEdited={new Date('2018-03-20 14:23')}
            lastEditor={{ id: 'me', name: 'me', permissions: [] }}
          />
        </li>
        <li>
          <DashboardRelease
            years="2015 to 2016"
            editing={window.location.search === '?status=editLiveRelease'}
            lastEdited={new Date('2017-03-20 16:15')}
            lastEditor={{ id: 'me', name: 'me', permissions: [] }}
          />
        </li>
      </ul>
      <a href="#" className="govuk-button">
        Create new release
      </a>

      <hr />

      <ul className="govuk-list govuk-list--bullet">
        <li>
          <DashboardRelease
            title="Pupil absense statistics and data for schools in England"
            years="2017 to 2018"
            editing={window.location.search === '?status=editLiveRelease'}
            lastEdited={new Date('2019-03-20 17:37')}
            lastEditor={{ id: 'me', name: 'me', permissions: [] }}
          />
        </li>
        {window.location.search === '?status=editNewRelease' && (
          <li>
            <DashboardRelease
              title="Pupil absense statistics and data for schools in England"
              years="2018 to 2019"
              isNew={window.location.search === '?status=editNewRelease'}
              lastEdited={new Date('2019-03-20 17:37')}
              lastEditor={{ id: 'me', name: 'me', permissions: [] }}
            />
          </li>
        )}
        <li>
          <DashboardRelease
            title="Permanent and fixed-period exclusions statistics"
            years="2017 to 2018"
            editing
            isNew={false}
            lastEdited={new Date('2019-04-24 16:55')}
            lastEditor={{ id: 'me', name: 'Ann Evans', permissions: [] }}
          />
        </li>
      </ul>
    </>
  );
};

export default AdminDashboardPublications;
