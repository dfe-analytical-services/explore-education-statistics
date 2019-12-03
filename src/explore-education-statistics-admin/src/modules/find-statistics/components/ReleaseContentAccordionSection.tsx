import { EditableContentBlock } from '@admin/services/publicationService';
import React, { MouseEventHandler, ReactNode } from 'react';
import ContentBlock, {
  ReorderHook,
} from '@admin/modules/find-statistics/components/EditableContentBlock';
import AccordionSection from '@admin/components/EditableAccordionSection';
import classnames from 'classnames';
import styles from '@admin/modules/find-statistics/components/ReleaseContentAccordion.module.scss';
import { ContentType } from '@admin/modules/find-statistics/components/ReleaseContentAccordion';
import { AbstractRelease } from '@common/services/publicationService';

export interface ReleaseContentAccordionSectionProps {
  id?: string;
  contentItem: ContentType;
  index: number;
  release: AbstractRelease<EditableContentBlock>;
  headingButtons?: ReactNode[];
  canToggle?: boolean;
}

const ReleaseContentAccordionSection = ({
  id,
  index,
  contentItem,
  release,
  headingButtons,
  canToggle = true,
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
      {...restOfProps}
    >
      <ContentBlock
        isReordering={isReordering}
        canAddBlocks
        sectionId={id}
        content={content}
        id={`content_${order}`}
        publication={release.publication}
        onContentChange={newContent => setContent(newContent)}
        onReorderHook={s => {
          saveOrder.current = s;
        }}
      />
    </AccordionSection>
  );
};

export default ReleaseContentAccordionSection;
