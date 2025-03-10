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
import styles from '@admin/pages/release/content/ReleaseContentPage.module.scss';
import { ReleaseRouteParams } from '@admin/routes/releaseRoutes';
import permissionService from '@admin/services/permissionService';
import releaseContentService from '@admin/services/releaseContentService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import WarningMessage from '@common/components/WarningMessage';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import ReleasePreviewTableTool from '@admin/pages/release/content/components/ReleasePreviewTableTool';
import getUnresolvedComments from '@admin/pages/release/content/utils/getUnresolvedComments';
import featuredTableService from '@admin/services/featuredTableService';
import ButtonText from '@common/components/ButtonText';
import classNames from 'classnames';
import React, { useState } from 'react';
import { RouteComponentProps } from 'react-router';

const ReleaseContentPageLoaded = () => {
  const { canUpdateRelease, release, featuredTables } =
    useReleaseContentState();
  const [previewFeaturedTableId, setPreviewFeaturedTableId] =
    useState<string>();
  const canPreviewRelease =
    canUpdateRelease || (!canUpdateRelease && !release.published);

  return (
    <EditingContextProvider
      editingMode={canUpdateRelease ? 'edit' : 'preview'}
      unresolvedComments={getUnresolvedComments(release)}
    >
      {({
        editingMode,
        setEditingMode,
        totalUnresolvedComments,
        totalUnsavedBlocks,
      }) => {
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

            {canPreviewRelease && (
              <EditablePageModeToggle
                canUpdateRelease={canUpdateRelease}
                previewLabel="Preview release page"
                showTablePreviewOption
              />
            )}

            {canUpdateRelease && (
              <>
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
              </>
            )}

            <div
              className={classNames({
                [`govuk-width-container ${styles.container}`]:
                  editingMode !== 'table-preview',
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

                    <ReleaseContentHubContextProvider
                      releaseVersionId={release.id}
                    >
                      <ReleaseContent
                        transformFeaturedTableLinks={
                          canPreviewRelease
                            ? (url: string, text: string) => {
                                return (
                                  <ButtonText
                                    onClick={() => {
                                      // the url format is `/data-tables/fast-track/<data-block-parent-id>?featuredTables`
                                      // so split twice to get the dataBlockParentId.
                                      const dataBlockParentId = url
                                        .split('fast-track/')[1]
                                        .split('?')[0];
                                      const featuredTable =
                                        featuredTables?.find(
                                          table =>
                                            table.dataBlockParentId ===
                                            dataBlockParentId,
                                        );
                                      if (featuredTable) {
                                        setEditingMode('table-preview');
                                        setPreviewFeaturedTableId(
                                          featuredTable.dataBlockId,
                                        );
                                      }
                                    }}
                                  >
                                    {text}
                                  </ButtonText>
                                );
                              }
                            : undefined
                        }
                      />
                    </ReleaseContentHubContextProvider>
                  </>
                )}
                {editingMode === 'table-preview' && (
                  <ReleasePreviewTableTool
                    releaseVersionId={release.id}
                    releaseType={release.type}
                    publication={release.publication}
                    featuredTableId={previewFeaturedTableId}
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
  const { releaseVersionId } = match.params;

  const { value, isLoading } =
    useAsyncRetry<ReleaseContentContextState>(async () => {
      const { release, unattachedDataBlocks } =
        await releaseContentService.getContent(releaseVersionId);

      const canUpdateRelease = await permissionService.canUpdateRelease(
        releaseVersionId,
      );

      const featuredTables = await featuredTableService.listFeaturedTables(
        releaseVersionId,
      );

      return {
        release,
        unattachedDataBlocks,
        canUpdateRelease,
        featuredTables,
      };
    }, [releaseVersionId]);

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
