import ButtonLink from '@admin/components/ButtonLink';
import Link from '@admin/components/Link';
import FootnotesList from '@admin/pages/release/footnotes/components/FootnotesList';
import {
  releaseDataRoute,
  releaseFootnotesCreateRoute,
  ReleaseRouteParams,
} from '@admin/routes/releaseRoutes';
import footnotesService from '@admin/services/footnoteService';
import permissionService from '@admin/services/permissionService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import WarningMessage from '@common/components/WarningMessage';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import React from 'react';
import { generatePath, RouteComponentProps } from 'react-router';

const ReleaseFootnotesPage = ({
  match,
}: RouteComponentProps<ReleaseRouteParams>) => {
  const { publicationId, releaseId } = match.params;

  const {
    value: canUpdateRelease = false,
    isLoading: isPermissionLoading,
  } = useAsyncHandledRetry(
    () => permissionService.canUpdateRelease(releaseId),
    [releaseId],
  );

  const {
    value: footnoteMeta,
    isLoading: isFootnoteMetaLoading,
  } = useAsyncHandledRetry(() => footnotesService.getFootnoteMeta(releaseId), [
    releaseId,
  ]);

  const {
    value: footnotes,
    setState: setFootnotes,
    isLoading: isFootnotesLoading,
  } = useAsyncHandledRetry(() => footnotesService.getFootnotes(releaseId), [
    releaseId,
  ]);

  const renderInner = () => {
    if (!footnotes || !footnoteMeta) {
      return null;
    }

    if (Object.keys(footnoteMeta.subjects).length === 0 && canUpdateRelease) {
      return (
        <WarningMessage>
          Before footnotes can be created, relevant data files need to be
          uploaded. That can be done in the{' '}
          <Link
            to={generatePath<ReleaseRouteParams>(releaseDataRoute.path, {
              publicationId,
              releaseId,
            })}
          >
            Data and files
          </Link>{' '}
          section.
        </WarningMessage>
      );
    }

    return (
      <>
        {canUpdateRelease && (
          <ButtonLink
            to={generatePath<ReleaseRouteParams>(
              releaseFootnotesCreateRoute.path,
              {
                publicationId,
                releaseId,
              },
            )}
          >
            Create footnote
          </ButtonLink>
        )}

        <FootnotesList
          publicationId={publicationId}
          releaseId={releaseId}
          footnotes={footnotes}
          footnoteMeta={footnoteMeta}
          canUpdateRelease={canUpdateRelease}
          onDelete={async footnote => {
            await footnotesService.deleteFootnote(releaseId, footnote.id);

            setFootnotes({
              value: footnotes?.filter(f => f.id !== footnote.id),
            });
          }}
        />
      </>
    );
  };

  return (
    <LoadingSpinner loading={isPermissionLoading}>
      <h2>Footnotes</h2>

      {!canUpdateRelease && (
        <p>This release has been approved, and can no longer be updated.</p>
      )}

      <LoadingSpinner loading={isFootnotesLoading || isFootnoteMetaLoading}>
        {renderInner()}
      </LoadingSpinner>
    </LoadingSpinner>
  );
};

export default ReleaseFootnotesPage;
