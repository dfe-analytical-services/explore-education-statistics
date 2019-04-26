import React, { Component, createRef, ReactNode } from 'react';

interface Props {
  children: ReactNode;
}

class FormConditionalRadioGroup extends Component<Props> {
  private ref = createRef<HTMLDivElement>();

  public componentDidMount(): void {
    if (this.ref.current) {
      import('govuk-frontend/components/radios/radios').then(
        ({ default: GovUkRadios }) => {
          console.log(this.ref.current);
          new GovUkRadios(this.ref.current).init();
        },
      );
    }
  }

  public render() {
    const { children } = this.props;

    return (
      <div className="govuk-radios govuk-radios--conditional" ref={this.ref}>
        {children}
      </div>
    );
  }
}

export default FormConditionalRadioGroup;
