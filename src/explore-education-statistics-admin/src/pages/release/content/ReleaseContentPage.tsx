import BrowserWarning from '@admin/components/BrowserWarning';
import EditablePageModeToggle from '@admin/components/editable/EditablePageModeToggle';
import { EditingContextProvider } from '@admin/contexts/EditingContext';
import ReleaseContent from '@admin/pages/release/content/components/ReleaseContent';
import {
  ReleaseContentProvider,
  ReleaseContextState,
  useReleaseContentState,
} from '@admin/pages/release/content/contexts/ReleaseContentContext';
import { ReleaseRouteParams } from '@admin/routes/releaseRoutes';
import permissionService from '@admin/services/permissionService';
import releaseContentService, {
  EditableRelease,
} from '@admin/services/releaseContentService';
import { Comment, EditableBlock } from '@admin/services/types/content';
import LoadingSpinner from '@common/components/LoadingSpinner';
import WarningMessage from '@common/components/WarningMessage';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import { ContentSection } from '@common/services/publicationService';
import { getNumberOfUnSavedBlocks } from '@admin/pages/release/content/components/utils/unSavedEdits';
import classNames from 'classnames';
import React from 'react';
import { RouteComponentProps } from 'react-router';

const ReleaseContentPageLoaded = () => {
  const {
    canUpdateRelease,
    unresolvedComments,
    release,
  } = useReleaseContentState();

  return (
    <EditingContextProvider
      value={{
        isEditing: canUpdateRelease,
      }}
    >
      {({ isEditing, unSavedEdits }) => {
        const numOfEdits = getNumberOfUnSavedBlocks(unSavedEdits);
        return (
          <>
            {isEditing && (
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
                {numOfEdits > 0 && (
                  <WarningMessage>
                    {numOfEdits === 1
                      ? 'One content block has unsaved changes. Clicking away from this tab will result in the changes being lost.'
                      : `${numOfEdits} content blocks have unsaved changes. Clicking away from this tab will result in the changes being lost.`}
                  </WarningMessage>
                )}
                {unresolvedComments.length > 0 &&
                unresolvedComments.length > 1 ? (
                  <WarningMessage>
                    There are {unresolvedComments.length} unresolved comments
                  </WarningMessage>
                ) : (
                  <WarningMessage>There is 1 unresolved comment</WarningMessage>
                )}

                <EditablePageModeToggle />
              </div>
            )}

            <div
              className={classNames('govuk-width-container', {
                'govuk-!-margin-right-0': isEditing,
              })}
            >
              <div
                className={isEditing ? 'dfe-page-editing' : 'dfe-page-preview'}
              >
                <span className="govuk-caption-l">{release.title}</span>

                <h2 className="govuk-heading-l dfe-print-break-before">
                  {release.publication.title}
                </h2>

                <ReleaseContent />
              </div>
            </div>
          </>
        );
      }}
    </EditingContextProvider>
  );
};

const contentSectionComments = (
  contentSection: ContentSection<EditableBlock>,
): Comment[] => {
  if (contentSection.content?.length) {
    return contentSection.content.reduce<Comment[]>(
      (allCommentsForSection, content) => {
        content.comments.forEach(comment =>
          allCommentsForSection.push(comment),
        );
        return allCommentsForSection;
      },
      [],
    );
  }

  return [];
};

const getUnresolvedComments = (release: EditableRelease) =>
  [
    ...contentSectionComments(release.summarySection),
    ...contentSectionComments(release.keyStatisticsSection),
    ...release.content
      .filter(_ => _.content !== undefined)
      .reduce<Comment[]>(
        (allComments, contentSection) => [
          ...allComments,
          ...contentSectionComments(contentSection),
        ],
        [],
      ),
  ].filter(comment => comment !== undefined);

const ReleaseContentPage = ({
  match,
}: RouteComponentProps<ReleaseRouteParams>) => {
  const { releaseId } = match.params;

  const { value, isLoading } = useAsyncRetry<ReleaseContextState>(async () => {
    const {
      release,
      availableDataBlocks,
    } = await releaseContentService.getContent(releaseId);

    const canUpdateRelease = await permissionService.canUpdateRelease(
      releaseId,
    );

    return {
      release,
      availableDataBlocks,
      canUpdateRelease,
      unresolvedComments: getUnresolvedComments(release),
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
