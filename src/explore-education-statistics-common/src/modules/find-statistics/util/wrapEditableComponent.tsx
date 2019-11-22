import React, { ComponentType, useContext } from 'react';

export interface ReleaseContentContext {
  isEditing: boolean;
  releaseId: string | undefined
}

export const EditingContext = React.createContext<ReleaseContentContext>({
  isEditing: true,
  releaseId: undefined
});

const wrapEditableComponent = <EditableProps extends RenderProps, RenderProps>(
  EditableComponent: ComponentType<EditableProps>,
  RenderComponent: ComponentType<RenderProps>,
) => {
  return function WrappedEditableComponent(props: EditableProps | RenderProps) {
    const { isEditing } = useContext(EditingContext);
    return isEditing ? (
      <EditableComponent {...(props as EditableProps)} />
    ) : (
      <RenderComponent {...(props as RenderProps)} />
    );
  };
};

export default wrapEditableComponent;
