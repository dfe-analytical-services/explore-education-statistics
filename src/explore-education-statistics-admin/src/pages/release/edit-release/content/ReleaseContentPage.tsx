import EditablePageModeToggle from '@admin/components/editable/EditablePageModeToggle';
import { EditingContextProvider } from '@admin/contexts/EditingContext';
import PublicationReleaseContent from '@admin/modules/find-statistics/PublicationReleaseContent';
import { ReleaseRouteParams } from '@admin/routes/edit-release/routes';
import permissionService from '@admin/services/permissions/permissionService';
import {
  EditableBlock,
  EditableRelease,
  ExtendedComment,
} from '@admin/services/publicationService';
import { releaseContentService } from '@admin/services/release/edit-release/content/service';
import LoadingSpinner from '@common/components/LoadingSpinner';
import WarningMessage from '@common/components/WarningMessage';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import { ContentSection } from '@common/services/publicationService';
import classNames from 'classnames';
import React from 'react';
import { RouteComponentProps } from 'react-router';
import {
  ReleaseContextState,
  ReleaseProvider,
  useReleaseState,
} from './ReleaseContext';

const ReleaseContentPageLoaded = () => {
  const { canUpdateRelease, unresolvedComments } = useReleaseState();

  return (
    <EditingContextProvider
      value={{
        isEditing: canUpdateRelease,
      }}
    >
      {({ isEditing }) => (
        <>
          {canUpdateRelease && (
            <div className="govuk-form-group">
              {unresolvedComments.length > 0 && (
                <WarningMessage>
                  There are {unresolvedComments.length} unresolved comments
                </WarningMessage>
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
              <PublicationReleaseContent />
            </div>
          </div>
        </>
      )}
    </EditingContextProvider>
  );
};

const contentSectionComments = (
  contentSection: ContentSection<EditableBlock>,
): ExtendedComment[] => {
  if (contentSection.content?.length) {
    return contentSection.content.reduce<ExtendedComment[]>(
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
      .reduce<ExtendedComment[]>(
        (allComments, contentSection) => [
          ...allComments,
          ...contentSectionComments(contentSection),
        ],
        [],
      ),
  ].filter(comment => comment !== undefined && comment.state === 'open');

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
        <ReleaseProvider value={value}>
          <ReleaseContentPageLoaded />
        </ReleaseProvider>
      ) : (
        <p>There was a problem loading the release content</p>
      )}
    </LoadingSpinner>
  );
};

export default ReleaseContentPage;
