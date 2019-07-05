import DummyReferenceData from '@admin/pages/DummyReferenceData';
import FormattedDate from '@common/components/FormattedDate';
import React, { useEffect, useState } from 'react';
import { RouteComponentProps } from 'react-router';
import { format } from 'date-fns';
import { setupEditRoute } from '@admin/routes/releaseRoutes';
import ReleasePageTemplate from '@admin/pages/edit-release/components/ReleasePageTemplate';
import DummyPublicationsData from '@admin/pages/DummyPublicationsData';
import { ReleaseSetupDetails } from '../../services/publicationService';
import Link from '../../components/Link';

interface MatchProps {
  releaseId: string;
}

const ReleaseSetupPage = ({ match }: RouteComponentProps<MatchProps>) => {
  const { releaseId } = match.params;

  const [releaseSetupDetails, setReleaseSetupDetails] = useState<
    ReleaseSetupDetails
  >();

  useEffect(() => {
    setReleaseSetupDetails(
      DummyPublicationsData.getReleaseSetupDetails(releaseId),
    );
  }, [releaseId]);

  const selectedTimePeriodCoverageGroup =
    releaseSetupDetails && releaseSetupDetails.timePeriodCoverageCode
      ? DummyReferenceData.findTimePeriodCoverageGroup(
          releaseSetupDetails.timePeriodCoverageCode,
        )
      : null;

  return (
    <>
      <ReleasePageTemplate
        releaseId={releaseId}
        publicationTitle={
          releaseSetupDetails ? releaseSetupDetails.publicationTitle : ''
        }
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
              <dt className="govuk-summary-list__key">Coverage type</dt>
              <dd className="govuk-summary-list__value">
                {selectedTimePeriodCoverageGroup &&
                  selectedTimePeriodCoverageGroup.label}
              </dd>
            </div>
            <div className="govuk-summary-list__row">
              <dt className="govuk-summary-list__key">Release period</dt>
              <dd className="govuk-summary-list__value">
                <FormattedDate format="yyyy">
                  {releaseSetupDetails.timePeriodCoverageStartDate}
                </FormattedDate>{' '}
                to{' '}
                <FormattedDate format="yyyy">
                  {(
                    releaseSetupDetails.timePeriodCoverageStartDate.getFullYear() +
                    1
                  ).toString()}
                </FormattedDate>
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
                <FormattedDate>
                  {releaseSetupDetails.scheduledReleaseDate}
                </FormattedDate>
              </dd>
            </div>
            <div className="govuk-summary-list__row">
              <dt className="govuk-summary-list__key">Release type</dt>
              <dd className="govuk-summary-list__value">
                {releaseSetupDetails.releaseType.label}
              </dd>
            </div>
            <div className="govuk-summary-list__row">
              <dt className="govuk-summary-list__key" />
              <dd className="govuk-summary-list__actions">
                <Link to={setupEditRoute.generateLink(releaseId)}>
                  Edit release setup details
                </Link>
              </dd>
            </div>
          </dl>
        )}
      </ReleasePageTemplate>
    </>
  );
};

export default ReleaseSetupPage;
