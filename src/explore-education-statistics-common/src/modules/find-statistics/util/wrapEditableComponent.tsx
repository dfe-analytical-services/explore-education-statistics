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



export default wrapEditableComponent;