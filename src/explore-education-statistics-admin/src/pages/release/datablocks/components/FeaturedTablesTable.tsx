import Link from '@admin/components/Link';
import DroppableArea from '@admin/components/DroppableArea';
import DraggableItem, { DragHandle } from '@admin/components/DraggableItem';
import { FeaturedTable } from '@admin/services/featuredTableService';
import { ReleaseDataBlockSummary } from '@admin/services/dataBlockService';
import {
  releaseDataBlockEditRoute,
  ReleaseDataBlockRouteParams,
} from '@admin/routes/releaseRoutes';
import Button from '@common/components/Button';
import ButtonText from '@common/components/ButtonText';
import FormattedDate from '@common/components/FormattedDate';
import reorder from '@common/utils/reorder';
import useToggle from '@common/hooks/useToggle';
import React, { useEffect, useState } from 'react';
import { generatePath } from 'react-router';
import orderBy from 'lodash/orderBy';
import { DragDropContext, Droppable } from 'react-beautiful-dnd';

interface Props {
  canUpdateRelease: boolean;
  dataBlocks: ReleaseDataBlockSummary[];
  featuredTables: FeaturedTable[];
  publicationId: string;
  releaseId: string;
  onDelete: (dataBlock: ReleaseDataBlockSummary) => void;
  onSaveOrder: (reorderedTables: FeaturedTable[]) => Promise<void>;
}

export default function FeaturedTablesTable({
  canUpdateRelease,
  dataBlocks,
  featuredTables,
  publicationId,
  releaseId,
  onDelete,
  onSaveOrder,
}: Props) {
  const [isReordering, toggleIsReordering] = useToggle(false);
  const [currentFeaturedTables, setCurrentFeaturedTables] = useState(
    featuredTables,
  );

  useEffect(() => {
    setCurrentFeaturedTables(featuredTables);
  }, [featuredTables]);

  return (
    <>
      <div className="dfe-flex dfe-justify-content--space-between">
        <h3>Featured tables</h3>
        {canUpdateRelease && currentFeaturedTables.length > 1 && (
          <Button
            className="govuk-!-margin-bottom-4"
            variant="secondary"
            onClick={async () => {
              if (isReordering) {
                await onSaveOrder(currentFeaturedTables);
              }
              toggleIsReordering();
            }}
          >
            {isReordering ? 'Save order' : 'Reorder featured tables'}
          </Button>
        )}
      </div>

      <DragDropContext
        onDragEnd={result => {
          if (!result.destination) {
            return;
          }
          const reordered = reorder(
            currentFeaturedTables,
            result.source.index,
            result.destination.index,
          ).map((table, index) => ({
            ...table,
            order: index,
          }));
          setCurrentFeaturedTables(reordered);
        }}
      >
        <Droppable droppableId="footnotes" isDropDisabled={!isReordering}>
          {(droppableProvided, droppableSnapshot) => (
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
              <DroppableArea
                droppableProvided={droppableProvided}
                droppableSnapshot={droppableSnapshot}
                tag="tbody"
              >
                {orderBy(currentFeaturedTables, 'order').map(
                  (featuredTable, index) => {
                    const dataBlock = dataBlocks.find(
                      block => block.id === featuredTable.dataBlockId,
                    );
                    return dataBlock ? (
                      <DraggableItem
                        hideDragHandle
                        id={featuredTable.id}
                        index={index}
                        isReordering={isReordering}
                        key={featuredTable.id}
                        tag="tr"
                      >
                        <FeaturedTablesRow
                          canUpdateRelease={canUpdateRelease}
                          dataBlock={dataBlock}
                          featuredTable={featuredTable}
                          isReordering={isReordering}
                          link={generatePath<ReleaseDataBlockRouteParams>(
                            releaseDataBlockEditRoute.path,
                            {
                              publicationId,
                              releaseId,
                              dataBlockId: featuredTable.dataBlockId,
                            },
                          )}
                          onDelete={onDelete}
                        />
                      </DraggableItem>
                    ) : null;
                  },
                )}
              </DroppableArea>
            </table>
          )}
        </Droppable>
      </DragDropContext>
    </>
  );
}

interface FeaturedTablesRowProps {
  canUpdateRelease: boolean;
  dataBlock: ReleaseDataBlockSummary;
  featuredTable: FeaturedTable;
  isReordering: boolean;
  link: string;
  onDelete: (dataBlock: ReleaseDataBlockSummary) => void;
}

function FeaturedTablesRow({
  canUpdateRelease,
  dataBlock,
  featuredTable,
  isReordering,
  link,
  onDelete,
}: FeaturedTablesRowProps) {
  return (
    <>
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
        {!isReordering ? (
          <>
            <Link className="govuk-!-margin-bottom-0" unvisited to={link}>
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
          </>
        ) : (
          <DragHandle className="govuk-!-margin-right-2" />
        )}
      </td>
    </>
  );
}
