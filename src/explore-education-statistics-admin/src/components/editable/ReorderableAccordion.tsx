import Accordion, { AccordionProps } from '@common/components/Accordion';
import Button from '@common/components/Button';
import VisuallyHidden from '@common/components/VisuallyHidden';
import { ReorderResult } from '@common/components/ReorderableItem';
import ReorderableList from '@common/components/ReorderableList';
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
import styles from './ReorderableAccordion.module.scss';
import { ReorderableAccordionSectionProps } from './ReorderableAccordionSection';

export interface ReorderableAccordionProps
  extends OmitStrict<AccordionProps, 'openAll'> {
  heading?: string;
  onReorder: (sectionIds: string[]) => void;
  canReorder: boolean;
  reorderHiddenText?: string;
}

export default function ReorderableAccordion(props: ReorderableAccordionProps) {
  const {
    children,
    id,
    heading,
    onReorder,
    canReorder,
    reorderHiddenText = 'sections',
  } = props;

  const [isReordering, toggleIsReordering] = useToggle(false);

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
      await onReorder(sections.map(section => section.props.id));
      toggleIsReordering.off();
    }
  }, [onReorder, sections, toggleIsReordering]);

  const handleDragEnd = useCallback(
    ({ prevIndex, nextIndex }: ReorderResult) => {
      setSections(reorder(sections, prevIndex, nextIndex));
    },
    [sections],
  );

  const reorderableSections = useMemo(() => {
    return sections.map(child => {
      const section = child as ReactElement<ReorderableAccordionSectionProps>;

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
      <div className="dfe-flex dfe-justify-content--space-between govuk-!-margin-bottom-3">
        <h2 className="govuk-heading-l govuk-!-margin-bottom-0">{heading}</h2>

        {sections.length > 1 && canReorder && (
          <Button
            className="govuk-!-font-size-16 govuk-!-margin-bottom-0"
            variant={isReordering ? 'secondary' : undefined}
            onClick={() =>
              isReordering ? saveOrder() : toggleIsReordering.on()
            }
          >
            {isReordering ? (
              'Save order'
            ) : (
              <>
                Reorder
                <VisuallyHidden> {reorderHiddenText}</VisuallyHidden>
              </>
            )}
          </Button>
        )}
      </div>

      {isReordering ? (
        <ReorderableList
          id={id}
          list={reorderableSections}
          onMoveItem={handleDragEnd}
        />
      ) : (
        <Accordion {...props}>{sections}</Accordion>
      )}
    </div>
  );
}
