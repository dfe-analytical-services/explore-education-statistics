import ButtonLink from '@admin/components/ButtonLink';
import ReleaseSummary from '@admin/pages/admin-dashboard/components/ReleaseSummary';
import { getReleaseSummaryLabel } from '@admin/pages/release/utils/releaseSummaryUtil';
import {
  ReleaseRouteParams,
  releaseSummaryRoute,
} from '@admin/routes/releaseRoutes';
import { MyRelease } from '@admin/services/releaseService';
import Button from '@common/components/Button';
import React from 'react';
import { generatePath } from 'react-router';

interface Props {
  release: MyRelease;
  onClickAmendRelease?: (releaseId: string) => void;
  onClickCancelAmendment: (releaseId: string) => void;
}

const NonScheduledReleaseSummary = ({
  release,
  onClickAmendRelease,
  onClickCancelAmendment,
}: Props) => {
  return (
    <ReleaseSummary
      release={release}
      open={release.live && release.latestRelease}
      actions={
        <>
          {release.amendment ? (
            <>
              <ButtonLink
                to={generatePath<ReleaseRouteParams>(releaseSummaryRoute.path, {
                  publicationId: release.publicationId,
                  releaseId: release.id,
                })}
                data-testid={`Edit release amendment link for ${
                  release.publicationTitle
                }, ${getReleaseSummaryLabel(release)}`}
              >
                {release.permissions.canUpdateRelease
                  ? 'Edit this release amendment'
                  : 'View this release amendment'}
              </ButtonLink>
              <ButtonLink
                to={generatePath<ReleaseRouteParams>(releaseSummaryRoute.path, {
                  publicationId: release.publicationId,
                  releaseId: release.previousVersionId,
                })}
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
                to={generatePath<ReleaseRouteParams>(releaseSummaryRoute.path, {
                  publicationId: release.publicationId,
                  releaseId: release.id,
                })}
                data-testid={`Edit release link for ${
                  release.publicationTitle
                }, ${getReleaseSummaryLabel(release)}`}
              >
                {release.permissions.canUpdateRelease
                  ? 'Edit this release'
                  : 'View this release'}
              </ButtonLink>
              {onClickAmendRelease &&
                release.permissions.canMakeAmendmentOfRelease && (
                  <Button
                    className="govuk-button--secondary govuk-!-margin-left-4"
                    onClick={() => onClickAmendRelease(release.id)}
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
            onClick={() => onClickCancelAmendment(release.id)}
            className="govuk-button--warning"
          >
            Cancel amendment
          </Button>
        )
      }
    />
  );
};

export default NonScheduledReleaseSummary;
