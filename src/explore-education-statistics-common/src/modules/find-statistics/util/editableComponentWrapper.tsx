
import React, {ComponentType} from "react";

export interface ReleaseContentContext {
  isEditing: boolean
}

export const EditingContext = React.createContext<ReleaseContentContext>({ isEditing: true});

const editableComponentWrapper = <EditableProps, RenderProps>(
  EditableComponent: ComponentType<EditableProps>,
  RenderComponent: ComponentType<RenderProps>
) => {
  // eslint-disable-next-line react/display-name
  return (props : EditableProps | RenderProps) => (
    <EditingContext.Consumer>
      {({isEditing}) =>
        isEditing ? (
          <EditableComponent {...(props as EditableProps)} />
        ) : (
          <RenderComponent {...(props as RenderProps)} />
        )
      }
    </EditingContext.Consumer>
  );
};

editableComponentWrapper.displayName = "EditableComponentWrapper";

export default editableComponentWrapper;