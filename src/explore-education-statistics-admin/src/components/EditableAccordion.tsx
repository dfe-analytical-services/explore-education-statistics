import Accordion, { AccordionProps } from '@common/components/Accordion';
import Button from '@common/components/Button';
import useToggle from '@common/hooks/useToggle';
import reorder from '@common/utils/reorder';
import classNames from 'classnames';
import React, {
  cloneElement,
  isValidElement,
  ReactElement,
  useCallback,
  useEffect,
  useState,
} from 'react';
import { DragDropContext, Droppable, DropResult } from 'react-beautiful-dnd';
import wrapEditableComponent from '../hocs/wrapEditableComponent';
import styles from './EditableAccordion.module.scss';
import {
  DraggableAccordionSectionProps,
  EditableAccordionSectionProps,
} from './EditableAccordionSection';

export interface EditableAccordionProps extends AccordionProps {
  sectionName?: string;
  onAddSection: () => Promise<unknown>;
  onReorder: (sectionIds: string[]) => void;
}

const EditableAccordion = ({
  children,
  id,
  sectionName,
  onAddSection,
  onReorder,
}: EditableAccordionProps) => {
  const [isReordering, toggleReordering] = useToggle(false);

  const [sections, setSections] = useState<
    ReactElement<EditableAccordionSectionProps>[]
  >([]);

  useEffect(() => {
    const nextSections = React.Children.toArray(children).filter(
      isValidElement,
    ) as ReactElement<EditableAccordionSectionProps>[];

    setSections(nextSections);
  }, [children, id, isReordering]);

  const saveOrder = useCallback(async () => {
    if (onReorder) {
      await onReorder(sections.map(({ props }) => props.id));
      toggleReordering.off();
    }
  }, [onReorder, sections, toggleReordering]);

  const handleDragEnd = useCallback(
    ({ source, destination }: DropResult) => {
      if (source && destination) {
        setSections(reorder(sections, source.index, destination.index));
      }
    },
    [sections],
  );

  return (
    <>
      <h2 className="govuk-heading-l reorderable-relative">
        {sectionName}

        {sections.length > 1 &&
          (!isReordering ? (
            <Button
              variant="secondary"
              className="reorderable"
              onClick={toggleReordering.on}
            >
              Reorder <span className="govuk-visually-hidden">sections</span>
            </Button>
          ) : (
            <Button className="reorderable" onClick={saveOrder}>
              Save order
            </Button>
          ))}
      </h2>

      <DragDropContext onDragEnd={handleDragEnd}>
        <Droppable
          droppableId={id}
          isDropDisabled={!isReordering}
          type="accordion"
        >
          {(droppableProvided, snapshot) => (
            <div
              {...droppableProvided.droppableProps}
              ref={droppableProvided.innerRef}
              className={classNames({
                [styles.dragover]: snapshot.isDraggingOver && isReordering,
              })}
            >
              <Accordion id={id} openAll={isReordering ? false : undefined}>
                {sections.map((child, index) => {
                  const section = child as ReactElement<
                    EditableAccordionSectionProps &
                      DraggableAccordionSectionProps
                  >;

                  return cloneElement(section, {
                    index,
                    isReordering,
                  });
                })}
              </Accordion>
              {droppableProvided.placeholder}
            </div>
          )}
        </Droppable>
      </DragDropContext>

      <div>
        <Button
          onClick={onAddSection}
          className={styles.addSectionButton}
          disabled={isReordering}
        >
          Add new section
        </Button>
      </div>
    </>
  );
};

export default wrapEditableComponent(EditableAccordion, Accordion);
