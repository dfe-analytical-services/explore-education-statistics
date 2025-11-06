import Link from '@admin/components/Link';
import PageMetaTitle from '@admin/components/PageMetaTitle';
import FootnoteForm from '@admin/pages/release/footnotes/components/FootnoteForm';
import {
  ReleaseFootnoteRouteParams,
  releaseFootnotesRoute,
  ReleaseRouteParams,
} from '@admin/routes/releaseRoutes';
import footnoteService from '@admin/services/footnoteService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import React from 'react';
import { generatePath, RouteComponentProps } from 'react-router';

const ReleaseFootnoteEditPage = ({
  match,
  history,
}: RouteComponentProps<ReleaseFootnoteRouteParams>) => {
  const { publicationId, releaseVersionId, footnoteId } = match.params;

  const { value: footnoteMeta, isLoading: isFootnoteMetaLoading } =
    useAsyncHandledRetry(
      () => footnoteService.getFootnoteMeta(releaseVersionId),
      [releaseVersionId],
    );

  const { value: footnote, isLoading: isFootnoteLoading } =
    useAsyncHandledRetry(
      () => footnoteService.getFootnote(releaseVersionId, footnoteId),
      [releaseVersionId, footnoteId],
    );

  const footnotesPath = generatePath<ReleaseRouteParams>(
    releaseFootnotesRoute.path,
    {
      publicationId,
      releaseVersionId,
    },
  );

  return (
    <>
      <PageMetaTitle title="Edit footnote" />
      <Link to={footnotesPath} back className="govuk-!-margin-bottom-6">
        Back
      </Link>

      <LoadingSpinner loading={isFootnoteMetaLoading || isFootnoteLoading}>
        <h2>Edit footnote</h2>

        {footnoteMeta && footnote && (
          <FootnoteForm
            footnote={footnote}
            footnoteMeta={footnoteMeta}
            onSubmit={async values => {
              await footnoteService.updateFootnote(
                releaseVersionId,
                footnoteId,
                values,
              );

              history.push(footnotesPath);
            }}
            cancelButton={
              <Link unvisited to={footnotesPath}>
                Cancel
              </Link>
            }
          />
        )}
      </LoadingSpinner>
    </>
  );
};

export default ReleaseFootnoteEditPage;
