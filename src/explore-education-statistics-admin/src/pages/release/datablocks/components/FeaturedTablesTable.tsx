import Link from '@admin/components/Link';
import { FeaturedTable } from '@admin/services/featuredTableService';
import { ReleaseDataBlockSummary } from '@admin/services/dataBlockService';
import {
  releaseDataBlockEditRoute,
  ReleaseDataBlockRouteParams,
} from '@admin/routes/releaseRoutes';
import Button from '@common/components/Button';
import ButtonText from '@common/components/ButtonText';
import FormattedDate from '@common/components/FormattedDate';
import ReorderableList from '@common/components/ReorderableList';
import { ReorderableListItem } from '@common/components/ReorderableItem';
import reorder from '@common/utils/reorder';
import useToggle from '@common/hooks/useToggle';
import React, { useEffect, useState } from 'react';
import { generatePath } from 'react-router';
import orderBy from 'lodash/orderBy';

interface Props {
  canUpdateRelease: boolean;
  dataBlocks: ReleaseDataBlockSummary[];
  featuredTables: FeaturedTable[];
  publicationId: string;
  releaseVersionId: string;
  onDelete: (dataBlock: ReleaseDataBlockSummary) => void;
  onSaveOrder: (reorderedTables: FeaturedTable[]) => Promise<void>;
}

export default function FeaturedTablesTable({
  canUpdateRelease,
  dataBlocks,
  featuredTables,
  publicationId,
  releaseVersionId,
  onDelete,
  onSaveOrder,
}: Props) {
  const [isReordering, toggleIsReordering] = useToggle(false);
  const [currentFeaturedTables, setCurrentFeaturedTables] =
    useState(featuredTables);

  useEffect(() => {
    setCurrentFeaturedTables(orderBy(featuredTables, 'order'));
  }, [featuredTables]);

  if (isReordering) {
    return (
      <ReorderableList
        heading="Reorder featured tables"
        id="featured-tables"
        list={currentFeaturedTables.reduce<ReorderableListItem[]>(
          (acc, featuredTable) => {
            const dataBlock = dataBlocks.find(
              block => block.id === featuredTable.dataBlockId,
            );
            if (dataBlock) {
              acc.push({
                id: featuredTable.id,
                label: `${dataBlock.name} (${featuredTable.name})`,
              });
            }
            return acc;
          },
          [],
        )}
        onConfirm={async () => {
          await onSaveOrder(currentFeaturedTables);
          toggleIsReordering.off();
        }}
        onMoveItem={({ prevIndex, nextIndex }) => {
          const reordered = reorder(
            currentFeaturedTables,
            prevIndex,
            nextIndex,
          ).map((table, index) => ({
            ...table,
            order: index,
          }));
          setCurrentFeaturedTables(reordered);
        }}
        onReverse={() => {
          setCurrentFeaturedTables(currentFeaturedTables.toReversed());
        }}
      />
    );
  }

  return (
    <>
      <div className="dfe-flex dfe-justify-content--space-between">
        <h3>Featured tables</h3>
        {canUpdateRelease && currentFeaturedTables.length > 1 && (
          <Button
            className="govuk-!-margin-bottom-4"
            variant="secondary"
            onClick={toggleIsReordering.on}
          >
            Reorder featured tables
          </Button>
        )}
      </div>

      <table data-testid="featuredTables">
        <thead>
          <tr>
            <th scope="col" className="govuk-!-width-one-quarter">
              Data block name
            </th>
            <th scope="col">Has chart</th>
            <th scope="col">In content</th>
            <th scope="col">Featured table name</th>
            <th scope="col">Created date</th>
            <th scope="col" className="govuk-table__header--actions">
              Actions
            </th>
          </tr>
        </thead>
        <tbody>
          {currentFeaturedTables.map(featuredTable => {
            const dataBlock = dataBlocks.find(
              block => block.id === featuredTable.dataBlockId,
            );
            return dataBlock ? (
              <FeaturedTablesRow
                canUpdateRelease={canUpdateRelease}
                dataBlock={dataBlock}
                featuredTable={featuredTable}
                key={featuredTable.id}
                link={generatePath<ReleaseDataBlockRouteParams>(
                  releaseDataBlockEditRoute.path,
                  {
                    publicationId,
                    releaseVersionId,
                    dataBlockId: featuredTable.dataBlockId,
                  },
                )}
                onDelete={onDelete}
              />
            ) : null;
          })}
        </tbody>
      </table>
    </>
  );
}

interface FeaturedTablesRowProps {
  canUpdateRelease: boolean;
  dataBlock: ReleaseDataBlockSummary;
  featuredTable: FeaturedTable;
  link: string;
  onDelete: (dataBlock: ReleaseDataBlockSummary) => void;
}

function FeaturedTablesRow({
  canUpdateRelease,
  dataBlock,
  featuredTable,
  link,
  onDelete,
}: FeaturedTablesRowProps) {
  return (
    <tr>
      <td>{dataBlock.name}</td>
      <td>{dataBlock.chartsCount > 0 ? 'Yes' : 'No'}</td>
      <td>{dataBlock.inContent ? 'Yes' : 'No'}</td>
      <td>{featuredTable.name}</td>
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
          to={link}
          data-testid={`Edit data block ${dataBlock.name}`}
        >
          {canUpdateRelease ? 'Edit block' : 'View block'}
        </Link>
        {canUpdateRelease && (
          <ButtonText
            className="govuk-!-margin-bottom-0"
            onClick={() => onDelete(dataBlock)}
          >
            Delete block
          </ButtonText>
        )}
      </td>
    </tr>
  );
}
