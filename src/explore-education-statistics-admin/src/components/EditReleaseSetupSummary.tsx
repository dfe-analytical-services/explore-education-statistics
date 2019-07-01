import React from 'react';
import { Release } from '@admin/services/publicationService';
import { format } from 'date-fns';
import Link from '@admin/components/Link';

interface Props {
  publicationTitle: string;
  release: Release;
}

const EditReleaseSetupSummary = ({ publicationTitle, release }: Props) => {
  return (
    <>
      <h2 className="govuk-heading-m">Release setup summary</h2>

      <dl className="govuk-summary-list govuk-!-margin-bottom-9">
        <div className="govuk-summary-list__row">
          <dt className="govuk-summary-list__key">Publication title</dt>
          <dd className="govuk-summary-list__value">{publicationTitle}</dd>
        </div>
        <div className="govuk-summary-list__row">
          <dt className="govuk-summary-list__key">Release type</dt>
          <dd className="govuk-summary-list__value">
            {release.timePeriodCoverage.label}
          </dd>
        </div>
        <div className="govuk-summary-list__row">
          <dt className="govuk-summary-list__key">Release period</dt>
          <dd className="govuk-summary-list__value">{release.releaseName}</dd>
        </div>
        <div className="govuk-summary-list__row">
          <dt className="govuk-summary-list__key">Lead statistician</dt>
          <dd className="govuk-summary-list__value">{release.lead.name}</dd>
        </div>
        <div className="govuk-summary-list__row">
          <dt className="govuk-summary-list__key">Scheduled release</dt>
          <dd className="govuk-summary-list__value">
            {format(release.scheduledReleaseDate, 'd MMMM yyyy')}
          </dd>
        </div>
        <div className="govuk-summary-list__row">
          <dt className="govuk-summary-list__key" />
          <dd className="govuk-summary-list__actions">
            <Link to="/prototypes/publication-create-new-absence-config-edit">
              Edit release setup details
            </Link>
          </dd>
        </div>
      </dl>
    </>
  );
};

export default EditReleaseSetupSummary;
