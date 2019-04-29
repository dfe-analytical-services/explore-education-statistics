import isComponentType from '@common/lib/type-guards/components/isComponentType';
import React, { cloneElement, Component, createRef, ReactNode } from 'react';
import MethodologyHeader, { MethodologyHeaderProps } from './MethodologyHeader';

interface MethodologySectionProps {
  children: React.ReactNode;
}

export default class MethodologySection extends Component<
  MethodologySectionProps
> {
  private container = createRef<HTMLDivElement>();

  public get height() {
    if (this.container.current) {
      return this.container.current.offsetHeight;
    }
    return 0;
  }

  public get scrollTop() {
    if (this.container.current) {
      return this.container.current.getBoundingClientRect().top;
    }
    return 0;
  }

  public render() {
    const { children } = this.props;

    return (
      <div className="govuk-grid-row" ref={this.container}>
        {React.Children.map(children, (child: ReactNode) => {
          if (isComponentType(child, MethodologyHeader)) {
            return cloneElement<MethodologyHeaderProps>(child, {
              parent: this,
            });
          }

          return child;
        })}
      </div>
    );
  }
}
