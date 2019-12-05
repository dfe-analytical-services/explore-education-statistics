import WysiwygEditor from '@admin/components/WysiwygEditor';
import { RendererProps } from '@admin/modules/find-statistics/PublicationReleaseContent';
import React from 'react';
import wrapEditableComponent from '@common/modules/find-statistics/util/wrapEditableComponent';
import { EditingContentBlockContext } from '@admin/modules/find-statistics/components/EditableContentBlock';
import ContentService from '@admin/services/release/edit-release/content/service';

export type Props = RendererProps & {
  source: string;
  canDelete: boolean;
  onDelete: () => void;
  editable?: boolean;
  onContentChange?: (content: string) => void;
};

const EditableHtmlRenderer = ({
  contentId,
  source,
  canDelete,
  onDelete,
  editable = true,
  onContentChange,
}: Props) => {
  const [html, setHtml] = React.useState(source);

  const editingContext = React.useContext(EditingContentBlockContext);

  return (
    <>
      <WysiwygEditor
        content={html || ''}
        editable={editable}
        canDelete={canDelete}
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
            if (onContentChange) onContentChange(body);
            setHtml(body);
          }
        }}
        onDelete={onDelete}
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
