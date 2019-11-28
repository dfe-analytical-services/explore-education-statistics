import Accordion, {EditableAccordionProps} from '@admin/components/EditableAccordion';
import AccordionSection, { EditableAccordionSectionProps } from '@admin/components/EditableAccordionSection';
import ContentBlock from '@admin/modules/find-statistics/components/EditableContentBlock';
import React, {ReactNode} from 'react';
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

interface ReleaseContentAccordionSectionProps   {
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
  const { caption, content, heading, order } = contentItem;

  const [isReordering, setIsReordering] = React.useState(false);

  return (
    <AccordionSection
      id={id}
      index={index}
      heading={heading || ''}
      caption={caption}
      canToggle={canToggle}
      headingButtons={[
        <button
          key="toggle_reordering"
          className={classnames(styles.toggleContentDragging)}
          type="button"
          onClick={e => {
            e.stopPropagation();
            e.preventDefault();
            setIsReordering(!isReordering);
          }}
        >
          {isReordering ? 'Save order' : 'Reorder Content'}
        </button>,
        ...headingButtons || []
      ]}
    >
      <ContentBlock
        isReordering={isReordering}
        canAddBlocks
        sectionId={id}
        content={content}
        id={`content_${order}`}
        publication={release.publication}
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
