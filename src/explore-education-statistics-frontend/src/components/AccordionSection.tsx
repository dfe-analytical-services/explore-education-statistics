import classNames from 'classnames';
import React, { Component, ReactNode } from 'react';

interface Props {
  caption?: string;
  children: ReactNode;
  className?: string;
  heading: string;
  // Only for accessibility/semantic markup,
  // does not change the actual styling
  headingTag?: 'h2' | 'h3' | 'h4';
  id: string;
  open?: boolean;
}

interface State {
  isCollapsed: boolean;
}

class AccordionSection extends Component<Props> {
  public static defaultProps: Partial<Props> = {
    headingTag: 'h2',
    open: false,
  };

  public render() {
    const {
      caption,
      className,
      children,
      heading,
      headingTag,
    } = this.props;

    return (
      <div className={classNames('govuk-accordion__section', className)}>
        <div className="govuk-accordion__section-header">
          {React.createElement(
            `${headingTag}`,
            { className: 'govuk-accordion__section-heading' },
            <span className="govuk-accordion__section-button">{heading}</span>,
          )}
          {caption && (
            <span className="govuk-accordion__section-summary">{caption}</span>
          )}
        </div>

        <div className="govuk-accordion__section-content">{children}</div>
      </div>
    );
  }
}

export default AccordionSection;
