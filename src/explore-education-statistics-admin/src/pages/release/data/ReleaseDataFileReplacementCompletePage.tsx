import Link from '@admin/components/Link';
import {
  ReleaseDataBlockRouteParams,
  releaseDataBlocksRoute,
  releaseDataFileReplaceRoute,
  ReleaseDataFileReplaceRouteParams,
  ReleaseFootnoteRouteParams,
  releaseFootnotesRoute,
} from '@admin/routes/releaseRoutes';
import LoadingSpinner from '@common/components/LoadingSpinner';
import WarningMessage from '@common/components/WarningMessage';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import sanitizeHtml from '@common/utils/sanitizeHtml';
import React from 'react';
import { generatePath, RouteComponentProps } from 'react-router';
import releaseDataFileService from '@admin/services/releaseDataFileService';

const ReleaseDataFileReplacementCompletePage = ({
  match,
}: RouteComponentProps<ReleaseDataFileReplaceRouteParams>) => {
  const { publicationId, releaseVersionId, fileId } = match.params;

  const {
    value: dataSetAccoutrements,
    isLoading,
    error,
  } = useAsyncRetry(
    () =>
      releaseDataFileService.getDataSetAccoutrementsSummary(
        releaseVersionId,
        fileId,
      ),
    [releaseVersionId, fileId],
  );

  const dataFilePath = generatePath<ReleaseDataFileReplaceRouteParams>(
    releaseDataFileReplaceRoute.path,
    {
      publicationId,
      releaseVersionId,
      fileId,
    },
  );

  if (error) {
    return (
      <>
        <WarningMessage>
          There was a problem with the data replacement.
        </WarningMessage>
        <Link back className="govuk-!-margin-bottom-6" to={dataFilePath}>
          Back
        </Link>
      </>
    );
  }

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
        {dataSetAccoutrements && (
          <>
            <h3>Data blocks</h3>

            {dataSetAccoutrements.dataBlocks.length > 0 ? (
              <>
                <p>Please check the following data blocks:</p>

                <ul>
                  {dataSetAccoutrements.dataBlocks.map(dataBlock => (
                    <li key={dataBlock.id}>
                      <Link
                        unvisited
                        to={generatePath<ReleaseDataBlockRouteParams>(
                          releaseDataBlocksRoute.path,
                          {
                            publicationId,
                            releaseVersionId,
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

            {dataSetAccoutrements.footnotes.length > 0 ? (
              <>
                <p>Please check the following footnotes:</p>

                <ul className="govuk-list">
                  {dataSetAccoutrements.footnotes.map(footnote => (
                    <li key={footnote.id}>
                      <Link
                        unvisited
                        to={generatePath<ReleaseFootnoteRouteParams>(
                          releaseFootnotesRoute.path,
                          {
                            publicationId,
                            releaseVersionId,
                            footnoteId: footnote.id,
                          },
                        )}
                      >
                        {sanitizeHtml(footnote.content, {
                          allowedTags: [],
                        })}
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
