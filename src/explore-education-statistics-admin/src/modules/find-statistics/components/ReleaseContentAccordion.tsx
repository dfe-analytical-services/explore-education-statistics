import Accordion from '@admin/components/EditableAccordion';
import { EditableContentBlock } from '@admin/services/publicationService';
import { releaseContentService } from '@admin/services/release/edit-release/content/service';
import withErrorControl, {
  ErrorControlProps,
} from '@admin/validation/withErrorControl';
import { AbstractRelease } from '@common/services/publicationService';
import { Dictionary } from '@common/types/util';
import React from 'react';
import ReleaseContentAccordionSection from './ReleaseContentAccordionSection';

export type ContentType = AbstractRelease<EditableContentBlock>['content'][0];

interface ReleaseContentAccordionProps {
  releaseId: string;
  publication: AbstractRelease<EditableContentBlock>['publication'];
  accordionId: string;
  sectionName: string;
  onContentChange?: (content: ContentType[]) => void;
}

const ReleaseContentAccordion = ({
  releaseId,
  publication,
  accordionId,
  sectionName,
  onContentChange,
  handleApiErrors,
}: ReleaseContentAccordionProps & ErrorControlProps) => {
  const [content, _setContent] = React.useState<ContentType[]>([]);

  const setContent = React.useCallback(
    (newContent: ContentType[]) => {
      if (onContentChange) onContentChange(newContent);
      _setContent(newContent);
    },
    [onContentChange],
  );

  const onReorder = React.useCallback(
    async (ids: Dictionary<number>) => {
      const newContent = await releaseContentService
        .updateContentSectionsOrder(releaseId, ids)
        .catch(handleApiErrors);

      setContent(newContent);
    },
    [releaseId, setContent, handleApiErrors],
  );

  React.useEffect(() => {
    const f = async (rid: string) => {
      const newContent = await releaseContentService
        .getContentSections(rid)
        .catch(handleApiErrors);

      if (releaseId === rid) setContent(newContent);
    };
    f(releaseId);
  }, [releaseId, handleApiErrors, setContent]);

  const onAddSection = React.useCallback(async () => {
    const newContent: AbstractRelease<EditableContentBlock>['content'] = [
      ...content,
      await releaseContentService
        .addContentSection(releaseId, content.length)
        .catch(handleApiErrors),
    ];

    setContent(newContent);
  }, [content, releaseId, setContent, handleApiErrors]);

  const onUpdateHeading = React.useCallback(
    async (block: ContentType, index: number, newTitle: string) => {
      let result;
      if (block.id) {
        result = await releaseContentService
          .updateContentSectionHeading(releaseId, block.id, newTitle)
          .catch(handleApiErrors);

        const newContent = [...content];
        newContent[index].heading = newTitle;
        setContent(newContent);
      }
      return result;
    },
    [content, releaseId, setContent, handleApiErrors],
  );

  const updateContentSection = React.useCallback(
    (index: number, contentBlock?: EditableContentBlock[]) => {
      const newContent = [...content];
      newContent[index].content = contentBlock;
      setContent(newContent);
    },
    [content, setContent],
  );

  return (
    <>
      <Accordion
        id={accordionId}
        canReorder
        sectionName={sectionName}
        onSaveOrder={onReorder}
        onAddSection={onAddSection}
      >
        {content.map((contentItem, index) => (
          <ReleaseContentAccordionSection
            id={contentItem.id}
            key={contentItem.order}
            contentItem={contentItem}
            index={index}
            publication={publication}
            onHeadingChange={title =>
              onUpdateHeading(contentItem, index, title)
            }
            onContentChange={newContent =>
              updateContentSection(index, newContent)
            }
          />
        ))}
      </Accordion>
    </>
  );
};

export default withErrorControl(ReleaseContentAccordion);
