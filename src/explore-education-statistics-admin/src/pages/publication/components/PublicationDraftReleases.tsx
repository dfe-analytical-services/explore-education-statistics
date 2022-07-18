import Link from '@admin/components/Link';
import CancelAmendmentModal from '@admin/pages/admin-dashboard/components/CancelAmendmentModal';
import {
  DraftStatusGuidanceModal,
  IssuesGuidanceModal,
} from '@admin/pages/publication/components/PublicationGuidance';
import usePublicationContext from '@admin/pages/publication/contexts/PublicationContext';
import getUnresolvedComments from '@admin/pages/release/content/utils/getUnresolvedComments';
import {
  getReleaseApprovalStatusLabel,
  getReleaseSummaryLabel,
} from '@admin/pages/release/utils/releaseSummaryUtil';
import releaseContentService from '@admin/services/releaseContentService';
import releaseService, {
  DeleteReleasePlan,
} from '@admin/services/releaseService';
import {
  ReleaseRouteParams,
  releaseSummaryRoute,
} from '@admin/routes/releaseRoutes';
import ButtonText from '@common/components/ButtonText';
import InfoIcon from '@common/components/InfoIcon';
import Tag from '@common/components/Tag';
import VisuallyHidden from '@common/components/VisuallyHidden';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import useToggle from '@common/hooks/useToggle';
import React, { useState } from 'react';
import { generatePath } from 'react-router';

const ErrorsAndWarnings = ({ releaseId }: { releaseId: string }) => {
  const { value: checklist, isLoading } = useAsyncHandledRetry(() =>
    releaseService.getReleaseChecklist(releaseId),
  );
  return (
    <>
      <td>{!isLoading && checklist?.errors.length}</td>
      <td>{!isLoading && checklist?.warnings.length}</td>
    </>
  );
};

const UnresolvedComments = ({ releaseId }: { releaseId: string }) => {
  const { value: releaseContent, isLoading } = useAsyncHandledRetry(() =>
    releaseContentService.getContent(releaseId),
  );
  const unresolvedComments = releaseContent
    ? getUnresolvedComments(releaseContent.release)
    : [];
  return (
    <td>{!isLoading && Object.values(unresolvedComments).flat().length}</td>
  );
};

const PublicationDraftReleases = () => {
  const { draftReleases, onReload } = usePublicationContext();

  const [deleteReleasePlan, setDeleteReleasePlan] = useState<
    DeleteReleasePlan & {
      releaseId: string;
    }
  >();

  const [showDraftStatusGuidance, toggleDraftStatusGuidance] = useToggle(false);
  const [showIssuesGuidance, toggleIssuesGuidance] = useToggle(false);

  return (
    <>
      <table
        className="govuk-!-margin-bottom-9"
        data-testid="publication-draft-releases"
      >
        <caption className="govuk-table__caption--m">Draft releases</caption>
        <thead>
          <tr>
            <th className="govuk-!-width-one-quarter">Release period</th>
            <th>
              State{' '}
              <ButtonText onClick={toggleDraftStatusGuidance.on}>
                <InfoIcon description="Guidance on draft states" />
              </ButtonText>
            </th>
            <th>
              Errors{' '}
              <ButtonText onClick={toggleIssuesGuidance.on}>
                <InfoIcon description="Guidance on draft release issues" />
              </ButtonText>
            </th>
            <th>
              Warnings{' '}
              <ButtonText onClick={toggleIssuesGuidance.on}>
                <InfoIcon description="Guidance on draft release issues" />
              </ButtonText>
            </th>
            <th>
              Unresolved comments{' '}
              <ButtonText onClick={toggleIssuesGuidance.on}>
                <InfoIcon description="Guidance on draft release issues" />
              </ButtonText>
            </th>
            <th className="dfe-align--right">Actions</th>
          </tr>
        </thead>
        <tbody>
          {draftReleases.map(release => (
            <tr key={release.id}>
              <td className="govuk-!-width-one-quarter">{release.title}</td>
              <td>
                <Tag>
                  {release.amendment
                    ? 'Amendment'
                    : getReleaseApprovalStatusLabel(release.approvalStatus)}
                </Tag>
              </td>
              <ErrorsAndWarnings releaseId={release.id} />
              <UnresolvedComments releaseId={release.id} />
              <td className="dfe-align--right">
                {release.amendment && (
                  <>
                    {release.permissions.canDeleteRelease && release.amendment && (
                      <ButtonText
                        onClick={async () => {
                          setDeleteReleasePlan({
                            ...(await releaseService.getDeleteReleasePlan(
                              release.id,
                            )),
                            releaseId: release.id,
                          });
                        }}
                      >
                        Cancel amendment{' '}
                        <VisuallyHidden> for {release.title}</VisuallyHidden>
                      </ButtonText>
                    )}
                    <Link
                      className="govuk-!-margin-left-4"
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
                    >
                      View original
                      <VisuallyHidden> for {release.title}</VisuallyHidden>
                    </Link>
                  </>
                )}
                <Link
                  className="govuk-!-margin-left-4"
                  to={generatePath<ReleaseRouteParams>(
                    releaseSummaryRoute.path,
                    {
                      publicationId: release.publicationId,
                      releaseId: release.id,
                    },
                  )}
                >
                  {release.permissions.canUpdateRelease ? 'Edit' : 'View'}
                  <VisuallyHidden> {release.title}</VisuallyHidden>
                </Link>
              </td>
            </tr>
          ))}
        </tbody>
      </table>

      {deleteReleasePlan && (
        <CancelAmendmentModal
          scheduledMethodologies={deleteReleasePlan.scheduledMethodologies}
          onConfirm={async () => {
            await releaseService.deleteRelease(deleteReleasePlan.releaseId);
            setDeleteReleasePlan(undefined);
            onReload();
          }}
          onCancel={() => setDeleteReleasePlan(undefined)}
        />
      )}

      <DraftStatusGuidanceModal
        open={showDraftStatusGuidance}
        onClose={toggleDraftStatusGuidance.off}
      />
      <IssuesGuidanceModal
        open={showIssuesGuidance}
        onClose={toggleIssuesGuidance.off}
      />
    </>
  );
};

export default PublicationDraftReleases;
