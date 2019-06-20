import { ReleaseSummary } from '@common/services/publicationService';
import DashboardRelease from '@admin/components/DashboardRelease';
import Link from '@admin/components/Link';
import React from 'react';

export interface DashboardReleaseListProps {
  releases: ReleaseSummary[];
}

const DashboardReleaseList = ({ releases }: DashboardReleaseListProps) => (
  <>
    <dl className="govuk-summary-list">
      <div className="govuk-summary-list__row">
        <dt className="govuk-summary-list__key dfe-summary-list__key--small">
          Releases
        </dt>
        <dd className="govuk-summary-list__value">
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
                  showComments
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
        </dd>
      </div>
    </dl>
    <Link to="/prototypes/release-create-new" className="govuk-button">
      Create a new release
    </Link>
  </>
);

export default DashboardReleaseList;
