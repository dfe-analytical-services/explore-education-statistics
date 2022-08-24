import ButtonLink from '@admin/components/ButtonLink';
import ReleaseSummary from '@admin/pages/admin-dashboard/components/ReleaseSummary';
import { getReleaseSummaryLabel } from '@admin/pages/release/utils/releaseSummaryUtil';
import {
  ReleaseRouteParams,
  releaseSummaryRoute,
} from '@admin/routes/releaseRoutes';
import releaseService, {
  DeleteReleasePlan,
  Release,
} from '@admin/services/releaseService';
import Button from '@common/components/Button';
import React, { useState } from 'react';
import { generatePath, useHistory } from 'react-router';
import CancelAmendmentModal from '@admin/pages/admin-dashboard/components/CancelAmendmentModal';
import ModalConfirm from '@common/components/ModalConfirm';

interface Props {
  release: Release;
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
    DeleteReleasePlan & {
      releaseId: string;
    }
  >();

  const [amendReleaseId, setAmendReleaseId] = useState<string>();

  return (
    <>
      <ReleaseSummary
        release={release}
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
                  variant="secondary"
                >
                  {release.permissions?.canUpdateRelease
                    ? 'Edit release amendment'
                    : 'View release amendment'}
                </ButtonLink>
                <ButtonLink
                  to={generatePath<ReleaseRouteParams>(
                    releaseSummaryRoute.path,
                    {
                      publicationId: release.publicationId,
                      releaseId: release.previousVersionId,
                    },
                  )}
                  data-testid={`View original release link for ${
                    release.publicationTitle
                  }, ${getReleaseSummaryLabel(release)}`}
                  variant="secondary"
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
                  variant="secondary"
                >
                  {release.permissions?.canUpdateRelease
                    ? 'Edit release'
                    : 'View release'}
                </ButtonLink>
                {includeCreateAmendmentControls &&
                  release.permissions?.canMakeAmendmentOfRelease && (
                    <Button
                      variant="secondary"
                      onClick={() => setAmendReleaseId(release.id)}
                    >
                      Amend release
                    </Button>
                  )}
              </>
            )}
            {release.permissions?.canDeleteRelease && release.amendment && (
              <Button
                onClick={async () => {
                  setDeleteReleasePlan({
                    ...(await releaseService.getDeleteReleasePlan(release.id)),
                    releaseId: release.id,
                  });
                }}
                variant="warning"
              >
                Cancel amendment
              </Button>
            )}
          </>
        }
      />

      {deleteReleasePlan && (
        <CancelAmendmentModal
          scheduledMethodologies={deleteReleasePlan.scheduledMethodologies}
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
          open
          title="Confirm you want to amend this live release"
          onCancel={() => setAmendReleaseId(undefined)}
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
