import { useEditingContext } from '@admin/contexts/EditingContext';
import Accordion, { AccordionProps } from '@common/components/Accordion';
import Button from '@common/components/Button';
import ReorderableList from '@common/components/ReorderableList';
import { ReorderResult } from '@common/components/ReorderableItem';
import useToggle from '@common/hooks/useToggle';
import { OmitStrict } from '@common/types';
import reorder from '@common/utils/reorder';
import React, {
  cloneElement,
  isValidElement,
  ReactElement,
  useCallback,
  useEffect,
  useMemo,
  useState,
} from 'react';
import styles from './EditableAccordion.module.scss';
import { EditableAccordionSectionProps } from './EditableAccordionSection';

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
    ({ prevIndex, nextIndex }: ReorderResult) => {
      setSections(reorder(sections, prevIndex, nextIndex));
    },
    [sections],
  );

  const reorderableSections = useMemo(() => {
    return sections.map(child => {
      const section = child as ReactElement<EditableAccordionSectionProps>;

      return {
        id: section.props.id,
        label: cloneElement(section, {
          isReordering,
        }),
      };
    });
  }, [isReordering, sections]);

  return (
    <div className={styles.container}>
      {editingMode === 'edit' && (
        <div className="dfe-flex dfe-justify-content--space-between govuk-!-margin-bottom-3">
          <h2 className="govuk-heading-l govuk-!-margin-bottom-0">
            {sectionName}
          </h2>

          {sections.length > 1 && (
            <Button
              className="govuk-!-font-size-16 govuk-!-margin-bottom-0"
              id={`${id}-reorder`}
              variant={isReordering ? 'secondary' : undefined}
              onClick={() =>
                isReordering ? saveOrder() : toggleReordering.on()
              }
            >
              {isReordering ? (
                'Save order'
              ) : (
                <>
                  Reorder
                  <span className="govuk-visually-hidden"> sections</span>
                </>
              )}
            </Button>
          )}
        </div>
      )}

      {isReordering ? (
        <ReorderableList
          id="reorder-sections"
          list={reorderableSections}
          onMoveItem={handleDragEnd}
        />
      ) : (
        <Accordion {...props}>{sections}</Accordion>
      )}

      {editingMode === 'edit' && (
        <div>
          <Button
            onClick={onAddSection}
            className={styles.addSectionButton}
            disabled={isReordering}
            id="editable-accordion-add-section-button"
          >
            Add new section
          </Button>
        </div>
      )}
    </div>
  );
};

export default EditableAccordion;
