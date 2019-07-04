import React, { useEffect, useState } from 'react';
import { RouteComponentProps } from 'react-router';
import { format } from 'date-fns';
import EditReleasePageTemplate from '@admin/pages/edit-release/components/EditReleasePageTemplate';
import DummyPublicationsData from '@admin/pages/DummyPublicationsData';
import { dataRoute } from '@admin/routes/editReleaseRoutes';
import { ReleaseSetupDetails } from '../../services/publicationService';
import Link from '../../components/Link';

interface MatchProps {
  releaseId: string;
}

const EditReleaseSetupPage = ({ match }: RouteComponentProps<MatchProps>) => {
  const { releaseId } = match.params;

  const [releaseSetupDetails, setReleaseSetupDetails] = useState<
    ReleaseSetupDetails
  >();

  useEffect(() => {
    setReleaseSetupDetails(
      DummyPublicationsData.getReleaseSetupDetails(releaseId),
    );
  }, [releaseId]);

  return (
    <>
      <EditReleasePageTemplate
        releaseId={releaseId}
        publicationTitle={
          releaseSetupDetails ? releaseSetupDetails.publicationTitle : ''
        }
        nextLink={{
          label: dataRoute.title,
          linkTo: dataRoute.generateLink(releaseId),
        }}
      >
        {releaseSetupDetails && (
          <dl className="govuk-summary-list govuk-!-margin-bottom-9">
            <div className="govuk-summary-list__row">
              <dt className="govuk-summary-list__key">Publication title</dt>
              <dd className="govuk-summary-list__value">
                {releaseSetupDetails.publicationTitle}
              </dd>
            </div>
            <div className="govuk-summary-list__row">
              <dt className="govuk-summary-list__key">Release type</dt>
              <dd className="govuk-summary-list__value">
                {releaseSetupDetails.releaseType}
              </dd>
            </div>
            <div className="govuk-summary-list__row">
              <dt className="govuk-summary-list__key">Release period</dt>
              <dd className="govuk-summary-list__value">
                {releaseSetupDetails.releaseName}
              </dd>
            </div>
            <div className="govuk-summary-list__row">
              <dt className="govuk-summary-list__key">Lead statistician</dt>
              <dd className="govuk-summary-list__value">
                {releaseSetupDetails.leadStatisticianName}
              </dd>
            </div>
            <div className="govuk-summary-list__row">
              <dt className="govuk-summary-list__key">Scheduled release</dt>
              <dd className="govuk-summary-list__value">
                {format(
                  releaseSetupDetails.scheduledReleaseDate,
                  'd MMMM yyyy',
                )}
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
    </>
  );
};

export default EditReleaseSetupPage;
