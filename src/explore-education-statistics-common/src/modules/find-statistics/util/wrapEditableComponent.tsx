import React, { ComponentType, useContext } from 'react';

export interface ReleaseContentContext {
  isEditing: boolean;
  isCommenting: boolean;
  isReviewing: boolean;
  releaseId: string | undefined;
}

export const EditingContext = React.createContext<ReleaseContentContext>({
  isEditing: false,
  isCommenting: false,
  isReviewing: false,
  releaseId: undefined,
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
