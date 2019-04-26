import React from 'react';
import Link from '../../components/Link';
import PrototypeAdminNavigation from './components/PrototypeAdminNavigation';
import PrototypePage from './components/PrototypePage';

const PublicationSchedulePage = () => {
  return (
    <PrototypePage
      wide
      breadcrumbs={[
        {
          link: '/prototypes/admin-dashboard?status=editNewRelease',
          text: 'Administrator dashboard',
        },
        { text: 'Create new release', link: '#' },
      ]}
    >
      <PrototypeAdminNavigation sectionId="schedule" />

      <dl className="govuk-summary-list">
        <div className="govuk-summary-list__row">
          <dt className="govuk-summary-list__key">Scheduled publish date</dt>
          <dd className="govuk-summary-list__value">To be set</dd>
        </div>
        <div className="govuk-summary-list__row">
          <dt className="govuk-summary-list__key">Scheduled published time</dt>
          <dd className="govuk-summary-list__value">
            <time dateTime="20:00">09:30</time>
          </dd>
        </div>
      </dl>
      <Link to="/prototypes/publication-create-new-absence-schedule-edit">
        Edit scheduled publish date
      </Link>
    </PrototypePage>
  );
};

export default PublicationSchedulePage;
