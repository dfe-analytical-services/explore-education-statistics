import Accordion from '@admin/components/EditableAccordion';
import AccordionSection from '@admin/components/EditableAccordionSection';
import ContentBlock, {
  ReorderHook,
} from '@admin/modules/find-statistics/components/EditableContentBlock';
import React, { MouseEventHandler, ReactNode } from 'react';
import { AbstractRelease } from '@common/services/publicationService';
import { EditableContentBlock } from '@admin/services/publicationService';
import releaseContentService from '@admin/services/release/edit-release/content/service';
import { Dictionary } from '@common/types/util';
import classnames from 'classnames';
import styles from './ReleaseContentAccordion.module.scss';

type ContentType = AbstractRelease<EditableContentBlock>['content'][0];

interface ReleaseContentAccordionProps {
  release: AbstractRelease<EditableContentBlock>;
  content: ContentType[];
  accordionId: string;
  sectionName: string;
}

interface ReleaseContentAccordionSectionProps {
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
        content.length > 1 && (
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

const ReleaseContentAccordion = ({
  release,
  accordionId,
  sectionName,
}: ReleaseContentAccordionProps) => {
  const [content, setContent] = React.useState<ContentType[]>([]);

  const onReorder = async (ids: Dictionary<number>) => {
    return releaseContentService.updateContentSectionsOrder(release.id, ids);
  };

  const updateContent = async (id: string) => {
    setContent(await releaseContentService.getContentSections(id));
  };

  React.useEffect(() => {
    updateContent(release.id);
  }, [release.id]);

  return (
    <>
      {content && content.length > 0 && (
        <Accordion
          id={accordionId}
          canReorder
          sectionName={sectionName}
          onSaveOrder={onReorder}
        >
          {content.map((contentItem, index) => (
            <ReleaseContentAccordionSection
              id={contentItem.id}
              key={contentItem.order}
              contentItem={contentItem}
              index={index}
              release={release}
            />
          ))}
        </Accordion>
      )}
    </>
  );
};

export default ReleaseContentAccordion;
