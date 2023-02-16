import BrowserWarning from '@admin/components/BrowserWarning';
import EditablePageModeToggle from '@admin/components/editable/EditablePageModeToggle';
import { ReleaseContentHubContextProvider } from '@admin/contexts/ReleaseContentHubContext';
import { EditingContextProvider } from '@admin/contexts/EditingContext';
import ReleaseContent from '@admin/pages/release/content/components/ReleaseContent';
import {
  ReleaseContentProvider,
  ReleaseContentContextState,
  useReleaseContentState,
} from '@admin/pages/release/content/contexts/ReleaseContentContext';
import { ReleaseRouteParams } from '@admin/routes/releaseRoutes';
import permissionService from '@admin/services/permissionService';
import releaseContentService from '@admin/services/releaseContentService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import WarningMessage from '@common/components/WarningMessage';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import ReleasePreviewTableTool from '@admin/pages/release/content/components/ReleasePreviewTableTool';
import getUnresolvedComments from '@admin/pages/release/content/utils/getUnresolvedComments';
import classNames from 'classnames';
import React from 'react';
import { RouteComponentProps } from 'react-router';

const ReleaseContentPageLoaded = () => {
  const { canUpdateRelease, release } = useReleaseContentState();

  return (
    <EditingContextProvider
      editingMode={canUpdateRelease ? 'edit' : 'preview'}
      unresolvedComments={getUnresolvedComments(release)}
    >
      {({ editingMode, totalUnresolvedComments, totalUnsavedBlocks }) => {
        return (
          <>
            {editingMode === 'edit' && (
              <BrowserWarning>
                <ul>
                  <li>Editing key statistic guidance text</li>
                  <li>Editing headline text</li>
                  <li>Editing text blocks</li>
                </ul>
              </BrowserWarning>
            )}

            {canUpdateRelease && (
              <div className="govuk-form-group">
                {totalUnsavedBlocks > 0 && (
                  <WarningMessage>
                    {`${totalUnsavedBlocks} content ${
                      totalUnsavedBlocks === 1 ? 'block has' : 'blocks have'
                    } unsaved changes. Clicking away from this tab will result in the changes being lost.`}
                  </WarningMessage>
                )}
                {totalUnresolvedComments > 0 && (
                  <WarningMessage>
                    {totalUnresolvedComments === 1
                      ? 'There is 1 unresolved comment'
                      : `There are ${totalUnresolvedComments} unresolved comments`}
                  </WarningMessage>
                )}

                <EditablePageModeToggle
                  previewLabel="Preview release page"
                  showTablePreviewOption
                />
              </div>
            )}

            <div
              className={classNames({
                'govuk-!-margin-right-0': editingMode === 'edit',
                'govuk-width-container': editingMode !== 'table-preview',
              })}
            >
              <div
                className={
                  editingMode === 'edit'
                    ? 'dfe-page-editing'
                    : 'dfe-page-preview'
                }
              >
                {editingMode !== 'table-preview' && (
                  <>
                    <span className="govuk-caption-l">{release.title}</span>

                    <h2 className="govuk-heading-l dfe-print-break-before">
                      {release.publication.title}
                    </h2>

                    <ReleaseContentHubContextProvider releaseId={release.id}>
                      <ReleaseContent />
                    </ReleaseContentHubContextProvider>
                  </>
                )}
                {editingMode === 'table-preview' && (
                  <ReleasePreviewTableTool
                    releaseId={release.id}
                    publication={release.publication}
                  />
                )}
              </div>
            </div>
          </>
        );
      }}
    </EditingContextProvider>
  );
};

const ReleaseContentPage = ({
  match,
}: RouteComponentProps<ReleaseRouteParams>) => {
  const { releaseId } = match.params;

  const { value, isLoading } = useAsyncRetry<
    ReleaseContentContextState
  >(async () => {
    const {
      release,
      unattachedDataBlocks,
    } = await releaseContentService.getContent(releaseId);

    const canUpdateRelease = await permissionService.canUpdateRelease(
      releaseId,
    );

    return {
      release,
      unattachedDataBlocks,
      canUpdateRelease,
    };
  }, [releaseId]);

  return (
    <LoadingSpinner loading={isLoading}>
      {value ? (
        <ReleaseContentProvider value={value}>
          <ReleaseContentPageLoaded />
        </ReleaseContentProvider>
      ) : (
        <p>There was a problem loading the release content</p>
      )}
    </LoadingSpinner>
  );
};

export default ReleaseContentPage;
