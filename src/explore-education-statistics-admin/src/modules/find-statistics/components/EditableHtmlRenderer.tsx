import WysiwygEditor from '@admin/components/WysiwygEditor';
import { RendererProps } from '@admin/modules/find-statistics/PublicationReleaseContent';
import React from 'react';
import wrapEditableComponent from '@common/modules/find-statistics/util/wrapEditableComponent';
import { EditingContentBlockContext } from '@admin/modules/find-statistics/components/EditableContentBlock';
import ContentService from '@admin/services/release/edit-release/content/service';

export type MarkdownRendererProps = RendererProps & { source: string };

const EditableHtmlRenderer = ({ contentId, source }: MarkdownRendererProps) => {
  const [html, setHtml] = React.useState(source);

  const editingContext = React.useContext(EditingContentBlockContext);

  return (
    <>
      <WysiwygEditor
        content={html || ''}
        editable
        onContentChange={async ss => {
          if (
            editingContext.releaseId &&
            editingContext.sectionId &&
            contentId
          ) {
            const { body } = await ContentService.updateContentSectionBlock(
              editingContext.releaseId,
              editingContext.sectionId,
              contentId,
              {
                body: ss,
              },
            );

            setHtml(body);
          }
        }}
      />
    </>
  );
};

const HtmlRenderer = ({ source }: { source: string }) => {
  return (
    <div
      // eslint-disable-next-line react/no-danger
      dangerouslySetInnerHTML={{ __html: source }}
    />
  );
};

export default wrapEditableComponent(EditableHtmlRenderer, HtmlRenderer);
