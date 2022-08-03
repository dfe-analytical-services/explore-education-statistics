import StatusBlock from '@admin/components/StatusBlock';
import getStatusDetail from '@admin/pages/release/utils/getStatusDetail';
import { ReleaseStageStatuses } from '@admin/services/releaseService';
import Details from '@common/components/Details';
import React from 'react';

const notStartedStatuses = ['Validating', 'Invalid'];

interface Props {
  checklistStyle?: boolean;
  currentStatus?: ReleaseStageStatuses;
  includeScheduled?: boolean;
}

const ReleasePublishingStages = ({
  checklistStyle = false,
  currentStatus,
  includeScheduled = false,
}: Props) => {
  if (
    !currentStatus ||
    (includeScheduled
      ? notStartedStatuses.includes(currentStatus.overallStage)
      : [...notStartedStatuses, 'Scheduled'].includes(
          currentStatus.overallStage,
        ))
  ) {
    return null;
  }
  return (
    <Details
      className="govuk-!-margin-bottom-0 govuk-!-margin-top-1"
      summary="View stages"
    >
      {checklistStyle &&
        ![...notStartedStatuses, 'Scheduled'].includes(
          currentStatus.overallStage,
        ) && (
          <p className="govuk-!-font-weight-bold">Release process started</p>
        )}
      <ul className="govuk-list">
        {Object.entries(currentStatus).map(([key, val]) => {
          if (['overallStage', 'releaseId', 'lastUpdated'].includes(key)) {
            return null;
          }
          const { color, text } = getStatusDetail(val);

          if (!color) {
            return null;
          }

          return (
            <li key={key}>
              <StatusBlock
                checklistStyle={checklistStyle}
                color={color}
                text={
                  checklistStyle
                    ? `${key.replace('Stage', '')} ${text}`
                    : `${key.replace('Stage', '')} - ${text}`
                }
              />
            </li>
          );
        })}
      </ul>
      {checklistStyle && currentStatus.overallStage === 'Failed' && (
        <>
          <p className="govuk-!-margin-bottom-0 govuk-!-font-size-16">
            <strong>Help and guidance</strong>
          </p>
          <p className=" govuk-!-font-size-16">
            For extra help and guidance to help rectify this issue please email:{' '}
            <a href="mailto:explore.statistics@education.gov.uk">
              explore.statistics@education.gov.uk
            </a>
          </p>
        </>
      )}
    </Details>
  );
};

export default ReleasePublishingStages;
