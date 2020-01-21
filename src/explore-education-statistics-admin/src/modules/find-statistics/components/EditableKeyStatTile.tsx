import React from 'react';
import wrapEditableComponent, {
  ReleaseContentContext,
} from '@common/modules/find-statistics/util/wrapEditableComponent';
import KeyStatTile, {
  KeyStatProps,
} from '@common/modules/find-statistics/components/KeyStatTile';

interface EditableKeyStatProps extends KeyStatProps, ReleaseContentContext {}

const EditableKeyStatTile = ({
  isEditing,
  isCommenting,
  isReviewing,
  releaseId,
  availableDataBlocks,
  ...summary
}: EditableKeyStatProps) => {
  return <>EditableKeyStat</>;
};

export default wrapEditableComponent(EditableKeyStatTile, KeyStatTile);
