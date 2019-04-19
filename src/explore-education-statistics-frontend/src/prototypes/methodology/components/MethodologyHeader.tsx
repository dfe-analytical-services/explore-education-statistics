import React, { createRef } from 'react';
import MethodologySection from './MethodologySection';

export interface MethodologyHeaderProps {
  children: React.ReactNode;
  parent?: MethodologySection;
}

export default class MethodologyHeader extends React.Component<
  MethodologyHeaderProps
> {
  private element = createRef<HTMLDivElement>();

  private staticRef = createRef<HTMLDivElement>();

  public componentDidMount(): void {
    window.addEventListener('scroll', this.scroll);

    requestAnimationFrame(() => this.updateElements());
  }

  public componentWillUnmount(): void {
    window.removeEventListener('scroll', this.scroll);
  }

  private scroll = () => requestAnimationFrame(() => this.updateElements());

  private updateElements() {
    const { parent } = this.props;

    if (this.element.current && parent && this.staticRef.current) {
      const parentTop = parent.scrollTop;
      const parentBottom = parentTop + parent.height;
      const headerHeight = this.element.current.clientHeight;
      const headerBottom = parentBottom - headerHeight;

      if (parentTop < 0) {
        this.staticRef.current.classList.add('fixed');
        if (headerBottom > 0) {
          this.staticRef.current.style.top = `${-parentTop}px`;
        } else {
          this.staticRef.current.style.top = `${parent.height -
            headerHeight}px`;
        }
      } else {
        this.staticRef.current.classList.remove('fixed');
        this.staticRef.current.style.top = '';
      }
    }
  }

  public render() {
    const { children } = this.props;

    return (
      <div className="govuk-grid-column-one-quarter" ref={this.element}>
        <div ref={this.staticRef}>{children}</div>
      </div>
    );
  }
}
