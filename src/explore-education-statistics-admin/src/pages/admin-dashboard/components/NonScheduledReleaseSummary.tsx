import ButtonLink from '@admin/components/ButtonLink';
import ReleaseSummary from '@admin/pages/admin-dashboard/components/ReleaseSummary';
import { getReleaseSummaryLabel } from '@admin/pages/release/utils/releaseSummaryUtil';
import {
  ReleaseRouteParams,
  releaseSummaryRoute,
} from '@admin/routes/releaseRoutes';
import releaseService, {
  DeleteReleasePlan,
  MyRelease,
} from '@admin/services/releaseService';
import Button from '@common/components/Button';
import React, { useState } from 'react';
import { generatePath, useHistory } from 'react-router';
import CancelAmendmentModal from '@admin/pages/admin-dashboard/components/CancelAmendmentModal';
import ModalConfirm from '@common/components/ModalConfirm';

interface Props {
  release: MyRelease;
  includeCreateAmendmentControls?: boolean;
  onAmendmentCancelled: (releaseId: string) => void;
}

const NonScheduledReleaseSummary = ({
  release,
  includeCreateAmendmentControls = false,
  onAmendmentCancelled,
}: Props) => {
  const history = useHistory();

  const [deleteReleasePlan, setDeleteReleasePlan] = useState<
    DeleteReleasePlan
  >();

  const [amendReleaseId, setAmendReleaseId] = useState<string>();

  return (
    <>
      <ReleaseSummary
        release={release}
        open={release.live && release.latestRelease}
        actions={
          <>
            {release.amendment ? (
              <>
                <ButtonLink
                  to={generatePath<ReleaseRouteParams>(
                    releaseSummaryRoute.path,
                    {
                      publicationId: release.publicationId,
                      releaseId: release.id,
                    },
                  )}
                  data-testid={`Edit release amendment link for ${
                    release.publicationTitle
                  }, ${getReleaseSummaryLabel(release)}`}
                >
                  {release.permissions.canUpdateRelease
                    ? 'Edit this release amendment'
                    : 'View this release amendment'}
                </ButtonLink>
                <ButtonLink
                  to={generatePath<ReleaseRouteParams>(
                    releaseSummaryRoute.path,
                    {
                      publicationId: release.publicationId,
                      releaseId: release.previousVersionId,
                    },
                  )}
                  className="govuk-button--secondary govuk-!-margin-left-4"
                  data-testid={`View original release link for ${
                    release.publicationTitle
                  }, ${getReleaseSummaryLabel(release)}`}
                >
                  View original release
                </ButtonLink>
              </>
            ) : (
              <>
                <ButtonLink
                  to={generatePath<ReleaseRouteParams>(
                    releaseSummaryRoute.path,
                    {
                      publicationId: release.publicationId,
                      releaseId: release.id,
                    },
                  )}
                  data-testid={`Edit release link for ${
                    release.publicationTitle
                  }, ${getReleaseSummaryLabel(release)}`}
                >
                  {release.permissions.canUpdateRelease
                    ? 'Edit this release'
                    : 'View this release'}
                </ButtonLink>
                {includeCreateAmendmentControls &&
                  release.permissions.canMakeAmendmentOfRelease && (
                    <Button
                      className="govuk-button--secondary govuk-!-margin-left-4"
                      onClick={() => setAmendReleaseId(release.id)}
                    >
                      Amend this release
                    </Button>
                  )}
              </>
            )}
          </>
        }
        secondaryActions={
          release.permissions.canDeleteRelease &&
          release.amendment && (
            <Button
              onClick={async () => {
                setDeleteReleasePlan(
                  await releaseService.getDeleteReleasePlan(release.id),
                );
              }}
              className="govuk-button--warning"
            >
              Cancel amendment
            </Button>
          )
        }
      />

      {deleteReleasePlan && (
        <CancelAmendmentModal
          methodologiesScheduledWithRelease={
            deleteReleasePlan.methodologiesScheduledWithRelease
          }
          onConfirm={async () => {
            await releaseService.deleteRelease(deleteReleasePlan.releaseId);
            setDeleteReleasePlan(undefined);
            onAmendmentCancelled(deleteReleasePlan.releaseId);
          }}
          onCancel={() => setDeleteReleasePlan(undefined)}
        />
      )}

      {amendReleaseId && (
        <ModalConfirm
          title="Confirm you want to amend this live release"
          onConfirm={async () => {
            const amendment = await releaseService.createReleaseAmendment(
              amendReleaseId,
            );

            history.push(
              generatePath<ReleaseRouteParams>(releaseSummaryRoute.path, {
                publicationId: release.publicationId,
                releaseId: amendment.id,
              }),
            );
          }}
          onExit={() => setAmendReleaseId(undefined)}
          onCancel={() => setAmendReleaseId(undefined)}
          open
        >
          <p>
            Please note, any changes made to this live release must be approved
            before updates can be published.
          </p>
        </ModalConfirm>
      )}
    </>
  );
};

export default NonScheduledReleaseSummary;
