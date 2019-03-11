import React, { cloneElement, Component, createRef, ReactNode } from 'react';
import isComponentType from '../lib/type-guards/components/isComponentType';
import AccordionSection, { AccordionSectionProps } from './AccordionSection';

export interface AccordionProps {
  children: ReactNode;
  id: string;
}

class Accordion extends Component<AccordionProps> {
  private ref = createRef<HTMLDivElement>();

  public componentDidMount(): void {
    import('govuk-frontend/components/accordion/accordion').then(
      ({ default: GovUkAccordion }) => {
        if (this.ref.current) {
          new GovUkAccordion(this.ref.current).init();

          if (location.hash) {
            const anchor = this.ref.current.querySelector(
              location.hash,
            ) as HTMLButtonElement;

            if (anchor) {
              anchor.scrollIntoView();
            }
          }
        }
      },
    );
  }

  public render() {
    const { id } = this.props;

    let sectionId = 0;

    return (
      <div className="govuk-accordion" ref={this.ref} id={id}>
        {React.Children.map(this.props.children, child => {
          if (isComponentType(child, AccordionSection)) {
            sectionId += 1;

            const contentId = `${id}-content-${sectionId}`;
            const headingId = `${id}-heading-${sectionId}`;

            const isLocationHashMatching =
              process.browser &&
              (location.hash === `#${headingId}` ||
                location.hash === `#${contentId}`);

            return cloneElement<AccordionSectionProps>(child, {
              contentId,
              headingId,
              open: child.props.open || isLocationHashMatching,
            });
          }

          return child;
        })}
      </div>
    );
  }
}

export default Accordion;
