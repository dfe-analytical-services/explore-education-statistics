import styles from '@admin/components/editable/ReorderableAccordionSection.module.scss';
import AccordionSection, {
  accordionSectionClasses,
  AccordionSectionProps,
} from '@common/components/AccordionSection';
import classNames from 'classnames';
import React, { createElement, ReactNode, useMemo } from 'react';
import { Draggable } from 'react-beautiful-dnd';

export interface DraggableAccordionSectionProps {
  index: number;
  isReordering: boolean;
}

export interface ReorderableAccordionSectionProps
  extends AccordionSectionProps {
  id: string;
  onRemoveSection?: () => void;
}

const ReorderableAccordionSection = (
  props: ReorderableAccordionSectionProps,
) => {
  const {
    children,
    heading,
    headingTag = 'h2',
    id,
    onRemoveSection,
    ...restProps
  } = props;

  const { index, isReordering } = restProps as DraggableAccordionSectionProps;

  const header: ReactNode = useMemo(() => {
    if (isReordering) {
      return createElement(
        headingTag,
        {
          className: accordionSectionClasses.sectionHeading,
        },
        heading,
      );
    }

    return undefined;
  }, [heading, headingTag, isReordering]);

  if (!isReordering) {
    return <AccordionSection {...props} />;
  }

  return (
    <Draggable draggableId={id} isDragDisabled={!isReordering} index={index}>
      {(draggableProvided, snapshot) => (
        <div
          // eslint-disable-next-line react/jsx-props-no-spreading
          {...draggableProvided.draggableProps}
          // eslint-disable-next-line react/jsx-props-no-spreading
          {...draggableProvided.dragHandleProps}
          ref={draggableProvided.innerRef}
          className={classNames({
            [styles.dragContainer]: isReordering,
            [styles.isDragging]: snapshot.isDragging,
          })}
          data-testid="reorderableAccordionSection"
        >
          <AccordionSection
            {...props}
            id={id}
            heading={heading}
            header={header}
          >
            {sectionProps => (
              <>
                {typeof children === 'function'
                  ? children(sectionProps)
                  : children}
              </>
            )}
          </AccordionSection>
        </div>
      )}
    </Draggable>
  );
};

export default ReorderableAccordionSection;
