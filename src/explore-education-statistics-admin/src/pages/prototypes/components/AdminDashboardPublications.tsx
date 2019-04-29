import { DashboardRelease } from '@admin/pages/prototypes/components/DashboardRelease';
import React from 'react';

export const AdminDashboardPublications = () => {
  return (
    <>
      <h2 className="govuk-heading-l govuk-!-margin-bottom-0">
        Absence and exclusions
      </h2>
      <p className="govuk-body">
        Edit an existing release or create a new release for current
        publications.
      </p>

      <ul className="govuk-list govuk-list--bullet">
        <li>
          <DashboardRelease
            title="Pupil absense statistics and data for schools in England"
            years="2017 to 2018"
            editing={location.search == '?status=editLiveRelease'}
            lastEdited={new Date('2019-03-20 17:37')}
            lastEditor={{ id: 'me', name: 'me', permissions: [] }}
          />
        </li>
        {location.search == '?status=editNewRelease' && (
          <li>
            <DashboardRelease
              title="Pupil absense statistics and data for schools in England"
              years="2018 to 2019"
              isNew={location.search == '?status=editNewRelease'}
              lastEdited={new Date('2019-03-20 17:37')}
              lastEditor={{ id: 'me', name: 'me', permissions: [] }}
            />
          </li>
        )}
        <li>
          <DashboardRelease
            title="Permanent and fixed-period exclusions statistics"
            years="2017 to 2018"
            editing={true}
            isNew={false}
            lastEdited={new Date('2019-04-24 16:55')}
            lastEditor={{ id: 'me', name: 'Ann Evans', permissions: [] }}
          />
        </li>
      </ul>
    </>
  );
};
