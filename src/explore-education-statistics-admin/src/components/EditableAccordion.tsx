import Accordion, {AccordionProps} from '@common/components/Accordion';
import isComponentType from '@common/lib/type-guards/components/isComponentType';
import React, {cloneElement, ComponentClass, createRef, ReactElement, ReactNode} from 'react';
import wrapEditableComponent from '@common/modules/find-statistics/util/wrapEditableComponent';
import {DragDropContext, Draggable, Droppable, DropResult} from "react-beautiful-dnd";
import EditableAccordionSection, {EditableAccordionSectionProps,} from './EditableAccordionSection';

import styles from './EditableAccordion.module.scss';

export interface EditableAccordionProps extends AccordionProps {
  index?: number;
  canReorder?: boolean;
}

interface State {
  openAll: boolean;
}

interface DroppableAccordionProps {
  id: string,
  isReordering: boolean,
  children: ReactNode
}

const DroppableAccordion = ({
  id,
  isReordering,
  children
}: DroppableAccordionProps) => {

  if (isReordering) {
    return (
      <Droppable droppableId={id} type="accordion">
        {droppableProvided => (
          <div
            {...droppableProvided.droppableProps}
            ref={droppableProvided.innerRef}
          >
            {children}

            {droppableProvided.placeholder}
          </div>


        )}
      </Droppable>
    );
  }

  return <>{children}</>

};

interface DraggableAccordionSectionProps {
  id: string,
  isReordering: boolean,
  index: number,
  openAll: boolean,
  section: ReactElement<EditableAccordionSectionProps>
}

const DraggableAccordionSection = ({
  id,
  isReordering,
  index,
  openAll,
  section
}: DraggableAccordionSectionProps) => {

  if (isReordering) {
    return (
      <Draggable draggableId={id} type="section" index={index}>
        {draggableProvided => (
          <div
            {...draggableProvided.draggableProps}
            ref={draggableProvided.innerRef}
            className={styles.dragContainer}
          >
            {cloneElement<EditableAccordionSectionProps>(section, {
              index,
              droppableIndex: index,
              open: openAll,
              headingButtons: (
                <span
                  className={styles.dragHandle}
                  {...draggableProvided.dragHandleProps}
                />
              )
            })}
          </div>
        )}
      </Draggable>
    );
  }

  return cloneElement<EditableAccordionSectionProps>(section, {
    index,
    droppableIndex: index,
    open: openAll,
  });
};

const EditableAccordion = ({
  children,
  id,
  index,
  canReorder
}: EditableAccordionProps) => {
  const ref = createRef<HTMLDivElement>();
  const [openAll, setOpenAll] = React.useState(false);
  const [isReordering, setIsReordering] = React.useState(false);

  const goToHash = React.useCallback(() => {
    if (ref.current && window.location.hash) {
      try {
        const anchor = ref.current.querySelector(
          window.location.hash,
        ) as HTMLButtonElement;

        if (anchor) {
          anchor.scrollIntoView();
        }
      } catch (_) {
        // ignoring any errors
      }
    }
  }, [ref])

  React.useEffect(() => {
    window.addEventListener('hashchange', goToHash);

    return () => {
      window.removeEventListener('hashchange', goToHash);
    }
  }, [goToHash]);


  const toggleAll = () => {
    setOpenAll(!openAll);
  };

  const reorder = () => {
    setIsReordering(true);
  };

  const onDragEnd = (result: DropResult) => {

  };

  return (
    <DragDropContext onDragEnd={onDragEnd}>
      <DroppableAccordion id={id} isReordering={isReordering}>
        <div className="govuk-accordion" ref={ref} id={id}>
          <div className="govuk-accordion__controls">
            {canReorder && (
              <button
                type="button"
                aria-expanded="false"
                onClick={() => reorder()}
              >
                Reorder<span className="govuk-visually-hidden"> sections</span>
              </button>

            )}
            <button
              type="button"
              className="govuk-accordion__open-all"
              aria-expanded="false"
              onClick={() => toggleAll()}
            >
              Open all<span className="govuk-visually-hidden"> sections</span>
            </button>
          </div>
          {React.Children.map(children, (child, thisIndex) => {

            if (child) {
              const section = child as ReactElement<EditableAccordionSectionProps>;

              const key = `section_${thisIndex}`;

              return (
                <DraggableAccordionSection
                  id={key}
                  key={key}
                  index={thisIndex}
                  isReordering={isReordering}
                  section={section}
                  openAll={openAll}
                />
              );
            }

            return undefined;
          })}
        </div>
      </DroppableAccordion>
    </DragDropContext>
  );
};

export default wrapEditableComponent(EditableAccordion, Accordion);
