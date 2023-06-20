import ButtonLink from '@admin/components/ButtonLink';
import Link from '@admin/components/Link';
import DataBlockDeletePlanModal from '@admin/pages/release/datablocks/components/DataBlockDeletePlanModal';
import FeaturedTablesTable from '@admin/pages/release/datablocks/components/FeaturedTablesTable';
import dataBlockQueries from '@admin/queries/dataBlockQueries';
import permissionQueries from '@admin/queries/permissionQueries';
import featuredTableQueries from '@admin/queries/featuredTableQueries';
import {
  releaseDataBlockCreateRoute,
  releaseDataBlockEditRoute,
  ReleaseDataBlockRouteParams,
  ReleaseRouteParams,
  releaseTableToolRoute,
} from '@admin/routes/releaseRoutes';
import { ReleaseDataBlockSummary } from '@admin/services/dataBlockService';
import featuredTableService, {
  FeaturedTable,
} from '@admin/services/featuredTableService';
import ButtonText from '@common/components/ButtonText';
import FormattedDate from '@common/components/FormattedDate';
import InsetText from '@common/components/InsetText';
import LoadingSpinner from '@common/components/LoadingSpinner';
import WarningMessage from '@common/components/WarningMessage';
import React, { useCallback, useMemo, useState } from 'react';
import { generatePath, RouteComponentProps } from 'react-router';
import { useQuery, useQueryClient } from '@tanstack/react-query';

const ReleaseDataBlocksPage = ({
  match,
}: RouteComponentProps<ReleaseRouteParams>) => {
  const { publicationId, releaseId } = match.params;

  const [deleteDataBlock, setDeleteDataBlock] = useState<
    ReleaseDataBlockSummary
  >();

  const queryClient = useQueryClient();

  const listFeaturedTablesQuery = useMemo(
    () => featuredTableQueries.list(releaseId),
    [releaseId],
  );
  const listDataBlocksQuery = useMemo(() => dataBlockQueries.list(releaseId), [
    releaseId,
  ]);

  const { data: dataBlocks = [], isLoading: isLoadingDataBlocks } = useQuery(
    listDataBlocksQuery,
  );
  const {
    data: featuredTables = [],
    isLoading: isLoadingFeaturedTables,
  } = useQuery(listFeaturedTablesQuery);
  const {
    data: canUpdateRelease = true,
    isLoading: isLoadingPermissions,
  } = useQuery(permissionQueries.canUpdateRelease(releaseId));

  const handleDelete = useCallback(async () => {
    if (!deleteDataBlock) {
      return;
    }

    queryClient.setQueryData(
      listDataBlocksQuery.queryKey,
      dataBlocks.filter(dataBlock => dataBlock.id !== deleteDataBlock.id),
    );

    queryClient.setQueryData(
      listFeaturedTablesQuery.queryKey,
      featuredTables.filter(table => table.dataBlockId !== deleteDataBlock.id),
    );
    await queryClient.invalidateQueries([
      listDataBlocksQuery.queryKey,
      listFeaturedTablesQuery.queryKey,
    ]);

    setDeleteDataBlock(undefined);
  }, [
    dataBlocks,
    deleteDataBlock,
    featuredTables,
    listDataBlocksQuery.queryKey,
    listFeaturedTablesQuery.queryKey,
    queryClient,
  ]);

  const handleDeleteCancel = useCallback(() => {
    setDeleteDataBlock(undefined);
  }, []);

  const handleSaveOrder = useCallback(
    async (reorderedTables: FeaturedTable[]) => {
      await featuredTableService.reorderFeaturedTables(
        releaseId,
        reorderedTables?.map(table => table.id) ?? [],
      );
      await queryClient.invalidateQueries(listFeaturedTablesQuery.queryKey);
    },
    [listFeaturedTablesQuery, queryClient, releaseId],
  );

  const createPath = generatePath<ReleaseRouteParams>(
    releaseDataBlockCreateRoute.path,
    {
      publicationId,
      releaseId,
    },
  );

  if (isLoadingDataBlocks || isLoadingFeaturedTables || isLoadingPermissions) {
    return <LoadingSpinner />;
  }

  const filteredDataBlocks = dataBlocks.filter(dataBlock => {
    return !featuredTables.find(table => table.dataBlockId === dataBlock.id);
  });

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
        <p>
          Featured tables are data blocks that are also available when the
          publication is selected via the table builder.
        </p>
      </InsetText>

      {canUpdateRelease && filteredDataBlocks.length > 5 && (
        <ButtonLink to={createPath}>Create data block</ButtonLink>
      )}

      {!canUpdateRelease && (
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

      {!dataBlocks.length && (
        <InsetText>No data blocks have been created.</InsetText>
      )}

      {featuredTables.length > 0 && (
        <FeaturedTablesTable
          canUpdateRelease={canUpdateRelease}
          dataBlocks={dataBlocks}
          featuredTables={featuredTables}
          publicationId={publicationId}
          releaseId={releaseId}
          onDelete={setDeleteDataBlock}
          onSaveOrder={handleSaveOrder}
        />
      )}

      {filteredDataBlocks.length > 0 && (
        <>
          <h3>Data blocks</h3>

          <table data-testid="dataBlocks">
            <thead>
              <tr>
                <th scope="col" className="govuk-!-width-one-quarter">
                  Name
                </th>
                <th scope="col">Has chart</th>
                <th scope="col">In content</th>
                <th scope="col">Created date</th>
                <th scope="col" className="govuk-table__header--actions">
                  Actions
                </th>
              </tr>
            </thead>
            <tbody>
              {filteredDataBlocks.map(dataBlock => (
                <tr key={dataBlock.id}>
                  <td>{dataBlock.name}</td>
                  <td>{dataBlock.chartsCount > 0 ? 'Yes' : 'No'}</td>
                  <td>{dataBlock.inContent ? 'Yes' : 'No'}</td>
                  <td>
                    {dataBlock.created ? (
                      <FormattedDate format="d MMMM yyyy HH:mm">
                        {dataBlock.created}
                      </FormattedDate>
                    ) : (
                      'Not available'
                    )}
                  </td>
                  <td className="govuk-table__cell--actions govuk-!-width-one-quarter">
                    <Link
                      className="govuk-!-margin-bottom-0"
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
                      <ButtonText
                        className="govuk-!-margin-bottom-0"
                        onClick={() => setDeleteDataBlock(dataBlock)}
                      >
                        Delete block
                      </ButtonText>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </>
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
