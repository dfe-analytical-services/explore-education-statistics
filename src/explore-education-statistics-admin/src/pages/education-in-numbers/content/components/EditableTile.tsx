import { useConfig } from '@admin/contexts/ConfigContext';
import EditableApiQueryStatTileForm from '@admin/pages/education-in-numbers/content/components/EditableApiQueryStatTileForm';
import EditableFreeTextStatTileForm from '@admin/pages/education-in-numbers/content/components/EditableFreeTextStatTileForm';
import useEducationInNumbersPageContentActions from '@admin/pages/education-in-numbers/content/context/useEducationInNumbersPageContentActions';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import ModalConfirm from '@common/components/ModalConfirm';
import VisuallyHidden from '@common/components/VisuallyHidden';
import ApiQueryStatTile from '@common/modules/education-in-numbers/components/ApiQueryTextStatTile';
import FreeTextStatTile from '@common/modules/education-in-numbers/components/FreeTextStatTile';
import { EinTile } from '@common/services/types/einBlocks';
import React from 'react';

interface Props {
  blockId: string;
  educationInNumbersPageId: string;
  groupTitle?: string;
  isEditing: boolean;
  sectionId: string;
  showActions: boolean;
  tile: EinTile;
  onEdit: () => void;
  onEditEnd: () => void;
}

export default function EditableTile({
  blockId,
  educationInNumbersPageId,
  groupTitle,
  isEditing,
  sectionId,
  showActions,
  tile,
  onEdit,
  onEditEnd,
}: Props) {
  const { publicAppUrl } = useConfig();
  const { updateFreeTextStatTile, updateApiQueryStatTile, deleteTile } =
    useEducationInNumbersPageContentActions();

  const tileParams = {
    educationInNumbersPageId,
    blockId,
    sectionId,
    tileId: tile.id,
  };

  if (isEditing) {
    switch (tile.type) {
      case 'FreeTextStatTile':
        return (
          <div>
            <EditableFreeTextStatTileForm
              freeTextStatTile={tile}
              testId="freeTextStatTile-editForm"
              onSubmit={async values => {
                await updateFreeTextStatTile({ ...tileParams, values });
                onEditEnd();
              }}
              onCancel={onEditEnd}
            />
          </div>
        );
      case 'ApiQueryStatTile':
        return (
          <div>
            <EditableApiQueryStatTileForm
              apiQueryStatTile={tile}
              testId="apiQueryStatTile-editForm"
              onSubmit={async values => {
                await updateApiQueryStatTile({ ...tileParams, values });
                onEditEnd();
              }}
              onCancel={onEditEnd}
            />
          </div>
        );
      default:
        return null;
    }
  }

  const tileContent = (() => {
    switch (tile.type) {
      case 'FreeTextStatTile':
        return <FreeTextStatTile tile={tile} />;
      case 'ApiQueryStatTile':
        return <ApiQueryStatTile tile={tile} publicAppUrl={publicAppUrl} />;
      default:
        return null;
    }
  })();

  if (!tileContent) {
    return null;
  }

  return (
    <div>
      {tileContent}

      {showActions && (
        <ButtonGroup className="govuk-!-margin-top-2">
          <Button onClick={onEdit} variant="secondary">
            Edit <VisuallyHidden> tile: {groupTitle}</VisuallyHidden>
          </Button>
          <ModalConfirm
            title="Remove tile"
            triggerButton={
              <ButtonText variant="warning">
                Delete tile
                <VisuallyHidden>- {groupTitle}</VisuallyHidden>
              </ButtonText>
            }
            onConfirm={async () => {
              await deleteTile(tileParams);
            }}
          >
            <p>Are you sure you want to remove this tile?</p>
          </ModalConfirm>
        </ButtonGroup>
      )}
    </div>
  );
}
