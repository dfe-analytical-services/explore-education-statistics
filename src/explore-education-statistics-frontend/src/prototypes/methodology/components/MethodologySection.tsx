import isComponentType from '@common/lib/type-guards/components/isComponentType';
import React, { cloneElement, Component, ReactNode } from 'react';
import {
  MethodologyHeader,
  MethodologyHeaderProps,
} from 'src/prototypes/methodology/components/MethodologyHeader';

interface MethodologySectionProps {
  children: React.ReactNode;
}

export class MethodologySection extends Component<MethodologySectionProps> {
  private container: HTMLElement | null;

  public constructor(props: MethodologySectionProps) {
    super(props);
    this.container = null;
  }

  public get height() {
    if (this.container) {
      return this.container.offsetHeight;
    }
    return 0;
  }

  public get scrollTop() {
    if (this.container) {
      return this.container.getBoundingClientRect().top;
    }
    return 0;
  }

  public render() {
    return (
      <div className="govuk-grid-row" ref={ref => (this.container = ref)}>
        {React.Children.map(this.props.children, (child: ReactNode) => {
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
