import styles from '@admin/components/EditableAccordionSection.module.scss';
import AccordionSection, {
  accordionSectionClasses,
  AccordionSectionProps,
} from '@common/components/AccordionSection';
import Button from '@common/components/Button';
import { FormTextInput } from '@common/components/form';
import ModalConfirm from '@common/components/ModalConfirm';
import useToggle from '@common/hooks/useToggle';
import classNames from 'classnames';
import React, {
  createElement,
  ReactNode,
  useCallback,
  useMemo,
  useState,
} from 'react';
import { Draggable } from 'react-beautiful-dnd';
import wrapEditableComponent from '../hocs/wrapEditableComponent';

export interface DraggableAccordionSectionProps {
  index: number;
  isReordering: boolean;
}

export interface EditableAccordionSectionProps extends AccordionSectionProps {
  id: string;
  headerButtons?: ReactNode;
  onHeadingChange: (heading: string) => void;
  onRemoveSection?: () => void;
}

const EditableAccordionSection = ({
  children,
  heading,
  id,
  onHeadingChange,
  onRemoveSection,
  headerButtons,
  headingTag = 'h2',
  ...props
}: EditableAccordionSectionProps) => {
  const { index, isReordering } = props as DraggableAccordionSectionProps;

  const [showRemoveModal, toggleRemoveModal] = useToggle(false);
  const [isEditingHeading, toggleEditingHeading] = useToggle(false);

  const [newHeading, setNewHeading] = useState(heading);

  const saveHeading = useCallback(async () => {
    if (isEditingHeading && onHeadingChange && newHeading !== heading) {
      await onHeadingChange(newHeading);
    }

    toggleEditingHeading.off();
  }, [
    heading,
    isEditingHeading,
    newHeading,
    onHeadingChange,
    toggleEditingHeading,
  ]);

  const header = useMemo(() => {
    if (isEditingHeading) {
      return (
        <FormTextInput
          id="heading"
          name="heading"
          label="Edit Heading"
          defaultValue={newHeading}
          onChange={e => {
            setNewHeading(e.target.value);
          }}
          onClick={e => {
            e.stopPropagation();
          }}
          onKeyPress={async e => {
            switch (e.key) {
              case 'Enter':
                await saveHeading();
                break;
              case 'Esc':
                toggleEditingHeading.off();
                break;
              default:
                break;
            }
          }}
        />
      );
    }

    if (isReordering) {
      return createElement(
        headingTag,
        {
          className: accordionSectionClasses.sectionHeading,
        },
        <span className={accordionSectionClasses.sectionButton}>
          {heading}
        </span>,
      );
    }

    return undefined;
  }, [
    heading,
    headingTag,
    isEditingHeading,
    isReordering,
    newHeading,
    saveHeading,
    toggleEditingHeading,
  ]);

  return (
    <Draggable draggableId={id} isDragDisabled={!isReordering} index={index}>
      {draggableProvided => (
        <div
          {...draggableProvided.draggableProps}
          ref={draggableProvided.innerRef}
          className={classNames({
            [styles.dragContainer]: isReordering,
          })}
        >
          <span
            {...draggableProvided.dragHandleProps}
            className={classNames({
              [styles.dragHandle]: isReordering,
            })}
          />
          <AccordionSection
            {...props}
            id={id}
            heading={heading}
            header={header}
          >
            <div>
              {onHeadingChange &&
                (isEditingHeading ? (
                  <Button onClick={saveHeading}>Save section title</Button>
                ) : (
                  <Button
                    type="button"
                    onClick={toggleEditingHeading}
                    variant="secondary"
                  >
                    Edit section title
                  </Button>
                ))}

              {headerButtons}

              {onRemoveSection && (
                <>
                  <Button onClick={toggleRemoveModal.on} variant="warning">
                    Remove this section
                  </Button>

                  <ModalConfirm
                    title="Are you sure?"
                    mounted={showRemoveModal}
                    onConfirm={onRemoveSection}
                    onExit={toggleRemoveModal.off}
                    onCancel={toggleRemoveModal.off}
                  >
                    <p>
                      Are you sure you want to remove the following section?
                      <br />
                      <strong>"{heading}"</strong>
                    </p>
                  </ModalConfirm>
                </>
              )}
            </div>

            {children}
          </AccordionSection>
        </div>
      )}
    </Draggable>
  );
};

export default wrapEditableComponent(
  EditableAccordionSection,
  AccordionSection,
);
