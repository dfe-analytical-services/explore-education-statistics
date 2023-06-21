import Button from '@common/components/Button';
import ButtonLink from '@admin/components/ButtonLink';
import Link from '@admin/components/Link';
import FootnotesList from '@admin/pages/release/footnotes/components/FootnotesList';
import {
  releaseDataRoute,
  releaseFootnotesCreateRoute,
  ReleaseRouteParams,
} from '@admin/routes/releaseRoutes';
import footnoteService, { Footnote } from '@admin/services/footnoteService';
import permissionService from '@admin/services/permissionService';
import InsetText from '@common/components/InsetText';
import LoadingSpinner from '@common/components/LoadingSpinner';
import WarningMessage from '@common/components/WarningMessage';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import useToggle from '@common/hooks/useToggle';
import classNames from 'classnames';
import { generatePath, useParams } from 'react-router';
import React from 'react';

const ReleaseFootnotesPage = () => {
  const { publicationId, releaseId } = useParams<ReleaseRouteParams>();

  const [isReordering, toggleIsReordering] = useToggle(false);

  const { value: canUpdateRelease = false, isLoading: isPermissionLoading } =
    useAsyncHandledRetry(
      () => permissionService.canUpdateRelease(releaseId),
      [releaseId],
    );

  const { value: footnoteMeta, isLoading: isFootnoteMetaLoading } =
    useAsyncHandledRetry(
      () => footnoteService.getFootnoteMeta(releaseId),
      [releaseId],
    );

  const {
    value: footnotes,
    setState: setFootnotes,
    isLoading: isFootnotesLoading,
  } = useAsyncHandledRetry(
    () => footnoteService.getFootnotes(releaseId),
    [releaseId],
  );

  const handleReorder = (reorderedFootnotes: Footnote[]) => {
    setFootnotes({
      value: reorderedFootnotes,
    });
  };

  const handleSaveOrder = async () => {
    await footnoteService.updateFootnotesOrder(
      releaseId,
      footnotes?.map(footnote => footnote.id) ?? [],
    );
  };

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
          <div
            className={classNames('dfe-flex', {
              'dfe-justify-content--space-between': !isReordering,
              'dfe-justify-content--flex-end': isReordering,
            })}
          >
            {!isReordering && (
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

            {footnotes.length > 1 && (
              <Button
                className="govuk-!-margin-left-auto"
                variant={!isReordering ? 'secondary' : undefined}
                onClick={async () => {
                  if (isReordering) {
                    await handleSaveOrder();
                  }
                  toggleIsReordering();
                }}
              >
                {isReordering ? 'Save order' : 'Reorder footnotes'}
              </Button>
            )}
          </div>
        )}

        <FootnotesList
          publicationId={publicationId}
          releaseId={releaseId}
          footnotes={footnotes}
          footnoteMeta={footnoteMeta}
          canUpdateRelease={canUpdateRelease}
          isReordering={isReordering}
          onDelete={async footnote => {
            await footnoteService.deleteFootnote(releaseId, footnote.id);
            setFootnotes({
              value: footnotes?.filter(f => f.id !== footnote.id),
            });
          }}
          onReorder={handleReorder}
        />
      </>
    );
  };

  return (
    <LoadingSpinner loading={isPermissionLoading}>
      <h2>Footnotes</h2>
      <InsetText>
        <h3>Before you start</h3>
        <p>
          A footnote should outline any necessary caveats within your data.
          These should be used sparingly, and only for information that is
          critical to understanding the data in the table or chart it refers to.
        </p>
      </InsetText>
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
