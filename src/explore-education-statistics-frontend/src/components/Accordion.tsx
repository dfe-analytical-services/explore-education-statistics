import GovUkAccordion from 'govuk-frontend/components/accordion/accordion';
import React, { Component, createRef, ReactNode } from 'react';

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
    return (
      <div className="govuk-accordion" ref={this.ref} id={this.props.id}>
        {this.props.children}
      </div>
    );
  }
}

export default Accordion;
