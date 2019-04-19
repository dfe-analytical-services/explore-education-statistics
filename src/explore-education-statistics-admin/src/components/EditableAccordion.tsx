import isComponentType from '@common/lib/type-guards/components/isComponentType';
import React, { cloneElement, Component, createRef, ReactNode } from 'react';
import EditableAccordionSection, {
  EditableAccordionSectionProps,
} from './EditableAccordionSection';

export interface EditableAccordionProps {
  children: ReactNode;
  id: string;
  index: number;
}

class EditableAccordion extends Component<EditableAccordionProps> {
  private ref = createRef<HTMLDivElement>();

  public componentDidMount(): void {
    import('govuk-frontend/components/accordion/accordion').then(
      ({ default: GovUkAccordion }) => {
        if (this.ref.current) {
          new GovUkAccordion(this.ref.current).init();
        }
      },
    );
  }

  public render() {
    const { children, id, index } = this.props;

    return (
      <div className="govuk-accordion" ref={this.ref} id={id}>
        {React.Children.map(children, (child, thisIndex) => {
          if (isComponentType(child, EditableAccordionSection)) {
            return cloneElement<EditableAccordionSectionProps>(child, {
              index: thisIndex,
              droppableIndex: index,
            });
          }

          return child;
        })}
      </div>
    );
  }
}

export default EditableAccordion;
