import Link from '@admin/components/Link';
import {
  ReleaseDataBlockRouteParams,
  releaseDataBlocksRoute,
  releaseDataFileReplaceRoute,
  ReleaseDataFileReplaceRouteParams,
  ReleaseFootnoteRouteParams,
  releaseFootnotesRoute,
} from '@admin/routes/releaseRoutes';
import dataReplacementService from '@admin/services/dataReplacementService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import WarningMessage from '@common/components/WarningMessage';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import { generatePath, useParams } from 'react-router';
import React from 'react';

const ReleaseDataFileReplacementCompletePage = () => {
  const { fileId, publicationId, releaseId } =
    useParams<ReleaseDataFileReplaceRouteParams>();

  // Run the replacement plan against itself so we can just get the
  // data blocks and footnotes in a convenient way.
  const { value: plan, isLoading } = useAsyncRetry(
    () => dataReplacementService.getReplacementPlan(releaseId, fileId, fileId),
    [releaseId, fileId],
  );

  const dataFilePath = generatePath<ReleaseDataFileReplaceRouteParams>(
    releaseDataFileReplaceRoute.path,
    {
      publicationId,
      releaseId,
      fileId,
    },
  );

  return (
    <>
      <Link back className="govuk-!-margin-bottom-6" to={dataFilePath}>
        Back
      </Link>

      <h2>Data replacement complete</h2>

      <WarningMessage>
        Your data replacement is now complete. Please make sure you review your
        data blocks and footnotes to ensure that these all remain valid before
        requesting approval of your release.
      </WarningMessage>

      <LoadingSpinner loading={isLoading}>
        {plan && (
          <>
            <h3>Data blocks</h3>

            {plan.dataBlocks.length > 0 ? (
              <>
                <p>Please check the following data blocks:</p>

                <ul>
                  {plan.dataBlocks.map(dataBlock => (
                    <li key={dataBlock.id}>
                      <Link
                        unvisited
                        to={generatePath<ReleaseDataBlockRouteParams>(
                          releaseDataBlocksRoute.path,
                          {
                            publicationId,
                            releaseId,
                            dataBlockId: dataBlock.id,
                          },
                        )}
                      >
                        {dataBlock.name}
                      </Link>
                    </li>
                  ))}
                </ul>
              </>
            ) : (
              <p>There are no data blocks to check.</p>
            )}

            <h3>Footnotes</h3>

            {plan.footnotes.length > 0 ? (
              <>
                <p>Please check the following footnotes:</p>

                <ul className="govuk-list">
                  {plan.footnotes.map(footnote => (
                    <li key={footnote.id}>
                      <Link
                        unvisited
                        to={generatePath<ReleaseFootnoteRouteParams>(
                          releaseFootnotesRoute.path,
                          {
                            publicationId,
                            releaseId,
                            footnoteId: footnote.id,
                          },
                        )}
                      >
                        {footnote.content}
                      </Link>
                    </li>
                  ))}
                </ul>
              </>
            ) : (
              <p>There are no footnotes to check.</p>
            )}
          </>
        )}
      </LoadingSpinner>

      <Link to={dataFilePath} unvisited>
        Back to data file
      </Link>
    </>
  );
};

export default ReleaseDataFileReplacementCompletePage;
