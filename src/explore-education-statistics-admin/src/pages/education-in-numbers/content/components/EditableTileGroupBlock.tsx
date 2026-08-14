import EditableBlockWrapper from '@admin/components/editable/EditableBlockWrapper';
import EditableTile from '@admin/pages/education-in-numbers/content/components/EditableTile';
import { useEducationInNumbersPageContentState } from '@admin/pages/education-in-numbers/content/context/EducationInNumbersPageContentContext';
import useEducationInNumbersPageContentActions from '@admin/pages/education-in-numbers/content/context/useEducationInNumbersPageContentActions';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import { FormTextInput } from '@common/components/form';
import InsetText from '@common/components/InsetText';
import ReorderableList from '@common/components/ReorderableList';
import TileWrapper from '@common/modules/education-in-numbers/components/TileWrapper';
import VisuallyHidden from '@common/components/VisuallyHidden';
import useToggle from '@common/hooks/useToggle';
import {
  EinTile,
  EinTileGroupBlock,
  EinTileType,
} from '@common/services/types/einBlocks';
import reorder from '@common/utils/reorder';
import React, { ReactNode, useCallback, useEffect, useState } from 'react';

interface Props {
  block: EinTileGroupBlock;
  editable: boolean;
  groupButtonsLabel?: ReactNode | string;
  removeButtonLabel?: ReactNode | string;
  sectionId: string;
  onSave?: (content: string) => void;
  onDelete: () => void;
}

const EditableTileGroupBlock = ({
  block,
  editable,
  groupButtonsLabel,
  removeButtonLabel,
  sectionId,
  onDelete,
  onSave,
}: Props) => {
  const { pageVersion } = useEducationInNumbersPageContentState();
  const { addTile, reorderTiles } = useEducationInNumbersPageContentActions();

  const [groupTiles, setGroupTiles] = useState(block.tiles);

  useEffect(() => {
    setGroupTiles(block.tiles);
  }, [block]);

  const [isReordering, toggleIsReordering] = useToggle(false);
  const [isEditingHeading, toggleEditingHeading] = useToggle(false);
  const [isEditingStatTile, setIsEditingStatTile] = useState<string | null>(
    null,
  );

  const { id: educationInNumbersPageId } = pageVersion;
  const { title, tiles } = block;

  const [newHeading, setNewHeading] = useState(title);

  const saveHeading = useCallback(async () => {
    if (isEditingHeading && onSave && newHeading !== title) {
      await onSave(newHeading || '');
    }

    toggleEditingHeading.off();
  }, [title, isEditingHeading, newHeading, onSave, toggleEditingHeading]);

  const handleAddTile = async (type: EinTileType) => {
    const newTile = await addTile({
      educationInNumbersPageId,
      blockId: block.id,
      sectionId,
      type,
    });
    setIsEditingStatTile(newTile.id);
  };

  return (
    <EditableBlockWrapper
      removeButtonLabel={removeButtonLabel}
      onDelete={editable ? onDelete : undefined}
    >
      {isEditingHeading ? (
        <FormTextInput
          className="govuk-!-margin-bottom-2"
          id={`${block.id}-editHeading`}
          name="heading"
          label="Edit heading"
          autoFocus
          value={newHeading}
          onChange={e => {
            setNewHeading(e.target.value);
          }}
          onClick={e => {
            e.stopPropagation();
          }}
          onKeyPress={async e => {
            switch (e.key) {
              case 'Enter':
                await saveHeading();
                break;
              case 'Esc':
                toggleEditingHeading.off();
                break;
              default:
                break;
            }
          }}
        />
      ) : (
        title && (
          <h3
            className="govuk-heading-m govuk-!-margin-top-none"
            data-testid="tile-group-heading"
          >
            {title}
          </h3>
        )
      )}

      <ButtonGroup>
        {isEditingHeading ? (
          <Button onClick={saveHeading}>Save group heading</Button>
        ) : (
          !isEditingStatTile &&
          !isReordering && (
            <>
              <Button
                type="button"
                variant="secondary"
                onClick={toggleEditingHeading}
              >
                {title ? 'Edit group heading' : 'Add group heading'}
                <VisuallyHidden> for {groupButtonsLabel}</VisuallyHidden>
              </Button>

              <Button
                type="button"
                variant="secondary"
                onClick={() => handleAddTile('FreeTextStatTile')}
              >
                Add new free text stat tile
                <VisuallyHidden> in {groupButtonsLabel}</VisuallyHidden>
              </Button>

              <Button
                type="button"
                variant="secondary"
                onClick={() => handleAddTile('ApiQueryStatTile')}
              >
                Add new API query stat tile
                <VisuallyHidden> in {groupButtonsLabel}</VisuallyHidden>
              </Button>

              {groupTiles.length > 1 && !isReordering && (
                <Button variant="secondary" onClick={toggleIsReordering.on}>
                  Reorder tiles
                  <VisuallyHidden> in {groupButtonsLabel}</VisuallyHidden>
                </Button>
              )}
            </>
          )
        )}
      </ButtonGroup>

      {groupTiles.length ? (
        <>
          {isReordering ? (
            <ReorderableList
              heading="Reorder tiles"
              id="reorder-stat-tiles"
              list={groupTiles.map(tile => ({
                id: tile.id,
                label: getTileReorderLabel(tile),
              }))}
              onCancel={() => {
                setGroupTiles(block.tiles);
                toggleIsReordering.off();
              }}
              onConfirm={async () => {
                await reorderTiles({
                  educationInNumbersPageId,
                  blockId: block.id,
                  sectionId,
                  tiles: groupTiles,
                });
                toggleIsReordering.off();
              }}
              onMoveItem={({ prevIndex, nextIndex }) => {
                const reorderedGroupTiles = reorder(
                  groupTiles,
                  prevIndex,
                  nextIndex,
                );
                setGroupTiles(reorderedGroupTiles);
              }}
              onReverse={() => {
                setGroupTiles(groupTiles.toReversed());
              }}
            />
          ) : (
            <TileWrapper>
              {tiles.map(tile => (
                <EditableTile
                  key={tile.id}
                  blockId={block.id}
                  educationInNumbersPageId={educationInNumbersPageId}
                  groupTitle={title}
                  isEditing={isEditingStatTile === tile.id}
                  sectionId={sectionId}
                  showActions={!isEditingStatTile}
                  tile={tile}
                  onEdit={() => setIsEditingStatTile(tile.id)}
                  onEditEnd={() => setIsEditingStatTile(null)}
                />
              ))}
            </TileWrapper>
          )}
        </>
      ) : (
        <InsetText className="govuk-!-margin-top-2">
          No statistic tiles have been added.
        </InsetText>
      )}
    </EditableBlockWrapper>
  );
};

function getTileReorderLabel(tile: EinTile): string {
  switch (tile.type) {
    case 'FreeTextStatTile':
      return (
        [tile.title, tile.statistic, tile.trend].filter(Boolean).join(' ') ||
        'Unset free text stat tile'
      );
    case 'ApiQueryStatTile':
      return (
        [tile.title, tile.statistic].filter(Boolean).join(' ') ||
        'Unset API query stat tile'
      );
    default:
      return 'Unknown tile';
  }
}

export default EditableTileGroupBlock;
