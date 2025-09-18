import EditableBlockWrapper from '@admin/components/editable/EditableBlockWrapper';
import EditableFreeTextStatTileForm from '@admin/pages/education-in-numbers/content/components/EditableFreeTextStatTileForm';
import { useEducationInNumbersPageContentState } from '@admin/pages/education-in-numbers/content/context/EducationInNumbersPageContentContext';
import useEducationInNumbersPageContentActions from '@admin/pages/education-in-numbers/content/context/useEducationInNumbersPageContentActions';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import { FormTextInput } from '@common/components/form';
import Gate from '@common/components/Gate';
import InsetText from '@common/components/InsetText';
import VisuallyHidden from '@common/components/VisuallyHidden';
import useToggle from '@common/hooks/useToggle';
import FreeTextStatTile from '@common/modules/education-in-numbers/components/FreeTextStatTile';
import FreeTextStatTileWrapper from '@common/modules/education-in-numbers/components/FreeTextStatTileWrapper';
import { EinTileGroupBlock } from '@common/services/types/einBlocks';
import React, { useCallback, useState } from 'react';

interface Props {
  block: EinTileGroupBlock;
  editable: boolean;
  sectionId: string;
  visible?: boolean;
  onSave?: (content: string) => void;
  onDelete: () => void;
}

const EditableTileGroupBlock = ({
  block,
  editable,
  sectionId,
  visible = true,
  onDelete,
  onSave,
}: Props) => {
  const { pageVersion } = useEducationInNumbersPageContentState();
  const {
    addFreeTextStatTile,
    updateFreeTextStatTile,
    deleteFreeTextStatTile,
  } = useEducationInNumbersPageContentActions();

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
      <Gate condition={!!visible}>
        {isEditingHeading ? (
          <FormTextInput
            className="govuk-!-margin-bottom-2"
            id={`${block.id}-editHeading`}
            name="heading"
            label="Edit Heading"
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
            <h3 className="govuk-heading-l govuk-!-margin-top-none">{title}</h3>
          )
        )}

        <ButtonGroup>
          {isEditingHeading ? (
            <Button onClick={saveHeading}>Save group title</Button>
          ) : (
            !isEditingStatTile && (
              <>
                <Button
                  type="button"
                  variant="secondary"
                  onClick={toggleEditingHeading}
                >
                  {title ? 'Edit' : 'Add'} group title
                </Button>

                <Button
                  type="button"
                  variant="secondary"
                  className="govuk-!-margin-left-2"
                  onClick={handleAddStatTile}
                >
                  Add stat tile
                </Button>
              </>
            )
          )}
        </ButtonGroup>

        {/* TODO Reorderable buttons */}

        {tiles.length ? (
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
                        <Button onClick={() => setIsEditingStatTile(tile.id)}>
                          Edit <VisuallyHidden> tile: {title}</VisuallyHidden>
                        </Button>
                        <Button
                          variant="warning"
                          onClick={async () => {
                            await deleteFreeTextStatTile({
                              educationInNumbersPageId,
                              blockId: block.id,
                              sectionId,
                              tileId: tile.id,
                            });
                          }}
                        >
                          Delete tile<VisuallyHidden>: {title}</VisuallyHidden>
                        </Button>
                      </ButtonGroup>
                    )}
                  </>
                )}
              </div>
            ))}
          </FreeTextStatTileWrapper>
        ) : (
          <InsetText className="govuk-!-margin-top-2">
            No statistic tiles have been added.
          </InsetText>
        )}
      </Gate>
    </EditableBlockWrapper>
  );
};

export default EditableTileGroupBlock;
