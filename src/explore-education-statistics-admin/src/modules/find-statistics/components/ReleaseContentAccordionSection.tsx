import { EditableContentBlock } from '@admin/services/publicationService';
import React, { MouseEventHandler, ReactNode } from 'react';
import ContentBlock, {
  ReorderHook,
} from '@admin/modules/find-statistics/components/EditableContentBlocks';
import AccordionSection, {
  EditableAccordionSectionProps,
} from '@admin/components/EditableAccordionSection';
import classnames from 'classnames';
import styles from '@admin/modules/find-statistics/components/ReleaseContentAccordion.module.scss';
import { ContentType } from '@admin/modules/find-statistics/components/ReleaseContentAccordion';
import { AbstractRelease } from '@common/services/publicationService';

export interface ReleaseContentAccordionSectionProps {
  id?: string;
  contentItem: ContentType;
  index: number;
  publication: AbstractRelease<EditableContentBlock>['publication'];
  headingButtons?: ReactNode[];
  onHeadingChange?: EditableAccordionSectionProps['onHeadingChange'];
  onContentChange?: (content?: EditableContentBlock[]) => void;
  canToggle?: boolean;
}

const ReleaseContentAccordionSection = ({
  id,
  index,
  contentItem,
  publication,
  headingButtons,
  canToggle = true,
  onHeadingChange,
  onContentChange,
  ...restOfProps
}: ReleaseContentAccordionSectionProps) => {
  const { caption, heading, order } = contentItem;

  const [isReordering, setIsReordering] = React.useState(false);

  const [content, setContent] = React.useState(contentItem.content);

  const saveOrder = React.useRef<ReorderHook>();

  const onReorderClick: MouseEventHandler = e => {
    e.stopPropagation();
    e.preventDefault();

    if (isReordering) {
      if (saveOrder.current) {
        saveOrder.current(contentItem.id).then(() => setIsReordering(false));
      } else {
        setIsReordering(false);
      }
    } else {
      setIsReordering(true);
    }
  };

  const contentChange = React.useCallback(
    (newContent?: EditableContentBlock[]) => {
      setContent(newContent);
      if (onContentChange) {
        onContentChange(newContent);
      }
    },
    [onContentChange],
  );

  return (
    <AccordionSection
      id={id}
      index={index}
      heading={heading || ''}
      caption={caption}
      canToggle={canToggle}
      headingButtons={[
        content && content.length > 1 && (
          <button
            key="toggle_reordering"
            className={classnames(styles.toggleContentDragging)}
            type="button"
            onClick={onReorderClick}
          >
            {isReordering ? 'Save order' : 'Reorder Content'}
          </button>
        ),
        ...(headingButtons || []),
      ]}
      canEditHeading
      onHeadingChange={onHeadingChange}
      {...restOfProps}
    >
      <ContentBlock
        isReordering={isReordering}
        canAddBlocks
        sectionId={id}
        content={content}
        id={`content_${order}`}
        publication={publication}
        onContentChange={newContent => contentChange(newContent)}
        onReorderHook={s => {
          saveOrder.current = s;
        }}
      />
    </AccordionSection>
  );
};

export default ReleaseContentAccordionSection;
