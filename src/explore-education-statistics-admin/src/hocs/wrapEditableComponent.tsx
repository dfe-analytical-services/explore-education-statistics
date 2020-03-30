import { EditingContext } from '@admin/contexts/EditingContext';
import React, { ComponentType, useContext } from 'react';

const wrapEditableComponent = <EditableProps extends RenderProps, RenderProps>(
  EditableComponent: ComponentType<EditableProps>,
  RenderComponent: ComponentType<RenderProps>,
) => {
  return function WrappedEditableComponent(props: EditableProps | RenderProps) {
    const editingContext = useContext(EditingContext);
    const { isEditing } = editingContext;

    return isEditing ? (
      <EditableComponent
        {...(props as EditableProps)}
        editingContext={editingContext}
      />
    ) : (
      <RenderComponent {...(props as RenderProps)} />
    );
  };
};

export default wrapEditableComponent;
