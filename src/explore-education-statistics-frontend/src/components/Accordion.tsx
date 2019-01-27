import GovUkAccordion from 'govuk-frontend/components/accordion/accordion';
import React, { cloneElement, Component, createRef, ReactNode } from 'react';
import isComponentType from '../lib/type-guards/components/isComponentType';
import AccordionSection, { AccordionSectionProps } from './AccordionSection';

interface Props {
  children: ReactNode;
  id: string;
}

class Accordion extends Component<Props> {
  private ref = createRef<HTMLDivElement>();

  public componentDidMount(): void {
    new GovUkAccordion(this.ref.current).init();
  }

  public render() {
    const { id } = this.props;

    let sectionId = 0;

    return (
      <div className="govuk-accordion" ref={this.ref} id={id}>
        {React.Children.map(this.props.children, child => {
          if (isComponentType(child, AccordionSection)) {
            sectionId += 1;

            return cloneElement<AccordionSectionProps>(child, {
              contentId: `${id}-content-${sectionId}`,
              headingId: `${id}-heading-${sectionId}`,
            });
          }

          return child;
        })}
      </div>
    );
  }
}

export default Accordion;
