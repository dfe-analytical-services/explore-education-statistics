import WysiwygEditor from '@admin/components/WysiwygEditor';
import { ErrorControlContext } from '@admin/contexts/ErrorControlContext';
import { EditingContentBlockContext } from '@admin/modules/find-statistics/components/EditableContentBlocks';
import { RendererProps } from '@admin/modules/find-statistics/PublicationReleaseContent';
import { releaseContentService } from '@admin/services/release/edit-release/content/service';
import wrapEditableComponent from '@common/modules/find-statistics/util/wrapEditableComponent';
import React, { useContext } from 'react';

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

  const { handleApiErrors } = useContext(ErrorControlContext);

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
            try {
              const {
                body,
              } = await releaseContentService.updateContentSectionBlock(
                editingContext.releaseId,
                editingContext.sectionId,
                contentId,
                {
                  body: ss,
                },
              );

              if (onContentChange) onContentChange(body);
              setHtml(body);
            } catch (err) {
              handleApiErrors(err);
            }
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
