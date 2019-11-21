/* eslint-disable no-shadow, react/jsx-indent */
import Accordion, { AccordionProps } from '@common/components/Accordion';
import wrapEditableComponent from '@common/modules/find-statistics/util/wrapEditableComponent';
import classnames from 'classnames';
import React, { cloneElement, createRef, ReactElement, ReactNode } from 'react';
import {
  DragDropContext,
  Draggable,
  Droppable,
  DropResult,
} from 'react-beautiful-dnd';

import styles from './EditableAccordion.module.scss';
import { EditableAccordionSectionProps } from './EditableAccordionSection';

export interface EditableAccordionProps extends AccordionProps {
  releaseId: string;
  index?: number;
  canReorder?: boolean;
  sectionName?: string;
}

interface DroppableAccordionProps {
  id: string;
  isReordering: boolean;
  children: ReactNode;
}

const DroppableAccordion = ({
  id,
  isReordering,
  children,
}: DroppableAccordionProps) => {
  if (isReordering) {
    return (
      <Droppable droppableId={id} type="accordion">
        {(droppableProvided, snapshot) => (
          <div
            {...droppableProvided.droppableProps}
            ref={droppableProvided.innerRef}
            style={{ backgroundColor: snapshot.isDraggingOver ? 'blue' : '' }}
          >
            {children}

            {droppableProvided.placeholder}
          </div>
        )}
      </Droppable>
    );
  }

  return <>{children}</>;
};

interface DraggableAccordionSectionProps {
  id: string;
  isReordering: boolean;
  index: number;
  openAll: boolean;
  section: ReactElement<EditableAccordionSectionProps>;
}

const DraggableAccordionSection = ({
  id,
  isReordering,
  index,
  openAll,
  section,
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
              open: false,
              canToggle: false,
              headingButtons: (
                <span
                  className={styles.dragHandle}
                  {...draggableProvided.dragHandleProps}
                />
              ),
            })}
          </div>
        )}
      </Draggable>
    );
  }

  return cloneElement<EditableAccordionSectionProps>(section, {
    index,
    open: openAll,
  });
};

interface ChildSection {
  id: string;
  key: string;
  index: number;
  section: ReactElement<EditableAccordionSectionProps>;
}

const EditableAccordion = ({
  children,
  id,
  index,
  canReorder,
  sectionName,
  releaseId,
}: EditableAccordionProps) => {
  const ref = createRef<HTMLDivElement>();
  const [openAll, setOpenAll] = React.useState(false);
  const [isReordering, setIsReordering] = React.useState(false);

  const [currentChildren, setCurrentChildren] = React.useState<ChildSection[]>(
    () => {
      return React.Children.map(children, (child, thisIndex) => {
        if (child) {
          const section = child as ReactElement<EditableAccordionSectionProps>;

          const key = section.props.id || `unknown_section_id_${thisIndex}`;

          return {
            id: key,
            key,
            index: thisIndex,
            section,
          };
        }

        return undefined;
      }).filter(item => !!item) as ChildSection[];
    },
  );

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
  }, [ref]);

  React.useEffect(() => {
    window.addEventListener('hashchange', goToHash);

    return () => {
      window.removeEventListener('hashchange', goToHash);
    };
  }, [goToHash]);

  const toggleAll = () => {
    setOpenAll(!openAll);
  };

  const reorder = () => {
    setIsReordering(true);
  };
  const saveOrder = () => {
    setIsReordering(false);

    const ids = currentChildren.map(({ id }) => id);
  };

  const onDragEnd = (result: DropResult) => {
    console.log(result);
    const movedSection = currentChildren.find(
      ({ id }) => id === result.draggableId,
    );

    if (movedSection && result.source && result.destination) {
      const newChildren = [...currentChildren];
      const [removed] = newChildren.splice(result.source.index, 1);
      newChildren.splice(result.destination.index, 0, removed);
      setCurrentChildren(newChildren);
    }
  };

  return (
    <DragDropContext onDragEnd={onDragEnd}>
      <h2 className="govuk-heading-l reorderable-relative">
        {canReorder &&
          (isReordering ? (
            <button
              className="govuk-button reorderable"
              onClick={() => saveOrder()}
              type="button"
            >
              Save Reordering
            </button>
          ) : (
            <button
              className="govuk-button govuk-button--secondary reorderable"
              onClick={() => reorder()}
              type="button"
            >
              Reorder <span className="govuk-visually-hidden"> sections </span>
            </button>
          ))}
        {sectionName}
      </h2>
      <DroppableAccordion id={id} isReordering={isReordering}>
        <div className="govuk-accordion" ref={ref} id={id}>
          <div className={classnames('govuk-accordion__controls')}>
            <button
              type="button"
              className="govuk-accordion__open-all"
              aria-expanded="false"
              onClick={() => toggleAll()}
            >
              Open all<span className="govuk-visually-hidden"> sections</span>
            </button>
          </div>
          {currentChildren.map(({ id, key, index, section }) => (
            <DraggableAccordionSection
              id={id}
              key={key}
              index={index}
              isReordering={isReordering}
              section={section}
              openAll={openAll}
            />
          ))}
        </div>
      </DroppableAccordion>
    </DragDropContext>
  );
};

export default wrapEditableComponent(EditableAccordion, Accordion);
