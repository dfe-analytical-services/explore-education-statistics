import { useConfig } from '@admin/contexts/ConfigContext';
import EditableApiQueryStatTileForm from '@admin/pages/education-in-numbers/content/components/EditableApiQueryStatTileForm';
import EditableFreeTextStatTileForm from '@admin/pages/education-in-numbers/content/components/EditableFreeTextStatTileForm';
import useEducationInNumbersPageContentActions from '@admin/pages/education-in-numbers/content/context/useEducationInNumbersPageContentActions';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import ModalConfirm from '@common/components/ModalConfirm';
import VisuallyHidden from '@common/components/VisuallyHidden';
import WarningMessage from '@common/components/WarningMessage';
import ApiQueryStatTile from '@common/modules/education-in-numbers/components/ApiQueryStatTile';
import FreeTextStatTile from '@common/modules/education-in-numbers/components/FreeTextStatTile';
import { EinTile } from '@common/services/types/einBlocks';
import React, { ReactNode } from 'react';

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
  const { updateTile, deleteTile } = useEducationInNumbersPageContentActions();

  const tileParams = {
    educationInNumbersPageId,
    blockId,
    sectionId,
    tileId: tile.id,
  };

  let tileContent: ReactNode;

  switch (tile.type) {
    case 'FreeTextStatTile':
      tileContent = isEditing ? (
        <EditableFreeTextStatTileForm
          freeTextStatTile={tile}
          testId="freeTextStatTile-editForm"
          onSubmit={async values => {
            await updateTile({ ...tileParams, type: tile.type, values });
            onEditEnd();
          }}
          onCancel={onEditEnd}
        />
      ) : (
        <FreeTextStatTile tile={tile} />
      );
      break;
    case 'ApiQueryStatTile':
      tileContent = isEditing ? (
        <EditableApiQueryStatTileForm
          apiQueryStatTile={tile}
          testId="apiQueryStatTile-editForm"
          onSubmit={async values => {
            await updateTile({ ...tileParams, type: tile.type, values });
            onEditEnd();
          }}
          onCancel={onEditEnd}
        />
      ) : (
        <>
          <ApiQueryStatTile tile={tile} publicAppUrl={publicAppUrl} />
          {!tile.isLatestVersion && (
            <WarningMessage
              className="govuk-!-margin-top-2 govuk-!-margin-bottom-0"
              testId="apiQueryStatTile-notLatestVersionWarning"
            >
              A newer version of this tile's API data set has been published.
              Edit the tile to re-run its query against the latest version.
            </WarningMessage>
          )}
        </>
      );
      break;
    default:
      return null;
  }

  if (isEditing) {
    return tileContent;
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
