import { ErrorControlContext } from '@admin/components/ErrorBoundary';
import WysiwygEditor from '@admin/components/WysiwygEditor';
import { RendererProps } from '@admin/modules/find-statistics/PublicationReleaseContent';
import React, { useContext } from 'react';
import wrapEditableComponent from '@common/modules/find-statistics/util/wrapEditableComponent';
import { EditingContentBlockContext } from '@admin/modules/find-statistics/components/EditableContentBlock';
import { releaseContentService } from '@admin/services/release/edit-release/content/service';

export type Props = RendererProps & {
  source: string;
  canDelete: boolean;
  onDelete: () => void;
  editable?: boolean;
};

const EditableHtmlRenderer = ({
  contentId,
  source,
  canDelete,
  onDelete,
  editable = true,
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
            const { body } = await releaseContentService
              .updateContentSectionBlock(
                editingContext.releaseId,
                editingContext.sectionId,
                contentId,
                {
                  body: ss,
                },
              )
              .catch(handleApiErrors);

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
