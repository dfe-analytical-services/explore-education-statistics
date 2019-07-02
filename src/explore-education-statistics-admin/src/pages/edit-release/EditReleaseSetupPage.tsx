import React, { useEffect, useState } from 'react';
import { RouteComponentProps } from 'react-router';
import { format } from 'date-fns';
import EditReleasePageTemplate, {
  ReleaseSection,
} from '@admin/pages/edit-release/components/EditReleasePageTemplate';
import DummyPublicationsData from '@admin/pages/DummyPublicationsData';
import { Release } from '../../services/publicationService';
import Link from '../../components/Link';

interface MatchProps {
  releaseId: string;
}

const EditReleaseSetupPage = ({ match }: RouteComponentProps<MatchProps>) => {
  const { releaseId } = match.params;

  const [release, setRelease] = useState<Release>();

  const [publicationTitle, setPublicationTitle] = useState('');

  useEffect(() => {
    const selectedRelease = DummyPublicationsData.getReleaseById(releaseId);

    const owningPublication = DummyPublicationsData.getOwningPublicationForRelease(
      selectedRelease,
    );

    setRelease(selectedRelease);

    setPublicationTitle(owningPublication ? owningPublication.title : '');
  }, [releaseId]);

  return (
    <EditReleasePageTemplate
      publicationTitle={publicationTitle}
      releaseId={releaseId}
      selectedSection={ReleaseSection.ReleaseSetup}
    >
      <h2 className="govuk-heading-m">Release setup summary</h2>

      {release && (
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
      )}
    </EditReleasePageTemplate>
  );
};

export default EditReleaseSetupPage;
