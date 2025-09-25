import EditableBlockWrapper from '@admin/components/editable/EditableBlockWrapper';
import EditableFreeTextStatTileForm from '@admin/pages/education-in-numbers/content/components/EditableFreeTextStatTileForm';
import { useEducationInNumbersPageContentState } from '@admin/pages/education-in-numbers/content/context/EducationInNumbersPageContentContext';
import useEducationInNumbersPageContentActions from '@admin/pages/education-in-numbers/content/context/useEducationInNumbersPageContentActions';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import { FormTextInput } from '@common/components/form';
import InsetText from '@common/components/InsetText';
import ModalConfirm from '@common/components/ModalConfirm';
import ReorderableList from '@common/components/ReorderableList';
import VisuallyHidden from '@common/components/VisuallyHidden';
import useToggle from '@common/hooks/useToggle';
import FreeTextStatTile from '@common/modules/education-in-numbers/components/FreeTextStatTile';
import FreeTextStatTileWrapper from '@common/modules/education-in-numbers/components/FreeTextStatTileWrapper';
import { EinTileGroupBlock } from '@common/services/types/einBlocks';
import reorder from '@common/utils/reorder';
import React, { useCallback, useEffect, useState } from 'react';

interface Props {
  block: EinTileGroupBlock;
  editable: boolean;
  sectionId: string;
  onSave?: (content: string) => void;
  onDelete: () => void;
}

const EditableTileGroupBlock = ({
  block,
  editable,
  sectionId,
  onDelete,
  onSave,
}: Props) => {
  const { pageVersion } = useEducationInNumbersPageContentState();
  const {
    addFreeTextStatTile,
    updateFreeTextStatTile,
    deleteFreeTextStatTile,
    reorderFreeTextStatTiles,
  } = useEducationInNumbersPageContentActions();

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

  const handleAddStatTile = async () => {
    const newTile = await addFreeTextStatTile({
      educationInNumbersPageId,
      blockId: block.id,
      sectionId,
    });
    setIsEditingStatTile(newTile.id);
  };

  return (
    <EditableBlockWrapper onDelete={editable ? onDelete : undefined}>
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
                {title ? 'Edit' : 'Add'} group heading
              </Button>

              <Button
                type="button"
                variant="secondary"
                onClick={handleAddStatTile}
              >
                Add new tile
              </Button>

              {groupTiles.length > 1 && !isReordering && (
                <Button variant="secondary" onClick={toggleIsReordering.on}>
                  Reorder tiles
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
              list={groupTiles.map(tile => {
                return {
                  id: tile.id,
                  label: `${tile.title} ${tile.statistic} ${tile.trend}`,
                };
              })}
              onCancel={() => {
                setGroupTiles(block.tiles);
                toggleIsReordering.off();
              }}
              onConfirm={async () => {
                await reorderFreeTextStatTiles({
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
            <FreeTextStatTileWrapper>
              {tiles.map(tile => (
                <div key={tile.id}>
                  {isEditingStatTile === tile.id ? (
                    <EditableFreeTextStatTileForm
                      statTile={tile}
                      testId="freeTextStatTile-editForm"
                      onSubmit={async values => {
                        await updateFreeTextStatTile({
                          educationInNumbersPageId,
                          blockId: block.id,
                          sectionId,
                          tileId: tile.id,
                          values,
                        });
                        setIsEditingStatTile(null);
                      }}
                      onCancel={() => setIsEditingStatTile(null)}
                    />
                  ) : (
                    <>
                      <FreeTextStatTile key={tile.id} tile={tile} />
                      {!isEditingStatTile && (
                        <ButtonGroup className="govuk-!-margin-top-2">
                          <Button
                            onClick={() => setIsEditingStatTile(tile.id)}
                            variant="secondary"
                          >
                            Edit <VisuallyHidden> tile: {title}</VisuallyHidden>
                          </Button>
                          <ModalConfirm
                            title="Remove tile"
                            triggerButton={
                              <ButtonText variant="warning">
                                Delete tile
                                <VisuallyHidden>- {title}</VisuallyHidden>
                              </ButtonText>
                            }
                            onConfirm={async () => {
                              await deleteFreeTextStatTile({
                                educationInNumbersPageId,
                                blockId: block.id,
                                sectionId,
                                tileId: tile.id,
                              });
                            }}
                          >
                            <p>Are you sure you want to remove this tile?</p>
                          </ModalConfirm>
                        </ButtonGroup>
                      )}
                    </>
                  )}
                </div>
              ))}
            </FreeTextStatTileWrapper>
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

export default EditableTileGroupBlock;
