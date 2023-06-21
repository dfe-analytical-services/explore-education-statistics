import Link from '@admin/components/Link';
import FootnoteForm from '@admin/pages/release/footnotes/components/FootnoteForm';
import {
  releaseFootnotesRoute,
  ReleaseRouteParams,
} from '@admin/routes/releaseRoutes';
import footnoteService from '@admin/services/footnoteService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import { generatePath, useHistory, useParams } from 'react-router';
import React from 'react';

const ReleaseFootnoteCreatePage = () => {
  const { publicationId, releaseId } = useParams<ReleaseRouteParams>();
  const history = useHistory();

  const { value: footnoteMeta, isLoading } = useAsyncHandledRetry(
    () => footnoteService.getFootnoteMeta(releaseId),
    [releaseId],
  );

  const footnotesPath = generatePath<ReleaseRouteParams>(
    releaseFootnotesRoute.path,
    {
      publicationId,
      releaseId,
    },
  );

  return (
    <>
      <Link to={footnotesPath} back className="govuk-!-margin-bottom-6">
        Back
      </Link>

      <LoadingSpinner loading={isLoading}>
        <h2>Create footnote</h2>

        {footnoteMeta && (
          <FootnoteForm
            footnoteMeta={footnoteMeta}
            onSubmit={async values => {
              await footnoteService.createFootnote(releaseId, values);
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

export default ReleaseFootnoteCreatePage;
