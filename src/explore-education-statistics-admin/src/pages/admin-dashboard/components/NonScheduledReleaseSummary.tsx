import ButtonLink from '@admin/components/ButtonLink';
import ReleaseSummary from '@admin/pages/admin-dashboard/components/ReleaseSummary';
import { getReleaseSummaryLabel } from '@admin/pages/release/util/releaseSummaryUtil';
import { summaryRoute } from '@admin/routes/edit-release/routes';
import { Release } from '@admin/services/releaseService';
import Button from '@common/components/Button';
import React from 'react';

interface Props {
  release: Release;
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
      actions={
        <>
          {release.amendment ? (
            <>
              <ButtonLink
                to={summaryRoute.generateLink({
                  publicationId: release.publicationId,
                  releaseId: release.id,
                })}
                testId={`Edit release amendment link for ${
                  release.publicationTitle
                }, ${getReleaseSummaryLabel(release)}`}
              >
                {release.permissions.canUpdateRelease
                  ? 'Edit this release amendment'
                  : 'View this release amendment'}
              </ButtonLink>
              <ButtonLink
                to={summaryRoute.generateLink({
                  publicationId: release.publicationId,
                  releaseId: release.previousVersionId,
                })}
                className="govuk-button--secondary govuk-!-margin-left-4"
                testId={`View original release link for ${
                  release.publicationTitle
                }, ${getReleaseSummaryLabel(release)}`}
              >
                View original release
              </ButtonLink>
            </>
          ) : (
            <>
              <ButtonLink
                to={summaryRoute.generateLink({
                  publicationId: release.publicationId,
                  releaseId: release.id,
                })}
                testId={`Edit release link for ${
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
