import Accordion, { AccordionProps } from '@common/components/Accordion';
import Button from '@common/components/Button';
import VisuallyHidden from '@common/components/VisuallyHidden';
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
import styles from './ReorderableAccordion.module.scss';
import {
  DraggableAccordionSectionProps,
  ReorderableAccordionSectionProps,
} from './ReorderableAccordionSection';

export interface ReorderableAccordionProps
  extends OmitStrict<AccordionProps, 'openAll'> {
  heading?: string;
  onReorder: (sectionIds: string[]) => void;
  canUpdateRelease: boolean;
}

const ReorderableAccordion = (props: ReorderableAccordionProps) => {
  const { children, id, heading, onReorder, canUpdateRelease } = props;

  const [isReordering, toggleReordering] = useToggle(false);

  const [sections, setSections] = useState<
    ReactElement<ReorderableAccordionSectionProps>[]
  >([]);

  useEffect(() => {
    const nextSections = React.Children.toArray(children).filter(
      isValidElement,
    ) as ReactElement<ReorderableAccordionSectionProps>[];

    setSections(nextSections);
  }, [children, id, isReordering]);

  const saveOrder = useCallback(async () => {
    if (onReorder) {
      onReorder(sections.map(section => section.props.id));
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
      <Accordion {...props} openAll={isReordering ?? false}>
        {sections.map((child, index) => {
          const section = child as ReactElement<
            ReorderableAccordionSectionProps & DraggableAccordionSectionProps
          >;

          return cloneElement(section, {
            index,
            isReordering,
          });
        })}
      </Accordion>
    );
  }, [isReordering, props, sections]);

  return (
    <div className={styles.container}>
      <div className="dfe-flex dfe-justify-content--space-between govuk-!-margin-bottom-3">
        <h2 className="govuk-heading-l govuk-!-margin-bottom-0">{heading}</h2>

        {sections.length > 1 &&
          canUpdateRelease &&
          (!isReordering ? (
            <Button
              variant="secondary"
              className="govuk-!-font-size-16 govuk-!-margin-bottom-0"
              onClick={toggleReordering.on}
            >
              Reorder<VisuallyHidden> data files</VisuallyHidden>
            </Button>
          ) : (
            <>
              <Button
                className="govuk-!-font-size-16 govuk-!-margin-bottom-0"
                onClick={saveOrder}
              >
                Save order
              </Button>
            </>
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
    </div>
  );
};

export default ReorderableAccordion;
