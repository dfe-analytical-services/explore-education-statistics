import styles from '@admin/components/editable/ReorderableAccordionSection.module.scss';
import DraggableItem from '@admin/components/DraggableItem';
import AccordionSection, {
  accordionSectionClasses,
  AccordionSectionProps,
} from '@common/components/AccordionSection';
import classNames from 'classnames';
import React, { createElement, ReactNode, useMemo } from 'react';

export interface DraggableAccordionSectionProps {
  index: number;
  isReordering: boolean;
}

export interface ReorderableAccordionSectionProps
  extends AccordionSectionProps {
  id: string;
}

const ReorderableAccordionSection = (
  props: ReorderableAccordionSectionProps,
) => {
  const { children, heading, headingTag = 'h2', id, ...restProps } = props;

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
    <DraggableItem
      className={classNames({ [styles.draggableItem]: isReordering })}
      id={id}
      index={index}
      isReordering={isReordering}
    >
      <AccordionSection {...props} id={id} heading={heading} header={header}>
        {sectionProps => (
          <>
            {typeof children === 'function' ? children(sectionProps) : children}
          </>
        )}
      </AccordionSection>
    </DraggableItem>
  );
};

export default ReorderableAccordionSection;
