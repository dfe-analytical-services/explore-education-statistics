import classNames from 'classnames';
import React, {
  Component,
  createElement,
  MouseEventHandler,
  ReactEventHandler,
  ReactNode,
} from 'react';

export interface AccordionSectionProps {
  caption?: string;
  children: ReactNode;
  className?: string;
  contentId?: string;
  heading: string;
  headingId?: string;
  // Only for accessibility/semantic markup,
  // does not change the actual styling
  headingTag?: 'h2' | 'h3' | 'h4';
  id?: string;
  open?: boolean;
  goToTopLink: boolean;
  onClick: MouseEventHandler;
}

class AccordionSection extends Component<AccordionSectionProps> {
  public static defaultProps: Partial<AccordionSectionProps> = {
    goToTopLink: true,
    headingTag: 'h2',
    onClick: () => null,
    open: false,
  };

  public render() {
    const {
      caption,
      className,
      children,
      contentId,
      heading,
      headingId,
      headingTag,
      open,
      goToTopLink,
      onClick,
    } = this.props;

    return (
      <div
        onClick={e => onClick(e)}
        className={classNames('govuk-accordion__section', className, {
          'govuk-accordion__section--expanded': open,
        })}
      >
        <div className="govuk-accordion__section-header">
          {createElement(
            `${headingTag}`,
            {
              className: 'govuk-accordion__section-heading',
            },
            <span className="govuk-accordion__section-button" id={headingId}>
              {heading}
            </span>,
          )}
          {caption && (
            <span className="govuk-accordion__section-summary">{caption}</span>
          )}
        </div>

        <div
          className="govuk-accordion__section-content"
          aria-labelledby={headingId}
          id={contentId}
        >
          {children}
          <br />
          {goToTopLink && <a href="#application">Go to Top</a>}
        </div>
      </div>
    );
  }
}

export default AccordionSection;
