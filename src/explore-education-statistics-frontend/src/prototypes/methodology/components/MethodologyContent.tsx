import * as React from 'react';

interface Props {
  children: React.ReactNode;
}

export class MethodologyContent extends React.Component<Props> {
  public render() {
    return (
      <div className="govuk-grid-column-three-quarters">
        {this.props.children}
      </div>
    );
  }
}
