import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import VisuallyHidden from '@common/components/VisuallyHidden';
import KeyStat, {
  KeyStatProps,
} from '@common/modules/find-statistics/components/KeyStat';
import { OmitStrict } from '@common/types';
import React from 'react';

export interface EditableKeyStatDisplayProps
  extends OmitStrict<KeyStatProps, 'children' | 'hasColumn'> {
  isReordering: boolean;
  isEditing: boolean;
  onRemove?: () => void;
  onEdit: () => void;
}

export default function EditableKeyStatPreview({
  isReordering,
  isEditing,
  onRemove,
  onEdit,
  ...keyStatProps
}: EditableKeyStatDisplayProps) {
  const { title } = keyStatProps;

  return (
    <KeyStat {...keyStatProps} hasColumn={false}>
      {isEditing && !isReordering && (
        <ButtonGroup className="govuk-!-margin-top-2">
          <Button onClick={onEdit}>
            Edit <VisuallyHidden> key statistic: {title}</VisuallyHidden>
          </Button>

          {onRemove && (
            <Button variant="secondary" onClick={onRemove}>
              Remove <VisuallyHidden> key statistic: {title}</VisuallyHidden>
            </Button>
          )}
        </ButtonGroup>
      )}
    </KeyStat>
  );
}
