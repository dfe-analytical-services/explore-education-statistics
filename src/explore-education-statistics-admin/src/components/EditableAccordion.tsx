/* eslint-disable no-shadow, react/jsx-indent */
import Accordion, { AccordionProps } from '@common/components/Accordion';
import wrapEditableComponent from '@common/modules/find-statistics/util/wrapEditableComponent';
import classnames from 'classnames';
import React, { createRef, ReactElement, ReactNode } from 'react';
import { DragDropContext, DropResult } from 'react-beautiful-dnd';
import { Dictionary } from '@common/types/util';
import DroppableAccordion from '@admin/components/DroppableAccordion';
import DraggableAccordionSection from '@admin/components/DraggableAccordionSection';
import styles from './EditableAccordion.module.scss';
import { EditableAccordionSectionProps } from './EditableAccordionSection';

export interface EditableAccordionProps extends AccordionProps {
  canReorder?: boolean;
  sectionName?: string;
  onSaveOrder: (order: Dictionary<number>) => Promise<unknown>;
  onAddSection: () => Promise<unknown>;
}

interface ChildSection {
  id: string;
  key: string;
  index: number;
  section: ReactElement<EditableAccordionSectionProps>;
}

const mapReactNodeToChildSection = (children: ReactNode): ChildSection[] =>
  React.Children.map(children, (child, thisIndex) => {
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

const EditableAccordion = ({
  children,
  id,
  canReorder,
  sectionName,
  onSaveOrder,
  onAddSection,
}: EditableAccordionProps) => {
  const ref = createRef<HTMLDivElement>();
  const [openAll, setOpenAll] = React.useState(false);
  const [isReordering, setIsReordering] = React.useState(false);
  const [isError, setIsError] = React.useState(false);

  const [currentChildren, setCurrentChildren] = React.useState<ChildSection[]>(
    [],
  );

  React.useEffect(() => {
    setCurrentChildren(mapReactNodeToChildSection(children));
  }, [children]);

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
    setIsError(false);
    setIsReordering(true);
  };

  const saveOrder = () => {
    if (onSaveOrder) {
      const ids = currentChildren.reduce<Dictionary<number>>(
        (result, { id, index }) => ({ ...result, [id]: index }),
        {},
      );

      onSaveOrder(ids)
        .then(() => {
          setIsReordering(false);
        })
        .catch(() => {
          setIsError(true);
        });
    }
  };

  const onDragEnd = (result: DropResult) => {
    const { source, destination } = result;

    if (source && destination) {
      const newChildren = [...currentChildren];
      const [removed] = newChildren.splice(source.index, 1);
      newChildren.splice(destination.index, 0, removed);

      const reordered = newChildren.map((child, index) => ({
        ...child,
        index,
      }));

      setCurrentChildren(reordered);
    }
  };

  const addSection = () => {
    if (onAddSection) onAddSection().then(() => {});
  };

  return (
    <DragDropContext onDragEnd={onDragEnd}>
      <h2 className="govuk-heading-l reorderable-relative">
        {isError && <span className={styles.error}>An error occurred</span>}
        {canReorder &&
          currentChildren.length > 1 &&
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

      {canReorder && (
        <div className="govuk-accordion" style={{ border: 'none' }}>
          <div className={classnames('govuk-accordion__controls')}>
            <button
              type="button"
              key="add_section"
              onClick={() => addSection()}
              className={classnames(styles.addSectionButton, 'govuk-button')}
            >
              Add new section
            </button>
          </div>
        </div>
      )}
    </DragDropContext>
  );
};

export default wrapEditableComponent(EditableAccordion, Accordion);
