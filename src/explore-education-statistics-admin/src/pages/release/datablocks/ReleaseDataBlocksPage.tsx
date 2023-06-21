import ButtonLink from '@admin/components/ButtonLink';
import Link from '@admin/components/Link';
import DataBlockDeletePlanModal from '@admin/pages/release/datablocks/components/DataBlockDeletePlanModal';
import {
  releaseDataBlockCreateRoute,
  releaseDataBlockEditRoute,
  ReleaseDataBlockRouteParams,
  ReleaseRouteParams,
  releaseTableToolRoute,
} from '@admin/routes/releaseRoutes';
import dataBlocksService, {
  ReleaseDataBlockSummary,
} from '@admin/services/dataBlockService';
import permissionService from '@admin/services/permissionService';
import ButtonText from '@common/components/ButtonText';
import FormattedDate from '@common/components/FormattedDate';
import InsetText from '@common/components/InsetText';
import LoadingSpinner from '@common/components/LoadingSpinner';
import WarningMessage from '@common/components/WarningMessage';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import classNames from 'classnames';
import React, { useCallback, useState } from 'react';
import { generatePath, useParams } from 'react-router';

interface Model {
  dataBlocks: ReleaseDataBlockSummary[];
  canUpdateRelease: boolean;
}

const ReleaseDataBlocksPage = () => {
  const { publicationId, releaseId } = useParams<ReleaseRouteParams>();

  const [deleteDataBlock, setDeleteDataBlock] =
    useState<ReleaseDataBlockSummary>();

  const {
    value: model,
    isLoading,
    setState: setModel,
  } = useAsyncHandledRetry<Model>(async () => {
    const [dataBlocks, canUpdateRelease] = await Promise.all([
      dataBlocksService.listDataBlocks(releaseId),
      permissionService.canUpdateRelease(releaseId),
    ]);

    return {
      dataBlocks,
      canUpdateRelease,
    };
  }, [releaseId]);

  const handleDelete = useCallback(async () => {
    if (!deleteDataBlock || !model) {
      return;
    }

    setModel({
      value: {
        ...model,
        dataBlocks: model.dataBlocks.filter(
          dataBlock => dataBlock.id !== deleteDataBlock.id,
        ),
      },
    });

    setDeleteDataBlock(undefined);
  }, [model, deleteDataBlock, setModel]);

  const handleDeleteCancel = useCallback(() => {
    setDeleteDataBlock(undefined);
  }, []);

  const createPath = generatePath<ReleaseRouteParams>(
    releaseDataBlockCreateRoute.path,
    {
      publicationId,
      releaseId,
    },
  );

  if (isLoading || !model) {
    return <LoadingSpinner />;
  }

  const { canUpdateRelease, dataBlocks } = model;

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

      {!canUpdateRelease && !isLoading && (
        <>
          <WarningMessage>
            This release has been approved, and can no longer be updated.
          </WarningMessage>

          <ButtonLink
            to={generatePath<ReleaseRouteParams>(releaseTableToolRoute.path, {
              publicationId,
              releaseId,
            })}
          >
            Go to table tool
          </ButtonLink>
        </>
      )}

      {dataBlocks.length > 0 ? (
        <>
          {canUpdateRelease && dataBlocks.length > 5 && (
            <ButtonLink to={createPath}>Create data block</ButtonLink>
          )}

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
                  Featured table name
                </th>
                <th scope="col">Created date</th>
                <th scope="col" className="govuk-table__header--actions">
                  Actions
                </th>
              </tr>
            </thead>
            <tbody>
              {dataBlocks.map(dataBlock => (
                <tr key={dataBlock.id}>
                  <td>{dataBlock.name}</td>
                  <td>{dataBlock.chartsCount > 0 ? 'Yes' : 'No'}</td>
                  <td>{dataBlock.inContent ? 'Yes' : 'No'}</td>
                  <td>{dataBlock.highlightName || 'None'}</td>
                  <td>
                    {dataBlock.created ? (
                      <FormattedDate format="d MMMM yyyy HH:mm">
                        {dataBlock.created}
                      </FormattedDate>
                    ) : (
                      'Not available'
                    )}
                  </td>
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
                      {canUpdateRelease ? 'Edit block' : 'View block'}
                    </Link>
                    {canUpdateRelease && (
                      <ButtonText onClick={() => setDeleteDataBlock(dataBlock)}>
                        Delete block
                      </ButtonText>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </>
      ) : (
        <InsetText>No data blocks have been created.</InsetText>
      )}

      {canUpdateRelease && (
        <ButtonLink to={createPath}>Create data block</ButtonLink>
      )}

      {deleteDataBlock && (
        <DataBlockDeletePlanModal
          releaseId={releaseId}
          dataBlockId={deleteDataBlock.id}
          onConfirm={handleDelete}
          onCancel={handleDeleteCancel}
          onExit={handleDeleteCancel}
        />
      )}
    </>
  );
};

export default ReleaseDataBlocksPage;
