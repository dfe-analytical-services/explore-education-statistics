import React, {ComponentType, ReactComponentElement, ReactElement, useContext, WeakValidationMap} from "react";

export interface ReleaseContentContext {
  isEditing: boolean
}

export const EditingContext = React.createContext<ReleaseContentContext>({isEditing: true});



const wrapEditableComponent = <EditableProps, RenderProps>(
  EditableComponent: ComponentType<EditableProps>,
  RenderComponent: ComponentType<RenderProps>
) => {

  const wrapper= function WrappedEditableComponent(props: EditableProps | RenderProps) {
    const {isEditing} = useContext(EditingContext);
    return isEditing ?
      <EditableComponent {...(props as EditableProps)} />
      :
      <RenderComponent {...(props as RenderProps)} />
  };

  wrapper.editable = EditableComponent.name;
  wrapper.renderer = RenderComponent.name;

  return wrapper;
};

export function isWrapped<P>(
  value: unknown,
  componentType: ComponentType<P>,
): value is ReactComponentElement<ComponentType<P>, P> {

  if (!value) {
    return false;
  }

  const element = value as ReactElement;

  if (typeof element.type === 'function') {
    // @ts-ignore
    if (element.type.name === "WrappedEditableComponent") {
      console.log(componentType);
    }
  }

  return false;
}


export default wrapEditableComponent;