import React, { ComponentType, useContext } from 'react';
import { DataBlock } from '@common/services/dataBlockService';

export interface ReleaseContentContext {
  isEditing: boolean;
  isCommenting: boolean;
  isReviewing: boolean;
  releaseId: string | undefined;
  availableDataBlocks: DataBlock[];
  updateAvailableDataBlocks?: () => void;
}

export const EditingContext = React.createContext<ReleaseContentContext>({
  isEditing: false,
  isCommenting: false,
  isReviewing: false,
  releaseId: undefined,
  availableDataBlocks: [],
});

const wrapEditableComponent = <EditableProps extends RenderProps, RenderProps>(
  EditableComponent: ComponentType<EditableProps>,
  RenderComponent: ComponentType<RenderProps>,
) => {
  return function WrappedEditableComponent(props: EditableProps | RenderProps) {
    const { isEditing, ...context } = useContext(EditingContext);
    return isEditing ? (
      <EditableComponent
        {...(props as EditableProps)}
        editingContext={{ isEditing, ...context }}
      />
    ) : (
      <RenderComponent {...(props as RenderProps)} />
    );
  };
};

export default wrapEditableComponent;
