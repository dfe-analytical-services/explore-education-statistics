import GovUkAccordion from 'govuk-frontend/components/accordion/accordion';
import React, { cloneElement, Component, createRef, ReactNode } from 'react';
import { RouteComponentProps, withRouter } from 'react-router';
import isComponentType from '../lib/type-guards/components/isComponentType';
import AccordionSection, { AccordionSectionProps } from './AccordionSection';

export interface AccordionProps {
  children: ReactNode;
  id: string;
}

class Accordion extends Component<AccordionProps & RouteComponentProps> {
  private ref = createRef<HTMLDivElement>();

  public componentDidMount(): void {
    if (this.ref.current) {
      new GovUkAccordion(this.ref.current).init();

      const { location } = this.props;

      if (location && location.hash) {
        const anchor = this.ref.current.querySelector(location.hash);

        if (anchor) {
          anchor.scrollIntoView();
        }
      }
    }
  }

  public render() {
    const { id, location } = this.props;

    let sectionId = 0;

    return (
      <div className="govuk-accordion" ref={this.ref} id={id}>
        {React.Children.map(this.props.children, child => {
          if (isComponentType(child, AccordionSection)) {
            sectionId += 1;

            const contentId = `${id}-content-${sectionId}`;
            const headingId = `${id}-heading-${sectionId}`;

            let isLocationHashMatching = false;

            if (location) {
              isLocationHashMatching =
                location.hash === `#${headingId}` ||
                location.hash === `#${contentId}`;
            }

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

export default withRouter(Accordion);
