import Link from '@admin/components/Link';
import {
  releaseDataBlocksRoute,
  releaseDataFileReplaceRoute,
  ReleaseDataFileReplaceRouteParams,
  releaseFootnotesRoute,
} from '@admin/routes/releaseRoutes';
import { DataReplacementResult } from '@admin/pages/release/data/components/DataFileReplacementPlan';
import {
  getManageIndicatorsPath,
  ReplacementPairedIndicator,
} from '@admin/pages/release/data/utils/replacementPairedIndicators';
import LoadingSpinner from '@common/components/LoadingSpinner';
import WarningMessage from '@common/components/WarningMessage';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import sanitizeHtml from '@common/utils/sanitizeHtml';
import partition from 'lodash/partition';
import React from 'react';
import { generatePath, useLocation, useParams } from 'react-router';
import releaseDataFileService from '@admin/services/releaseDataFileService';

const ReleaseDataFileReplacementCompletePage = () => {
  const { publicationId, releaseVersionId, fileId } =
    useParams<ReleaseDataFileReplaceRouteParams>() as ReleaseDataFileReplaceRouteParams;

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

  // Only set when arriving straight from confirming the replacement
  const replacementResult = useLocation().state as
    DataReplacementResult | undefined;

  const [maintainedPairs, droppedPairs] = partition(
    replacementResult?.pairedIndicators ?? [],
    pair => pair.maintained,
  );

  const dataFilePath = generatePath(releaseDataFileReplaceRoute.fullPath, {
    publicationId,
    releaseVersionId,
    fileId,
  });

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
                        to={generatePath(releaseDataBlocksRoute.fullPath, {
                          publicationId,
                          releaseVersionId,
                          dataBlockVersionId: dataBlock.id,
                        })}
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
                        to={generatePath(releaseFootnotesRoute.fullPath, {
                          publicationId,
                          releaseVersionId,
                          footnoteId: footnote.id,
                        })}
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

            <h3>Paired indicators</h3>

            {replacementResult ? (
              <>
                {replacementResult.pairedIndicators.length === 0 && (
                  <p>There are no paired indicators to check.</p>
                )}

                {maintainedPairs.length > 0 && (
                  <>
                    <p>The following paired indicators were maintained:</p>
                    <PairedIndicatorsList
                      pairs={maintainedPairs}
                      testId="maintained-paired-indicators"
                    />
                  </>
                )}

                {droppedPairs.length > 0 && (
                  <>
                    <p>
                      The following paired indicators were dropped because their
                      parent or child indicator was not mapped. Their indicators
                      can now be selected independently in the table tool:
                    </p>
                    <PairedIndicatorsList
                      pairs={droppedPairs}
                      testId="dropped-paired-indicators"
                    />
                  </>
                )}
              </>
            ) : (
              <p>Please check the paired indicators for this data set.</p>
            )}

            <p>
              <Link
                unvisited
                to={getManageIndicatorsPath({
                  publicationId,
                  releaseVersionId,
                  subjectId: replacementResult?.replacementSubjectId,
                })}
              >
                Manage paired indicators
              </Link>
            </p>
          </>
        )}
      </LoadingSpinner>

      <Link to={dataFilePath} unvisited>
        Back to data file
      </Link>
    </>
  );
};

function PairedIndicatorsList({
  pairs,
  testId,
}: {
  pairs: ReplacementPairedIndicator[];
  testId: string;
}) {
  return (
    <ul className="govuk-list govuk-list--bullet" data-testid={testId}>
      {pairs.map(pair => (
        <li key={`${pair.parentIndicatorId}-${pair.childIndicatorId}`}>
          {`${pair.parentLabel}, paired with ${pair.childLabel}`}
        </li>
      ))}
    </ul>
  );
}

export default ReleaseDataFileReplacementCompletePage;
