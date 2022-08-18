import { useEditingContext } from '@admin/contexts/EditingContext';
import Accordion, { AccordionProps } from '@common/components/Accordion';
import Button from '@common/components/Button';
import useToggle from '@common/hooks/useToggle';
import { OmitStrict } from '@common/types';
import reorder from '@common/utils/reorder';
import classNames from 'classnames';
import React, {
  cloneElement,
  isValidElement,
  ReactElement,
  useCallback,
  useEffect,
  useMemo,
  useState,
} from 'react';
import { DragDropContext, Droppable, DropResult } from 'react-beautiful-dnd';
import styles from './EditableAccordion.module.scss';
import {
  DraggableAccordionSectionProps,
  EditableAccordionSectionProps,
} from './EditableAccordionSection';

export interface EditableAccordionProps
  extends OmitStrict<AccordionProps, 'openAll'> {
  sectionName?: string;
  onAddSection: () => void;
  onReorder: (sectionIds: string[]) => void;
}

const EditableAccordion = (props: EditableAccordionProps) => {
  const { children, id, sectionName, onAddSection, onReorder } = props;

  const { editingMode } = useEditingContext();
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
      await onReorder(sections.map(section => section.props.id));
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

  const accordion = useMemo(() => {
    return (
      <Accordion {...props} openAll={isReordering ? false : undefined}>
        {sections.map((child, index) => {
          const section = child as ReactElement<
            EditableAccordionSectionProps & DraggableAccordionSectionProps
          >;

          return cloneElement(section, {
            index,
            isReordering,
          });
        })}
      </Accordion>
    );
  }, [isReordering, props, sections]);

  if (editingMode !== 'edit') {
    return <Accordion {...props}>{children}</Accordion>;
  }

  return (
    <div className={styles.container}>
      <div className="dfe-flex dfe-justify-content--space-between govuk-!-margin-bottom-3">
        <h2 className="govuk-heading-l govuk-!-margin-bottom-0">
          {sectionName}
        </h2>

        {sections.length > 1 &&
          (!isReordering ? (
            <Button
              variant="secondary"
              className="govuk-!-font-size-16 govuk-!-margin-bottom-0"
              id={`${id}-reorder`}
              onClick={toggleReordering.on}
            >
              Reorder<span className="govuk-visually-hidden"> sections</span>
            </Button>
          ) : (
            <Button
              className="govuk-!-font-size-16 govuk-!-margin-bottom-0"
              onClick={saveOrder}
            >
              Save order
            </Button>
          ))}
      </div>

      <DragDropContext onDragEnd={handleDragEnd}>
        <Droppable
          droppableId={id}
          isDropDisabled={!isReordering}
          type="accordion"
        >
          {(droppableProvided, snapshot) => (
            <div
              // eslint-disable-next-line react/jsx-props-no-spreading
              {...droppableProvided.droppableProps}
              ref={droppableProvided.innerRef}
              className={classNames({
                [styles.dragover]: snapshot.isDraggingOver && isReordering,
              })}
            >
              {accordion}
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
    </div>
  );
};

export default EditableAccordion;
