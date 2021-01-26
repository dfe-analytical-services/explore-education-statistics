import Link from '@admin/components/Link';
import DataBlockDeletePlanModal from '@admin/pages/release/datablocks/components/DataBlockDeletePlanModal';
import {
  releaseDataBlockEditRoute,
  ReleaseDataBlockRouteParams,
  ReleaseRouteParams,
} from '@admin/routes/releaseRoutes';
import dataBlocksService, {
  ReleaseDataBlockSummary,
} from '@admin/services/dataBlockService';
import permissionService from '@admin/services/permissionService';
import Button from '@common/components/Button';
import ButtonText from '@common/components/ButtonText';
import InsetText from '@common/components/InsetText';
import LoadingSpinner from '@common/components/LoadingSpinner';
import WarningMessage from '@common/components/WarningMessage';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import classNames from 'classnames';
import React, { useCallback, useState } from 'react';
import { generatePath, RouteComponentProps } from 'react-router';

const ReleaseDataBlocksPage = ({
  match,
}: RouteComponentProps<ReleaseRouteParams>) => {
  const { publicationId, releaseId } = match.params;

  const [deleteDataBlock, setDeleteDataBlock] = useState<
    ReleaseDataBlockSummary
  >();

  const { value: canUpdateRelease } = useAsyncHandledRetry(
    () => permissionService.canUpdateRelease(releaseId),
    [releaseId],
  );

  const {
    value: dataBlocks = [],
    isLoading,
    setState: setDataBlocks,
  } = useAsyncHandledRetry(() => dataBlocksService.listDataBlocks(releaseId), [
    releaseId,
  ]);

  const handleDelete = useCallback(async () => {
    if (!deleteDataBlock) {
      return;
    }

    setDataBlocks({
      value: dataBlocks.filter(
        dataBlock => dataBlock.id !== deleteDataBlock.id,
      ),
    });

    setDeleteDataBlock(undefined);
  }, [dataBlocks, deleteDataBlock, setDataBlocks]);

  const handleDeleteCancel = useCallback(() => {
    setDeleteDataBlock(undefined);
  }, []);

  const hasHighlightNames = dataBlocks.some(
    dataBlock => !!dataBlock.highlightName,
  );

  return (
    <>
      <h2>Data blocks</h2>

      <InsetText>
        <h3>Before you start</h3>
        <p>
          A data block is a smaller cut of data from your original file that you
          can embed into your publication as a presentation table, build charts
          from, or link users directly to.
        </p>
      </InsetText>

      {!canUpdateRelease && (
        <WarningMessage>
          This release has been approved, and can no longer be updated.
        </WarningMessage>
      )}

      <LoadingSpinner loading={isLoading}>
        {dataBlocks.length > 0 ? (
          <>
            {canUpdateRelease && <Button>Create data block</Button>}

            <table>
              <thead>
                <tr>
                  <th scope="col" className="govuk-!-width-one-quarter">
                    Name
                  </th>
                  <th scope="col">Has chart</th>
                  <th scope="col">In content</th>
                  <th
                    scope="col"
                    className={classNames({
                      'govuk-!-width-one-quarter': hasHighlightNames,
                    })}
                  >
                    Highlight name
                  </th>
                  {canUpdateRelease && (
                    <th scope="col" className="govuk-table__header--actions">
                      Actions
                    </th>
                  )}
                </tr>
              </thead>
              <tbody>
                {dataBlocks.map(dataBlock => (
                  <tr key={dataBlock.id}>
                    <td>{dataBlock.name}</td>
                    <td>{dataBlock.chartsCount > 0 ? 'Yes' : 'No'}</td>
                    <td>{dataBlock.contentSectionId ? 'Yes' : 'No'}</td>
                    <td>{dataBlock.highlightName || 'None'}</td>
                    {canUpdateRelease && (
                      <td className="govuk-table__cell--actions">
                        <Link
                          unvisited
                          to={generatePath<ReleaseDataBlockRouteParams>(
                            releaseDataBlockEditRoute.path,
                            {
                              publicationId,
                              releaseId,
                              dataBlockId: dataBlock.id,
                            },
                          )}
                        >
                          Edit block
                        </Link>
                        <ButtonText
                          onClick={() => setDeleteDataBlock(dataBlock)}
                        >
                          Delete block
                        </ButtonText>
                      </td>
                    )}
                  </tr>
                ))}
              </tbody>
            </table>
          </>
        ) : (
          <InsetText>No data blocks have been created.</InsetText>
        )}

        {canUpdateRelease && <Button>Create data block</Button>}

        {deleteDataBlock && (
          <DataBlockDeletePlanModal
            releaseId={releaseId}
            dataBlockId={deleteDataBlock.id}
            onConfirm={handleDelete}
            onCancel={handleDeleteCancel}
            onExit={handleDeleteCancel}
          />
        )}
      </LoadingSpinner>
    </>
  );
};

export default ReleaseDataBlocksPage;
