import Link from '@admin/components/Link';
import PageMetaTitle from '@admin/components/PageMetaTitle';
import FootnoteForm from '@admin/pages/release/footnotes/components/FootnoteForm';
import {
  releaseFootnotesRoute,
  ReleaseRouteParams,
} from '@admin/routes/releaseRoutes';
import footnoteService from '@admin/services/footnoteService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import React from 'react';
import { generatePath, useNavigate } from 'react-router';
import { useParams } from 'react-router-dom';

const ReleaseFootnoteCreatePage = () => {
  const navigate = useNavigate();
  const { publicationId, releaseVersionId } =
    useParams<ReleaseRouteParams>() as ReleaseRouteParams;

  const { value: footnoteMeta, isLoading } = useAsyncHandledRetry(
    () => footnoteService.getFootnoteMeta(releaseVersionId),
    [releaseVersionId],
  );

  const footnotesPath = generatePath(releaseFootnotesRoute.fullPath, {
    publicationId,
    releaseVersionId,
  });

  return (
    <>
      <PageMetaTitle title="Create footnote" />
      <Link to={footnotesPath} back className="govuk-!-margin-bottom-6">
        Back
      </Link>

      <LoadingSpinner loading={isLoading}>
        <h2>Create footnote</h2>

        {footnoteMeta && (
          <FootnoteForm
            footnoteMeta={footnoteMeta}
            onSubmit={async values => {
              await footnoteService.createFootnote(releaseVersionId, values);
              navigate(footnotesPath);
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

export default ReleaseFootnoteCreatePage;
