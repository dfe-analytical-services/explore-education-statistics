import classNames from 'classnames';
import React, {createElement, ReactNode} from 'react';
import GoToTopLink from "../../components/GoToTopLink";


export interface AccordionSectionProps {
  caption?: string;
  children: ReactNode;
  className?: string;
  contentId?: string;
  goToTop?: boolean;
  heading: string;
  headingId?: string;
  // Only for accessibility/semantic markup,
  // does not change the actual styling
  headingTag?: 'h2' | 'h3' | 'h4';
  index?: number,
  id?: string;
  open?: boolean;
  onToggle?: (open: boolean) => void;
}

class PrototypeEditableAccordionSection extends React.Component<AccordionSectionProps> {

  private header?: any ;

  onDragStart = (e: any) => {
    e.dataTransfer.effectAllowed = "move";
    e.dataTransfer.setData("text/html", e.target.parentNode);
    e.dataTransfer.setDragImage(this.header, 20, 20);
  };

  onDragOver = (e: any) => {
    console.log(this.props.index);
  };

  render() {


    const {
      caption,
      className,
      children,
      contentId,
      goToTop = true,
      heading,
      headingId,
      headingTag = 'h2',
      open = false,
      onToggle,
    } = this.props;

    return (
      <div draggable
           onDragStart={this.onDragStart}
           onDragOver={this.onDragOver}
           onClick={event => {
             if (onToggle) {
               onToggle(
                 event.currentTarget.classList.contains(
                   'govuk-accordion__section--expanded',
                 ),
               );
             }
           }}
           className={classNames('govuk-accordion__section', className, {
             'govuk-accordion__section--expanded': open,
           })}
      >

        <div className="govuk-accordion__section-header" ref={ (ref ) => { this.header=ref; } }>
          {createElement(
            headingTag,
            {
              className: 'govuk-accordion__section-heading',
            },


            <span className="govuk-accordion__section-button" id={headingId}>{heading}</span>

            ,
          )}
          {caption && (
            <span className="govuk-accordion__section-summary">{caption}</span>
          )}
        </div>
        <span className="govuk-accordion__drag_icon"
              onClick={(e) => {
                e.nativeEvent.preventDefault();
              }}
        />

        <div
          className="govuk-accordion__section-content"
          aria-labelledby={headingId}
          id={contentId}
        >
          {children}

          {goToTop && <GoToTopLink/>}
        </div>
      </div>
    );
  };
}

export default PrototypeEditableAccordionSection;
