import EditableBlockWrapper from '@admin/components/editable/EditableBlockWrapper';
import EditableFreeTextStatTileForm from '@admin/pages/education-in-numbers/content/components/EditableFreeTextStatTileForm';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import { FormTextInput } from '@common/components/form';
import Gate from '@common/components/Gate';
import useToggle from '@common/hooks/useToggle';
import { EinTileGroupBlock } from '@common/services/types/einBlocks';
import React, { useCallback, useState } from 'react';
import useEducationInNumbersPageContentActions from '@admin/pages/education-in-numbers/content/context/useEducationInNumbersPageContentActions';
import { useEducationInNumbersPageContentState } from '@admin/pages/education-in-numbers/content/context/EducationInNumbersPageContentContext';
import InsetText from '@common/components/InsetText';

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
  const { addFreeTextStatTile } = useEducationInNumbersPageContentActions();

  const [isEditingHeading, toggleEditingHeading] = useToggle(false);
  const [isEditingStatTile, setIsEditingStatTile] = useState<string | null>(
    null,
  );

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
      educationInNumbersPageId: pageVersion.id,
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
          title && <h3>{title}</h3>
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

        {/* Reorderable buttons */}

        {/* EditableTiles with edit, remove */}
        {tiles.length ? (
          tiles.map(tile => (
            <div key={tile.id}>
              {isEditingStatTile === tile.id ? (
                <EditableFreeTextStatTileForm
                  testId="freeTextStatTile-editForm"
                  onSubmit={values =>
                    // addFreeTextStatTile({
                    //   educationInNumbersPageId: pageVersion.id,
                    //   blockId: block.id,
                    //   sectionId,
                    // })
                    console.log(values)
                  }
                  onCancel={() => setIsEditingStatTile(null)}
                />
              ) : (
                <>
                  <div style={{ padding: '8px', border: '1px solid #b1b4b6' }}>
                    <strong>{tile.trend}</strong>
                  </div>
                  <Button onClick={() => setIsEditingStatTile(tile.id)}>
                    Edit tile
                  </Button>
                </>
              )}
            </div>
          ))
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
