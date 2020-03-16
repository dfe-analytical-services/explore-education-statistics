/* eslint-disable no-shadow, react/jsx-indent */
import DraggableAccordionSection from '@admin/components/DraggableAccordionSection';
import DroppableAccordion from '@admin/components/DroppableAccordion';
import Accordion, { AccordionProps } from '@common/components/Accordion';
import Button from '@common/components/Button';
import ButtonText from '@common/components/ButtonText';
import wrapEditableComponent from '@common/modules/find-statistics/util/wrapEditableComponent';
import { Dictionary } from '@common/types/util';
import classnames from 'classnames';
import React, {
  createRef,
  ReactElement,
  ReactNode,
  useCallback,
  useEffect,
  useState,
} from 'react';
import { DragDropContext, DropResult } from 'react-beautiful-dnd';
import styles from './EditableAccordion.module.scss';
import { EditableAccordionSectionProps } from './EditableAccordionSection';

export interface EditableAccordionProps extends AccordionProps {
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
  React.Children.toArray(children)
    .filter(child => !!child)
    .map((child, index) => {
      const section = child as ReactElement<EditableAccordionSectionProps>;

      const key = section.props.id || `unknown_section_id_${index}`;

      return {
        id: key,
        key,
        index,
        section,
      };
    });

const EditableAccordion = ({
  children,
  id,

  sectionName,
  onSaveOrder,
  onAddSection,
}: EditableAccordionProps) => {
  const ref = createRef<HTMLDivElement>();
  const [openAll, setOpenAll] = useState(false);
  const [isReordering, setIsReordering] = useState(false);
  const [isError, setIsError] = useState(false);

  const [currentChildren, setCurrentChildren] = useState<ChildSection[]>([]);

  useEffect(() => {
    setCurrentChildren(mapReactNodeToChildSection(children));
  }, [children]);

  const goToHash = useCallback(() => {
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

  useEffect(() => {
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

  return (
    <DragDropContext onDragEnd={onDragEnd}>
      <h2 className="govuk-heading-l reorderable-relative">
        {sectionName}
        {isError && <span className={styles.error}>An error occurred</span>}
        {currentChildren.length > 1 &&
          (!isReordering ? (
            <Button
              variant="secondary"
              className="reorderable"
              onClick={reorder}
            >
              Reorder <span className="govuk-visually-hidden"> sections </span>
            </Button>
          ) : (
            <Button className="reorderable" onClick={saveOrder}>
              Save order
            </Button>
          ))}
      </h2>
      <DroppableAccordion id={id} isReordering={isReordering}>
        <div className="govuk-accordion" ref={ref} id={id}>
          {!isReordering && (
            <div className="govuk-accordion__controls">
              <ButtonText
                className="govuk-accordion__open-all"
                aria-expanded="false"
                onClick={() => toggleAll()}
              >
                Open all<span className="govuk-visually-hidden"> sections</span>
              </ButtonText>
            </div>
          )}
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

      <div className="govuk-accordion" style={{ border: 'none' }}>
        <div className={classnames('govuk-accordion__controls')}>
          <Button
            key="add_section"
            onClick={onAddSection}
            className={styles.addSectionButton}
          >
            Add new section
          </Button>
        </div>
      </div>
    </DragDropContext>
  );
};

export default wrapEditableComponent(EditableAccordion, Accordion);
