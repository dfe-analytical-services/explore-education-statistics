import React from 'react';
import wrapEditableComponent, {
  ReleaseContentContext,
} from '@common/modules/find-statistics/util/wrapEditableComponent';
import KeyStatTile, {
  KeyStatProps,
} from '@common/modules/find-statistics/components/KeyStatTile';

interface EditableKeyStatProps extends KeyStatProps {
  editingContext?: ReleaseContentContext;
}

const EditableKeyStatTile = (/* props: EditableKeyStatProps */) => {
  return <>EditableKeyStat</>;
};

export default wrapEditableComponent(EditableKeyStatTile, KeyStatTile);
