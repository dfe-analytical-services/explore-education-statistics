import Link from '@admin/components/Link';
import styles from '@admin/pages/admin-dashboard/components/DraftReleasesTable.module.scss';
import getMethodologyApprovalStatusLabel from '@admin/pages/methodology/utils/getMethodologyApprovalStatusLabel';
import ReleaseStatusChecklistSummary from '@admin/pages/release/components/ReleaseStatusChecklistSummary';
import { getReleaseApprovalStatusLabel } from '@admin/pages/release/utils/releaseSummaryUtil';
import methodologyQueries from '@admin/queries/methodologyQueries';
import { methodologySummaryRoute } from '@admin/routes/methodologyRoutes';
import { publicationReleasesRoute } from '@admin/routes/publicationRoutes';
import { releaseSummaryRoute } from '@admin/routes/releaseRoutes';
import releaseVersionService, {
  DashboardReleaseVersionSummary,
  DeleteReleasePlan,
  ReleaseVersionSummaryWithPermissions,
} from '@admin/services/releaseVersionService';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Tag from '@common/components/Tag';
import VisuallyHidden from '@common/components/VisuallyHidden';
import { useQuery } from '@tanstack/react-query';
import { useState } from 'react';
import { generatePath } from 'react-router';
import CancelAmendmentModal from './CancelAmendmentModal';

interface Props {
  publicationRowSpan?: number;
  release: DashboardReleaseVersionSummary &
    ReleaseVersionSummaryWithPermissions;
  showIssues: boolean;
  showMethodology: boolean;
  onChangeRelease: () => void;
}

const DraftReleaseRow = ({
  publicationRowSpan,
  release,
  showIssues,
  showMethodology,
  onChangeRelease,
}: Props) => {
  const {
    data: methodologyVersions,
    isError: isMethodologyError,
    isLoading: isLoadingMethodology,
    refetch: refetchMethodology,
  } = useQuery({
    ...methodologyQueries.listLatestMethodologyVersions(release.publication.id),
    enabled: showMethodology,
  });
  const ownedMethodology = methodologyVersions?.find(
    methodology => methodology.owned,
  );
  const renderMethodologyStatus = () => {
    if (isLoadingMethodology) {
      return (
        <LoadingSpinner inline size="sm" text="Loading methodology status" />
      );
    }

    if (isMethodologyError) {
      return (
        <p role="alert">
          Could not load methodology status.{' '}
          <ButtonText
            onClick={() => {
              refetchMethodology();
            }}
          >
            Retry
            <VisuallyHidden>
              {' '}
              methodology status for {release.publication.title}
            </VisuallyHidden>
          </ButtonText>
        </p>
      );
    }

    if (!ownedMethodology) {
      return 'No owned methodology';
    }

    return (
      <Tag>{getMethodologyApprovalStatusLabel(ownedMethodology.status)}</Tag>
    );
  };
  const [deleteReleasePlan, setDeleteReleasePlan] = useState<
    DeleteReleasePlan & {
      releaseVersionId: string;
    }
  >();
  return (
    <tr>
      {publicationRowSpan !== undefined && (
        <th scope="rowgroup" rowSpan={publicationRowSpan}>
          <Link
            to={generatePath(publicationReleasesRoute.fullPath, {
              publicationId: release.publication.id,
            })}
            className="govuk-!-font-weight-regular"
          >
            {release.publication.title}
          </Link>
        </th>
      )}
      <th scope="row" className="govuk-!-font-weight-regular">
        {release.title}
      </th>
      <td className="dfe-white-space--nowrap">
        <Tag>
          {`${getReleaseApprovalStatusLabel(release.approvalStatus)}${
            release.amendment ? ' Amendment' : ''
          }`}
        </Tag>
      </td>
      {showIssues && (
        <td>
          <ReleaseStatusChecklistSummary
            publicationId={release.publication.id}
            releaseTitle={release.title}
            releaseVersionId={release.id}
            simple
          />
        </td>
      )}
      {showMethodology && <td>{renderMethodologyStatus()}</td>}
      <td>
        <ButtonGroup className={styles.actions}>
          <Link
            className="govuk-!-margin-right-4 govuk-!-display-inline-block"
            to={generatePath(releaseSummaryRoute.fullPath, {
              publicationId: release.publication.id,
              releaseVersionId: release.id,
            })}
          >
            {release.permissions?.canUpdateReleaseVersion
              ? 'Edit draft'
              : 'View draft'}
            <VisuallyHidden> {release.title}</VisuallyHidden>
          </Link>

          {showMethodology && ownedMethodology && (
            <Link
              className="govuk-!-margin-right-4 govuk-!-display-inline-block"
              to={generatePath(methodologySummaryRoute.fullPath, {
                methodologyId: ownedMethodology.id,
              })}
            >
              View methodology
              <VisuallyHidden> for {release.publication.title}</VisuallyHidden>
            </Link>
          )}

          {release.permissions?.canDeleteReleaseVersion &&
            release.amendment && (
              <CancelAmendmentModal
                scheduledMethodologies={
                  deleteReleasePlan?.scheduledMethodologies
                }
                triggerButton={
                  <ButtonText
                    variant="warning"
                    onClick={async () => {
                      setDeleteReleasePlan({
                        ...(await releaseVersionService.getDeleteReleaseVersionPlan(
                          release.id,
                        )),
                        releaseVersionId: release.id,
                      });
                    }}
                  >
                    Cancel amendment
                    <VisuallyHidden> for {release.title}</VisuallyHidden>
                  </ButtonText>
                }
                onConfirm={async () => {
                  if (deleteReleasePlan) {
                    await releaseVersionService.deleteReleaseVersion(
                      deleteReleasePlan.releaseVersionId,
                    );
                    setDeleteReleasePlan(undefined);
                    onChangeRelease();
                  }
                }}
                onCancel={() => setDeleteReleasePlan(undefined)}
              />
            )}
        </ButtonGroup>
      </td>
    </tr>
  );
};

export default DraftReleaseRow;
